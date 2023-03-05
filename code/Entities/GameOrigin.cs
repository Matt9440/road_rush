namespace RoadRush.Entities;

[HammerEntity]
public partial class GameOrigin : Entity
{
	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}
}
