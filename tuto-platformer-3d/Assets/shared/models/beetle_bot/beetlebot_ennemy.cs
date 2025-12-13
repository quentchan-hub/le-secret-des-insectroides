using Godot;
using System; 
 
public partial class beetlebot_ennemy : CharacterBody3D
{
	int life = 2; 
	[Export]
	public Node3D[] points;
	int actualPoint = 0;
	[Export] float speed = 0.5f;
	[Export] AnimationPlayer animPlayer;
	bool canWait = true;
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		life--;
		if (life <= 0)
			QueueFree();
	}
	
	public override void _Ready()
	{
		animPlayer.Play("walk");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var dir = (points[actualPoint].Position - Position).Normalized();
		dir.Y = 0;
		
		if (Position.DistanceTo(points[actualPoint].Position) >= 0.5f)
		{
			animPlayer.Play("walk");
			Velocity = dir * speed;
			MoveAndSlide(); 
		} else
		{
			if (canWait)
				WaitBeforeContinue();
		}
		
		LookAt(points[actualPoint].GlobalTransform.Origin, Vector3.Up);
		RotateObjectLocal(Vector3.Up, 3.14f);
	}
	
	private async void WaitBeforeContinue()
	{
		canWait = false;
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		GD.Print("3 second delay!");
		actualPoint++;
		if (actualPoint > points.Length - 1)
		{
			actualPoint = 0;
		}
		canWait = true;
	}

	private void _on_area_3d_2_area_entered(Area3D area)
	{
		if (area.GetParent().HasMethod("TakeDamages"))
		{
			area.GetParent().Call("TakeDamages");
		}
	}
	
}
