namespace RoadRush.Entities;

[HammerEntity, DrawAngles]
public partial class VehicleSpawner : Entity
{
	[Net]
	public TimeSince TimeSinceLastSpawn { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		TimeSinceLastSpawn = 0;
	}

	public void SpawnVehicle()
	{
		var spawnOccupied = VehicleEntity.HasCollided( Position, Rotation );

		if ( spawnOccupied is not null )
			return;

		var vehicle = VehicleEntity.FromPrefab( "prefabs/car.prefab" );
		vehicle.Transform = Transform;
		vehicle.PointsMultiplier = Math.Clamp( (float)GameRound.Instance?.Points / 30, 1, 5 );

		if ( GameRound.Instance is not null )
		{
			GameRound.Instance.TotalVehiclesSpawned += 1;
			vehicle.VehicleId = GameRound.Instance.TotalVehiclesSpawned;
		}

		TimeSinceLastSpawn = 0;
	}
}
