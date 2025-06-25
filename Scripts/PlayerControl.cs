using Godot;
using System;
using Game.Interfaces;
using Game.Enums;
using System.Collections.Generic;

public partial class PlayerControl : CharacterBody3D, IOcupanteTile
{
	public Vector3I PosicaoNaGrid { get; set; }

	[Export] public float Speed = 6.0f; // tiles/segundo
	[Export] public float Gravity = 9.8f;
	[Export] public float RotationSpeed = 30.0f;
	[Export] public float PressThreshold = 0.13f;

	private CameraRig cameraRig;
	private GridManager gridManager;
	private bool isMovingTile = false;

	private Vector3 destinoMundo;
	private Vector3I destinoGrid;
	private float targetYaw = 0f;

	private Dictionary<string, float> inputPressTime = new();
	private string currentKey = null;
	private Vector3I inputDirection = Vector3I.Zero;

	public override void _Ready()
	{
		cameraRig = GetTree().Root.FindChild("Camera_Position", true, false) as CameraRig;
		gridManager = GetTree().Root.FindChild("GridManager", true, false) as GridManager;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Se estiver em movimento, permitir que termine mesmo com câmera destravada
		if (isMovingTile)
		{
			// Continua interpolando até o destino
			Position = Position.MoveToward(destinoMundo, Speed * (float)delta);
			if (Position.DistanceTo(destinoMundo) < 0.01f)
			{
				Position = destinoMundo;
				AtualizarPosicaoNaGrid(destinoGrid);
				isMovingTile = false;

				// Só tenta mover de novo se a câmera estiver travada
				if (cameraRig != null && cameraRig.IsLocked && currentKey != null && Input.IsActionPressed(currentKey))
					TentarMover(inputDirection, currentKey);
			}
			return;
		}

		// Não faz mais nada se a câmera estiver destravada e não estamos movendo
		if (cameraRig == null || !cameraRig.IsLocked)
		{
			Velocity = Vector3.Zero;
			MoveAndSlide();
			return;
		}

		// Atualiza input e aplica rotação
		UpdateInputTimers();

		float currentYaw = Rotation.Y;
		currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, RotationSpeed * (float)delta);
		Rotation = new Vector3(0, currentYaw, 0);

		HandlePlayerMovement();
	}


	private void UpdateInputTimers()
	{
		var keys = new[] { "Walk_Left", "Walk_Right", "Walk_Front", "Walk_Back" };
		foreach (var key in keys)
		{
			if (Input.IsActionPressed(key))
			{
				if (!inputPressTime.ContainsKey(key))
					inputPressTime[key] = 0f;

				inputPressTime[key] += (float)GetProcessDeltaTime();
			}
			else
			{
				inputPressTime.Remove(key);
			}
		}
	}

	private void HandlePlayerMovement()
	{
		if (isMovingTile || cameraRig == null || !cameraRig.IsLocked)
			return;

		string pressedKey = null;
		if (Input.IsActionPressed("Walk_Front")) pressedKey = "Walk_Front";
		else if (Input.IsActionPressed("Walk_Back")) pressedKey = "Walk_Back";
		else if (Input.IsActionPressed("Walk_Left")) pressedKey = "Walk_Left";
		else if (Input.IsActionPressed("Walk_Right")) pressedKey = "Walk_Right";

		if (pressedKey == null)
	return;

		// Aplica rotação mesmo que não vá se mover ainda
		switch (pressedKey)
		{
			case "Walk_Front":
				targetYaw = Mathf.DegToRad(0);
				break;
			case "Walk_Back":
				targetYaw = Mathf.DegToRad(180);
				break;
			case "Walk_Left":
				targetYaw = Mathf.DegToRad(-90);
				break;
			case "Walk_Right":
				targetYaw = Mathf.DegToRad(90);
				break;
		}

		// Só move se tempo pressionado for suficiente
		if (!inputPressTime.ContainsKey(pressedKey) || inputPressTime[pressedKey] < PressThreshold)
			return;


		Vector3I direction = Vector3I.Zero;

		switch (pressedKey)
		{
			case "Walk_Front":
				direction.Z -= 1;
				targetYaw = Mathf.DegToRad(0);
				break;
			case "Walk_Back":
				direction.Z += 1;
				targetYaw = Mathf.DegToRad(180);
				break;
			case "Walk_Left":
				direction.X += 1;
				targetYaw = Mathf.DegToRad(-90);
				break;
			case "Walk_Right":
				direction.X -= 1;
				targetYaw = Mathf.DegToRad(90);
				break;
		}

		TentarMover(direction, pressedKey);
	}

	private void TentarMover(Vector3I input, string key)
	{
		Vector3I posAtual = PosicaoNaGrid;
		Vector3I posDestino = posAtual + input;

		Tile tileDestino = gridManager.GetTile(posDestino);
		if (tileDestino == null || !gridManager.OcuparTile(posDestino, this))
			return;

		gridManager.LiberarTile(posAtual);
		destinoGrid = posDestino;
		destinoMundo = tileDestino.Position;
		isMovingTile = true;

		inputDirection = input;
		currentKey = key;

		inputPressTime.Remove(key);
	}

	public void AtualizarPosicaoNaGrid(Vector3I novaPosicao)
	{
		PosicaoNaGrid = novaPosicao;
	}
}


