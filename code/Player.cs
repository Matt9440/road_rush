namespace RoadRush;

partial class Player : AnimatedEntity
{
	[ClientInput]
	public Ray MouseWorldRay { get; protected set; }

	[ClientInput]
	public Vector3 MouseWorldPosition { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceMouseDown { get; protected set; }

	[Net, Predicted]
	public VehicleEntity LastClickedVehicle { get; set; }

	[Net]
	public Vector3 LastClickPosition { get; set; }

	public ModelEntity ClientCursor { get; set; }

	private float TurnSpeed { get; set; } = 4f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		// Dress the player
		_ = new ModelEntity( "models/citizen_clothes/hat/police_hat/models/police_hat.vmdl_c", this );
		_ = new ModelEntity( "models/citizen_clothes/jacket/hivis_jacket/models/hivis_jacket.vmdl_c", this );
		_ = new ModelEntity( "models/citizen_clothes/shirt/polo_shirt/models/polo_shirt.vmdl_c", this );
		_ = new ModelEntity( "models/citizen_clothes/trousers/smarttrousers/smarttrousers.vmdl_c", this );
		_ = new ModelEntity( "models/citizen_clothes/shoes/boots/models/black_boots.vmdl_c", this );

		// Scale the player up a bit.
		Scale = 1.5f;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		SetupCamera();

		Game.RootPanel = new Hud();

		SetupCursor();
	}

	private void SetupCursor()
	{
		ClientCursor = new ModelEntity( "models/editor/arrow.vmdl_c" );
		ClientCursor.Rotation = Rotation.LookAt( Vector3.Down );
		ClientCursor.Scale = 4f;
		ClientCursor.EnableShadowCasting = false;
		ClientCursor.RenderColor = Color.Red;
	}

	private void SetupCamera()
	{
		var gameOrigin = RoadRush.GetGameOrigin();
		var zOffset = 1000f;
		var yOffset = 800f;
		var xOffset = 800f;

		Camera.Position = gameOrigin.Position + (Vector3.Up * zOffset) + (Vector3.Backward * yOffset) + (Vector3.Right * xOffset);
		Camera.Rotation = Rotation.LookAt( gameOrigin.Position - Camera.Main.Position );
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( 50 );

		Camera.Main.Ortho = true;
		Camera.Main.OrthoWidth = Screen.Height * 1.5f;
		Camera.Main.OrthoHeight = Screen.Height;

		// We want the sound to be listened to by the Pawn and not the Camera
		Sound.Listener = Transform;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		LookAtMouse();

		var trace = Trace.Ray( MouseWorldRay, 2000f ).Run();

		if ( ClientCursor is not null )
		{
			if ( GameRound.Instance?.State == GameState.Started )
			{
				ClientCursor.RenderColor = Color.Red;
				ClientCursor.Position = MouseWorldPosition + Vector3.Up * 70f;

				// Tell the vehicle we've hovered it, if it is a vehicle.
				(trace.Entity as VehicleEntity)?.OnHovered();
			}
			else
				ClientCursor.RenderColor = Color.Transparent;
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			TimeSinceMouseDown = 0f;

			LastClickPosition = trace.HitPosition;
			LastClickedVehicle = trace.Entity as VehicleEntity;

			return;
		}

		if ( LastClickedVehicle is null )
			return;

		// Click
		if ( Input.Released( InputButton.PrimaryAttack ) && TimeSinceMouseDown < 0.15f )
		{
			LastClickedVehicle.OnClick();

			if ( Game.IsClient )
				Sandbox.Services.Stats.Increment( "stopped", 1 );

			LastClickedVehicle = null;

			return;
		}

		// Drag 
		var dragDistance = Vector3.DistanceBetween( LastClickPosition, trace.HitPosition );

		if ( Input.Released( InputButton.PrimaryAttack ) && TimeSinceMouseDown > 0.45f || dragDistance > 50 )
		{
			LastClickedVehicle.OnDragged();

			if ( Game.IsClient )
				Sandbox.Services.Stats.Increment( "dragged", 1 );

			LastClickedVehicle = null;

			return;
		}
	}

	private Ray GetMouseRay() => Screen.GetOrthoRay( Mouse.Position );

	private Trace GetMouseTrace() => Trace.Ray( MouseWorldRay, 2000f );

	private void LookAtMouse()
	{
		var yaw = Rotation.LookAt( MouseWorldPosition - Position ).Yaw();
		yaw = (int)Math.Round( yaw / 100 ) * 90;

		Rotation = Rotation.Lerp( Rotation, Rotation.FromYaw( yaw ), TurnSpeed * Time.Delta );
	}

	public override void BuildInput()
	{
		MouseWorldRay = GetMouseRay();
		MouseWorldPosition = GetMouseTrace().Run().HitPosition;
	}
}
