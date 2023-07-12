namespace RoadRush.Entities;

[AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
[Solid, VisGroup( VisGroup.Trigger ), HideProperty( "enable_shadows" )]
[Library( "rr_trigger_points" ), HammerEntity]
public partial class AwardPointsTrigger : BaseTrigger
{
	public override void Spawn()
	{
		base.Spawn();

		ActivationTags.Add( "vehicle" );
	}

	public override void OnTouchStart( Entity other )
	{
		base.OnTouchStart( other );

		if ( other is not VehicleEntity vehicle )
			return;

		// Award the player a point.
		GameRound.AwardPoints( 1 );

		UpdatePointStats();

		// Destroy the vehicle.
		vehicle.Delete();
	}

	[ClientRpc]
	public static void UpdatePointStats()
	{
		Sandbox.Services.Stats.Increment( "points", 1 );
	}
}
