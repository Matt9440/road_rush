using Sandbox.Component;

namespace RoadRush.Entities;

public enum VehicleType
{
	Car,
	Bus,
	Bike
}

[Prefab]
public partial class VehicleEntity : AnimatedEntity
{
	[Net, Predicted]
	public float TravelSpeed { get; set; }

	[Net, Prefab]
	public VehicleType Type { get; set; }

	[Net, Prefab]
	public float DefaultSpeed { get; set; }

	[Prefab]
	public List<string> PossibleSkins { get; set; }

	[Net]
	public float PointsMultiplier { get; set; } = 1f;

	public float DragSpeedMultiplier { get; set; } = 1f;

	[Net]
	public TimeSince TimeSinceVehicleSpawned { get; set; }

	[Net]
	public bool IsFrozen { get; set; }

	[Net]
	public bool HasBeenDragged { get; set; }

	[Net]
	public bool VehicleCollided { get; set; }

	private List<Particles> CreatedParticles { get; set; } = new();

	private Sound EngineSound { get; set; }

	private Glow GlowComponent { get; set; }

	private TimeSince TimeSinceLastHovered { get; set; }

	private int VehiclesExisted { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		SetMaterialOverride( Material.Load( Game.Random.FromList( PossibleSkins ) ), "skin" );
		EnableAllCollisions = true;

		Tags.Add( "vehicle" );
		Tags.Add( "solid" );

		TravelSpeed = DefaultSpeed;
		TravelSpeed *= Game.Random.Float( 1, 1.3f );

		TimeSinceVehicleSpawned = 0;

		// Effects
		CreatedParticles.Add( Particles.Create( "particles/vehicle_exhaust.vpcf", this, "exhaust" ) );
		EngineSound = PlaySound( "engine_hum" );
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		VehiclesExisted += 1;

		// Instructions for freezing vehicles.
		if ( VehiclesExisted == 1 )
			DebugOverlay.Text( "I am the first vehicle", Position, Color.Red, 20 );

		// Instructions for dragging vehicles.
		if ( VehiclesExisted == 2 )
			DebugOverlay.Text( "I am the second vehicle", Position, Color.Red, 20 );
	}

	[Event.Tick]
	public void OnTick()
	{
		if ( TimeSinceLastHovered > 0.1f && GlowComponent is not null )
			GlowComponent.Enabled = false;

		if ( !Game.IsServer )
			return;

		if ( GameRound.Instance?.State == GameState.Ended || VehicleCollided )
			return;

		var collisionPosition = HasCollided( Position, Rotation, this );

		if ( collisionPosition is not null )
		{
			VehicleCollided = true;

			// Create particles
			CreatedParticles.Add( Particles.Create( "particles/crash_smoke.vpcf", collisionPosition.Value ) );
			CreatedParticles.Add( Particles.Create( "particles/crash_fire.vpcf", collisionPosition.Value ) );

			PlaySound( "vehicle_collision" );
			PlaySound( "vehicle_explosion" );

			GameRound.Instance?.End();
		}

		if ( IsFrozen )
			return;

		var travelSpeed = TravelSpeed * PointsMultiplier;
		travelSpeed *= DragSpeedMultiplier;

		// Slow down speed to accomodate for the vehicle infront, if the vehicle is moved we can speed up again.
		travelSpeed = SlowForVehicleInfront( travelSpeed );

		Position = Position.LerpTo( Position + Rotation.Forward * 10f, travelSpeed * Time.Delta );
	}

	/// <summary>
	/// Reduce speed for any vehicles infront of this vehicle, returns the original speed if no vehicles are infront.
	/// </summary>
	/// <param name="currentSpeed"></param>
	/// <returns></returns>
	private float SlowForVehicleInfront( float currentSpeed )
	{
		var verticalOffset = Vector3.Up * 20;
		var forwardTrace = Trace.Ray( Position + verticalOffset, Position + verticalOffset + Rotation.Forward * 200f ).WithTag( "vehicle" ).Ignore( this ).Run();

		if ( forwardTrace.Entity?.Rotation.Forward == Rotation.Forward )
		{
			var vehicleInfront = forwardTrace.Entity as VehicleEntity;

			if ( vehicleInfront is not null )
				return vehicleInfront.TravelSpeed;
		}

		return currentSpeed;
	}

	public static Vector3? HasCollided( Vector3 position, Rotation rotation, Entity ignore = null )
	{
		var width = 85;
		var length = 170;
		var height = 80;
		var mins = rotation.Right * width / 2 + rotation.Backward * length / 2 + Vector3.Up * height;
		var maxs = rotation.Left * width / 2 + rotation.Forward * length / 2;

		var bboxTrace = Trace.Box( 1, position + mins, position + maxs ).WithTag( "vehicle" ).Ignore( ignore ).Run();

		if ( bboxTrace.Entity is not null )
			return bboxTrace.HitPosition;
		else
			return null;
	}

	public virtual void OnHovered()
	{
		var glow = Components.GetOrCreate<Glow>();

		GlowComponent = glow;
		GlowComponent.Color = Color.White;
		GlowComponent.Width = 0.5f;

		GlowComponent.Enabled = true;

		TimeSinceLastHovered = 0;
	}

	public virtual void OnClick()
	{
		if ( !IsFrozen && TimeSinceVehicleSpawned < 1 )
			return;

		IsFrozen = !IsFrozen;
		HasBeenDragged = false;

		PlaySound( "tyre_skid" );
	}

	public virtual void OnDragged()
	{
		HasBeenDragged = true;
		IsFrozen = false;

		PlaySound( "engine_rev" );

		DragSpeedMultiplier = 10f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		foreach ( var particle in CreatedParticles )
			particle.Destroy( true );

		EngineSound.Stop();
	}

	public static VehicleEntity FromPrefab( string prefabName )
	{
		if ( PrefabLibrary.TrySpawn<VehicleEntity>( prefabName, out var vehicle ) )
			return vehicle;

		return null;
	}
}
