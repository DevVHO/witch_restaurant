using Godot;
using System;
using Game.Interfaces;
using Game.Enums;
using System.Collections.Generic;

public partial class PlayerControl : CharacterBody3D, IOcupanteTile
{
	public Vector3I PosicaoNaGrid { get; set; }

	//Elementos de efeito de animação
	[Export] public float Speed = 6.0f; // tiles/segundo
	[Export] public float Gravity = 9.8f;
	[Export] public float RotationSpeed = 30.0f;
	[Export] public float PressThreshold = 0.13f;
	private float targetYaw;
	private string currentKey = null;
	private Dictionary<string, float> inputPressTime = new();
	//Outras classes
	private CameraRig cameraRig;
	private GridManager gridManager;
	//Interacts
	public Item itemNaMao;
	public Node3D handPoint;
	private Tile tileInteragido = null;

	//Condições de Movimentação
	private bool isMovingTile = false;
	public Vector3I Direcao { get; set; } = Vector3I.Forward;
	private Vector3I inputDirection = Vector3I.Zero;
	//Comunicação para movimentar
	private Vector3 destinoMundo;
	private Vector3I destinoGrid;
	//Interação -parte visual
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
		if (Input.IsActionJustReleased("Interact"))
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
	#region Interação
	public bool MaoVazia() => itemNaMao == null;
	public Item ItemNaMao
	{
		get => itemNaMao;
		set => itemNaMao = value;
	}
	private void Interact()
	{
		Vector3I posFrente = PosicaoNaGrid + Direcao;
		Tile tileAlvo = gridManager.GetTile(posFrente);

		if (tileAlvo.Ocupante is Balcao balcao)
		{
			// Buscar bandeja dentro do balcão
			Bandeja bandeja = balcao.ObterBandejaNoBalcao();
			if (bandeja != null && itemNaMao is Copo copo)
			{
				if (bandeja.AdicionarItem(copo))
				{
					itemNaMao = null;
					GD.Print("Copo colocado na bandeja via balcão.");
					return;
				}
			}

			// Caso não tenha bandeja ou não colocou o copo, tenta a lógica do balcão normal
			if (MaoVazia() && balcao.PossuiItem())
			{
				var item = balcao.RetirarItem();
				ReceberItem(item);
				GD.Print("Item pego do balcão.");
			}
			else if (!MaoVazia() && !balcao.PossuiItem())
			{
				var item = EntregarItem();
				balcao.ReceberItem(item);
				GD.Print("Item colocado no balcão.");
			}
			else
			{
				GD.Print("Interação inválida com o balcão.");
			}
			return;
		}

		var objeto = GetOcupanteNaFrente();
		if (objeto is Bandeja bandejaFrente && itemNaMao is Copo copoMao)
		{
			if (bandejaFrente.AdicionarItem(copoMao))
			{
				itemNaMao = null;
				return;
			}
		}

		if (tileAlvo.Ocupante is IInteragivel interagivel)
		{
			interagivel.Interact(this);
			return;
		}

	}



	public void ReceberItem(Item item)
	{
		if (itemNaMao != null)
		{
			return;
		}

		if (handPoint == null)
			handPoint = GetNodeOrNull<Node3D>("HandPoint");

		// Remove de onde estiver antes de adicionar
		if (item.GetParent() != null)
			item.GetParent().RemoveChild(item);

		handPoint.AddChild(item);
		item.GlobalTransform = handPoint.GlobalTransform;
		item.Scale = Vector3.One;

		itemNaMao = item;
	}

	public Node3D GetOcupanteNaFrente()
	{
		Vector3I posFrente = PosicaoNaGrid + Direcao;
		Tile tile = gridManager.GetTile(posFrente);
		return tile?.Ocupante as Node3D;
	}
	public Item EntregarItem()
	{
		if (itemNaMao == null) return null;

		var item = itemNaMao;
		itemNaMao = null;

		if (item.GetParent() != null)
			item.GetParent().RemoveChild(item);

		return item;
	}

	public bool PossuiItem()
	{
		return itemNaMao != null;
	}
	public void AtualizarPosicaoNaGrid(Vector3I novaPosicao)
	{
		PosicaoNaGrid = novaPosicao;
	}
	#endregion interação

//Visualização de Interação
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
			GD.Print(tile.GetType());
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


