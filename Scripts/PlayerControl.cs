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
	private Node3D itemNaMao;
	private Node3D handPoint;
	private bool isMovingTile = false;

	private Vector3 destinoMundo;
	private Vector3I destinoGrid;
	private float targetYaw = 0f;

	private Dictionary<string, float> inputPressTime = new();
	private string currentKey = null;
	public Vector3I Direcao { get; set; } = Vector3I.Right;
	private Vector3I inputDirection = Vector3I.Zero;
	//Interação -parte visual
	private Tile tileInteragido = null;
	private Color corOriginalTile;
	private bool interactPressedLastFrame = false;
	private TileState estadoAnteriorTile;
	public override void _Ready()
	{
		cameraRig = GetTree().Root.FindChild("Camera_Position", true, false) as CameraRig;
		gridManager = GetTree().Root.FindChild("GridManager", true, false) as GridManager;
	}

	public override void _PhysicsProcess(double delta)
	{
		// Lida com cor do tile enquanto Interact estiver pressionado
		bool interactPressed = Input.IsActionPressed("Interact");

		if (interactPressed)
		{
			Vector3I posFrente = PosicaoNaGrid + Direcao;
			Tile tileAlvo = gridManager.GetTile(posFrente);
			if (tileAlvo != null)
			{
				if (tileInteragido != tileAlvo)
				{
					// Restaurar estado do tile anterior
					if (tileInteragido != null)
						tileInteragido.Estado = estadoAnteriorTile;

					tileInteragido = tileAlvo;
					estadoAnteriorTile = tileAlvo.Estado;  // salva estado atual
					tileAlvo.Estado = TileState.Interagindo; // muda para interagindo
				}
			}
		}
		else
		{
			if (tileInteragido != null)
			{
				// Quando soltar, restaura o estado anterior
				tileInteragido.Estado = estadoAnteriorTile;
				tileInteragido = null;
			}
		}


		interactPressedLastFrame = interactPressed;


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
		if (Input.IsActionJustPressed("Interact"))
		{
			Interact();
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
	public bool MaoVazia() => itemNaMao == null;

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

		// Verifica qual tecla foi pressionada
		string pressedKey = null;
		if (Input.IsActionPressed("Walk_Front")) pressedKey = "Walk_Front";
		else if (Input.IsActionPressed("Walk_Back")) pressedKey = "Walk_Back";
		else if (Input.IsActionPressed("Walk_Left")) pressedKey = "Walk_Left";
		else if (Input.IsActionPressed("Walk_Right")) pressedKey = "Walk_Right";

		if (pressedKey == null)
			return;

		// Define a direção e a rotação alvo com base na tecla
		Vector3I direction = Vector3I.Zero;
		switch (pressedKey)
		{
			case "Walk_Front":
				direction = Vector3I.Forward; // ou new Vector3I(0, 0, -1);
				targetYaw = Mathf.DegToRad(0);
				break;
			case "Walk_Back":
				direction = Vector3I.Back; // ou new Vector3I(0, 0, 1);
				targetYaw = Mathf.DegToRad(180);
				break;
			case "Walk_Left":
				direction = Vector3I.Right; // pois o grid está invertido horizontalmente
				targetYaw = Mathf.DegToRad(-90);
				break;
			case "Walk_Right":
				direction = Vector3I.Left; // idem
				targetYaw = Mathf.DegToRad(90);
				break;
		}

		// Atualiza a direção do jogador
		Direcao = direction;

		// Só move se tempo pressionado for suficiente
		if (!inputPressTime.ContainsKey(pressedKey) || inputPressTime[pressedKey] < PressThreshold)
			return;

		// Tenta mover na direção
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
	private void Interact()
	{
		Vector3I posFrente = PosicaoNaGrid + Direcao;

		Tile tileAlvo = gridManager.GetTile(posFrente);
		if (tileAlvo == null)
		{
			GD.Print("Nenhum tile na frente.");
			return;
		}

		if (tileAlvo.Ocupante is Balcao)
		{
			GD.Print("Tile à frente é um balcão.");
			SoltarItemEmBalcao(gridManager);
			return;
		}

		if (tileAlvo.Ocupante is IInteragivel interagivel)
		{
			GD.Print("Tile à frente é interagível.");
			interagivel.Interact(this);
			return;
		}

		GD.Print("Tile à frente não tem nada interagível.");
	}


	public void ReceberItem(Node3D item)
	{
		if (itemNaMao != null)
		{
			GD.Print("Já há um item na mão.");
			return;
		}

		// Garante que o ponto de ancoragem foi encontrado
		if (handPoint == null)
			handPoint = GetNodeOrNull<Node3D>("HandPoint");

		if (handPoint == null)
		{
			GD.PrintErr("HandPoint não encontrado no jogador.");
			return;
		}

		handPoint.AddChild(item);

		// Zera a transformação local do item (em relação ao HandPoint)
		item.Position = Vector3.Zero;
		item.Rotation = Vector3.Zero;
		item.Scale = Vector3.One;

		itemNaMao = item;
	}

	public void TentarInteragir()
	{
		Vector3I posFrente = PosicaoNaGrid + Direcao;
		var tile = gridManager.GetTile(posFrente);

		if (tile == null)
		{
			GD.Print("Não existe tile na frente.");
			return;
		}

		if (tile.Estado == TileState.Ocupado || tile.Estado == TileState.Interagindo)
		{
			KitchenObject objParaInteragir = gridManager.GetKitchenObjectNaPos(posFrente);
			if (objParaInteragir != null)
			{
				GD.Print("Interagindo com " + objParaInteragir.Name);
				if (objParaInteragir is IInteragivel interagivel)
				{
					interagivel.Interact(this);
				}
				else
				{
					GD.Print("Objeto não implementa IInteragivel.");
				}
			}
			else
			{
				GD.Print("Nenhum objeto para interagir no tile.");
			}
		}
		else
		{
			GD.Print("Tile não ocupado, nada para interagir.");
		}
	}

	public void DropItem()
	{
		if (itemNaMao == null) return;

		itemNaMao.GetParent().RemoveChild(itemNaMao);
		itemNaMao = null;
	}
	public Node3D GetOcupanteNaFrente()
	{
		Vector3I posFrente = PosicaoNaGrid + Direcao;
		Tile tile = gridManager.GetTile(posFrente);
		return tile?.Ocupante as Node3D;
	}
	public Node3D EntregarItem()
	{
		if (itemNaMao == null) return null;

		var item = itemNaMao;
		itemNaMao = null;

		// Remove o item da mão (desvincula do handPoint)
		item.GetParent()?.RemoveChild(item);

		return item;
	}


	public void SoltarItemEmBalcao(GridManager gridManager)
	{
		if (itemNaMao == null)
		{
			GD.Print("Nenhum item para soltar.");
			return;
		}

		Vector3I posFrente = PosicaoNaGrid + Direcao;
		GD.Print($"Tentando soltar item no tile à frente: {posFrente}");

		Tile tileAlvo = gridManager.GetTile(posFrente);
		if (tileAlvo == null)
		{
			GD.Print("Nenhum tile à frente.");
			return;
		}

		if (tileAlvo.Ocupante is Balcao balcao)
		{
			GD.Print("Tile à frente é um balcão.");

			if (balcao.PossuiItem())
			{
				GD.Print("O balcão já possui um item.");
				return;
			}

			GD.Print("Entregando item ao balcão...");
			balcao.ReceberItem(itemNaMao);
			itemNaMao = null;
		}
		else
		{
			GD.Print("O tile à frente não é um balcão.");
		}
	}
	public bool PossuiItem()
	{
		return itemNaMao != null;
	}
	public void AtualizarPosicaoNaGrid(Vector3I novaPosicao)
	{
		PosicaoNaGrid = novaPosicao;
	}
	private Material originalMaterial = null;

	private void GuardarCorOriginalETornarMaterial(Tile tile)
	{
		if (tile == null) return;

		// Supondo que o Tile tem um MeshInstance3D chamado "Visual"
		var mesh = tile.GetNodeOrNull<MeshInstance3D>("Visual");
		if (mesh != null)
		{
			originalMaterial = mesh.GetActiveMaterial(0);
		}
	}

	private void PintarTileDeVerde(Tile tile)
	{
		if (tile == null) return;

		var mesh = tile.GetNodeOrNull<MeshInstance3D>("Visual");
		if (mesh != null)
		{
			var mat = new StandardMaterial3D();
			mat.AlbedoColor = new Color(0, 1, 0); // Verde
			mesh.SetSurfaceOverrideMaterial(0, mat);
		}
	}

	private void RestaurarCorTile(Tile tile)
	{
		if (tile == null) return;

		var mesh = tile.GetNodeOrNull<MeshInstance3D>("Visual");
		if (mesh != null && originalMaterial != null)
		{
			mesh.SetSurfaceOverrideMaterial(0, originalMaterial);
		}
	}

}


