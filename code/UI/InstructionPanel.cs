namespace RoadRush.UI;

public class InstructionPanel : WorldPanel
{
	private Entity Following { get; set; }

	private Vector3 Offset { get; set; }

	public InstructionPanel( Entity parent, string instruction, Vector3? offset = null )
	{
		StyleSheet.Load( "./UI/InstructionPanel.scss" );

		PanelBounds = new Rect( -3000, -450, 6000, 900 );

		Add.Label( instruction );

		Following = parent;

		if ( offset.HasValue )
			Offset = offset.Value;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Following is null )
			return;

		Position = Following.Position + Offset;
		Rotation = Rotation.LookAt( (Camera.Position - Position).WithZ( 60 ) );
	}
}
