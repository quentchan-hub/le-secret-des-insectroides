using Godot;

public partial class AreaZoneMitraillee : Area3D
{
	public GameState gameState;

	[Export] private Timer fireTimer;
	private bool _playerIsHurt = false;
	private bool onfire = false;

	public override void _Ready()
	{
		gameState = GetNode<GameState>("/root/GameState");
		
		Visible = false;
		onfire = false;
	}

	public override void _Process(double delta)
	{
		if (!gameState.zoneFeu)
		{
			Visible = false;
			return;
		}

		if (gameState.zoneFeu && !onfire)
		{
			StartFireZone();
		}
	}

	private void StartFireZone()
	{
		Visible = true;
		onfire = true;
		fireTimer.Start();
	}

	private void OnFireTimerTimeout()
	{
		Visible = false;
		onfire = false;
	}

	public async void _on_area_body_entered(Node3D body)
	{
		if (body is PlayerBotCtrl playerBot && !_playerIsHurt)
		{
			playerBot.TakeDamages();
			_playerIsHurt = true;
			await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
			_playerIsHurt = false;
		}
	}
}
