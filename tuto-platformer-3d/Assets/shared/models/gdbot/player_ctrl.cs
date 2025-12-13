using Godot;
using System;

public partial class player_ctrl : CharacterBody3D
{
	[Export] int life = 3;
	[Export] float speed = 20f;
	[Export] float acceleration = 15f;
	[Export] float airAcceleration = 5f;
	[Export] float gravity = 1f;
	[Export] float maxTerminalVelocity = 55f;
	[Export] float jumpForce = 20f;
	
	[Export(PropertyHint.Range, "0.1,1.0")] 
	float mouseSensivity = 0.3f;
	[Export(PropertyHint.Range, "-90,0,1")] 
	float minPitch = 90f;
	[Export(PropertyHint.Range, "0,90,1")] 
	float maxPitch = 90f;
	bool bounce = false;
	
	Vector3 velocity;
	float yVelocity;
	[Export]Node3D cameraPivot;
	[Export]Camera3D camera;
	[Export]AnimationPlayer animationPlayer;
	
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		animationPlayer.Play("Idle");
		
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}
	
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motionEvent)
		{
			Vector3 rotDeg = RotationDegrees;
			rotDeg.Y -= motionEvent.Relative.X * mouseSensivity;
			RotationDegrees = rotDeg;
			
			rotDeg = cameraPivot.RotationDegrees;
			rotDeg.X -= motionEvent.Relative.Y * -mouseSensivity;
			rotDeg.X = Mathf.Clamp(rotDeg.X, minPitch, maxPitch);
			cameraPivot.RotationDegrees = rotDeg;
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
	}
	
	private void HandleMovement(double delta)
	{
		Vector3 direction = new Vector3(0,0,0);
		
		if (Input.IsActionPressed("move_up"))
			direction += Transform.Basis.Z;
		if (Input.IsActionPressed("move_down"))
			direction -= Transform.Basis.Z;
		if (Input.IsActionPressed("move_left"))
			direction += Transform.Basis.X;
		if (Input.IsActionPressed("move_right"))
			direction -= Transform.Basis.X;
		
		direction = direction.Normalized();

		float accel = IsOnFloor() ? acceleration : airAcceleration;
		//velocity = velocity.lerp(direction * speed, accel * delta);
		velocity = direction * speed * accel; 
		
		if (bounce)
		{
			yVelocity = jumpForce;
			bounce = false;
		}
		else
		{
			if (IsOnFloor())
			{
				yVelocity = -0.01f; 
			}
			else
			{
				yVelocity = Mathf.Clamp(yVelocity-gravity, -maxTerminalVelocity, maxTerminalVelocity);
				animationPlayer.Play("fall");
			}
		}
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			yVelocity = jumpForce;
		
		velocity.Y = yVelocity;
		Velocity = velocity;
		MoveAndSlide();
		
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			// GD.Print("I collided with ", ((Node)collision.GetCollider()).Name);
		}
		
		if (direction != new Vector3(0,0,0) && IsOnFloor())
		{
			animationPlayer.Play("walk");
		} // Idle, fall, walk, jump, run, ground_impact, simple_punch
		else if (direction == new Vector3(0,0,0) && IsOnFloor())
		{
			animationPlayer.Play("Idle");
		}
	}
	
	private void _on_area_3d_body_entered(Node3D body)
	{
		if (body.Name == "BeetlebotSkin")
		{
			bounce = true;
		}
	}
	
	public void TakeDamages()
	{
		GD.Print("TakeDamages!");
		
		life --;
		
		if (life <= 0)
			GD.Print("Dead!");
	}

}
