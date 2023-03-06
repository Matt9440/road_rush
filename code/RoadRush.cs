global using Editor;
global using RoadRush.Entities;
global using RoadRush.UI;
global using Sandbox;
global using Sandbox.UI;
global using Sandbox.UI.Construct;
global using System;
global using System.Collections.Generic;
global using System.Linq;

namespace RoadRush;

public partial class RoadRush : GameManager
{
	public static RoadRush Instance { get; set; }

	[Net]
	public GameRound LiveGame { get; set; }

	public RoadRush()
	{
		Instance = this;
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;

		// Start a new game.
		NewGame();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		LiveGame?.Simulate( cl );
	}

	[ConCmd.Server]
	public static void NewGame()
	{
		Instance?.LiveGame?.End();

		Instance.LiveGame = new();
		Instance.LiveGame.Startup();
	}

	[ConCmd.Server]
	public static void EndGame()
	{
		Instance?.LiveGame?.End();
	}

	public static Transform GetGameOrigin() => All.OfType<GameOrigin>().FirstOrDefault().Transform;
}
