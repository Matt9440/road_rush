namespace RoadRush;

public partial class GameRound : BaseNetworkable
{
	public static GameRound Instance => RoadRush.Instance.LiveGame;

	[Net]
	public TimeSince TimeSinceStartup { get; set; }

	[Net]
	public TimeSince TimeSinceStarted { get; set; }

	[Net] TimeSince TimeSinceEnded { get; set; }

	[Net]
	public int Points { get; protected set; }

	[Net]
	public GameState State { get; protected set; }

	[Net]
	public int TotalVehiclesSpawned { get; set; }

	public TimeSince TimeSinceLastVehicleSpawn { get; set; }

	public GameRound()
	{
		State = GameState.Waiting;
		TimeSinceStartup = 0;
	}

	public void Startup()
	{
		// Cleanup any entities from the remaining round.
		Cleanup();

		State = GameState.Started;
		TimeSinceStarted = 0;
	}

	public void End()
	{
		State = GameState.Ended;
		TimeSinceEnded = 0;
	}

	private void Cleanup()
	{
		// Remove all vehicles
		foreach ( var entity in Entity.All.OfType<VehicleEntity>() )
			entity.Delete();
	}

	public static void AwardPoints( int amount )
	{
		if ( Instance is null )
			return;

		Instance.Points += amount;

		if ( Game.IsClient )
			Sandbox.Services.Stats.Increment( "points", 1 );
	}

	public void Simulate( IClient cl )
	{
		if ( !Game.IsServer )
			return;

		// Game startup
		if ( State == GameState.Waiting )
		{
			if ( TimeSinceStartup < Settings.StartDelay )
				return;
			else
				Startup();
		}

		// Don't do any of the below unless our state is set to Started.
		if ( State != GameState.Started )
			return;

		// Don't run the below if the last vehicle was spawned less than 5 seconds ago.
		// TODO: Improve this.

		var minRandom = Math.Clamp( 5f - (float)Points / 10, Game.Random.Float( 0.5f, 0.8f ), 4f );

		if ( TimeSinceLastVehicleSpawn < Game.Random.Float( minRandom, 5f ) )
			return;

		var spawners = Entity.All.OfType<VehicleSpawner>();

		if ( !spawners.Any() )
			return;

		var amountToPick = 1;

		// A chance to activate more than 1 spawner.
		var shouldPickTwo = Game.Random.Int( 0, 4 );

		if ( shouldPickTwo == 4 )
			amountToPick = 2;

		var shouldSpawnThree = Game.Random.Int( 0, 10 );

		if ( shouldSpawnThree == 10 )
			amountToPick = 3;

		var usedSpawners = spawners.ToList().OrderBy( _ => Game.Random.Next() ).Take( amountToPick );

		foreach ( var spawner in usedSpawners )
		{
			spawner.SpawnVehicle();
		}

		TimeSinceLastVehicleSpawn = 0;
	}
}
