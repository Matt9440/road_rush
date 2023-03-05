namespace RoadRush.Entities;

public class BillboardPanel : WorldPanel
{
	private Billboard Billboard { get; set; }

	public BillboardPanel( Billboard board )
	{
		StyleSheet.Load( "./Entities/Billboard/BillboardPanel.scss" );

		Position = board.Position;
		Rotation = board.Rotation;
		PanelBounds = new Rect( -3950, -2000, 7900, 4000 );

		var webPanel = new WebPanel();
		var surface = webPanel.Surface;
		surface.ScaleFactor = 0.5f;
		surface.Url = $"https://ninetyfour.dev/games/roadrush/billboard/";

		AddChild( webPanel );

		Billboard = board;
	}
}
