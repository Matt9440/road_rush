namespace RoadRush.Entities;

[HammerEntity]
[EditorModel( "models/sbox_props/cardboard_box/cardboard_box.vmdl_c" )]
[DrawAngles]
[Title( "Billboard" ), Icon( "assignment" )]
[Category( "World" )]
public partial class Billboard : AnimatedEntity
{
	private WorldPanel BillboardPanel { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		BillboardPanel = new BillboardPanel( this );
	}
}
