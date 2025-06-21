using Godot;
using System;

public partial class PlayerControl : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public float Gravity = 9.8f;

	[Export] public NodePath CameraRigPath;
	private CameraRig cameraRig;

	private Vector3 _velocity = Vector3.Zero;

	public override void _Ready()
	{
		if (CameraRigPath != null && !CameraRigPath.IsEmpty)
			cameraRig = GetNode<CameraRig>(CameraRigPath);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (cameraRig != null && !cameraRig.IsLocked)
		{
			// Câmera destravada: jogador parado
			_velocity.X = 0;
			_velocity.Z = 0;

			if (!IsOnFloor())
				_velocity.Y -= Gravity * (float)delta;
			else
				_velocity.Y = 0;

			Velocity = _velocity;
			MoveAndSlide();
			return;
			
		}
		
		HandlePlayerMovement(delta);
	}

	private void HandlePlayerMovement(double delta)
	{
		float inputX = 0.0f;
		float inputZ = 0.0f;

		foreach (string action in new[] { "Walk_Left", "Walk_Right", "Walk_Front", "Walk_Back" })
		{
			if (Input.IsActionPressed(action) && cameraRig.IsLocked)
			{
				switch (action)
				{
					case "Walk_Left":
						inputX += 1.0f;
						break;
					case "Walk_Right":
						inputX -= 1.0f;
						break;
					case "Walk_Front":
						inputZ -= 1.0f;
						break;
					case "Walk_Back":
						inputZ += 1.0f;
						break;
				}
			}
		}

		Vector3 direction = new Vector3(inputX, 0, inputZ).Normalized();
		direction = Transform.Basis * direction;
		direction.Y = 0;

		_velocity.X = direction.X * Speed;
		_velocity.Z = direction.Z * Speed;

		if (!IsOnFloor())
			_velocity.Y -= Gravity * (float)delta;
		else
			_velocity.Y = 0;

		Velocity = _velocity;
		MoveAndSlide();
	}
}
