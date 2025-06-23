using Godot;
using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Interfaces;

public partial class GridManager : Node3D
{
    [Export] public PackedScene TileScene { get; set; }
    [Export] public PackedScene JogadorScene { get; set; }
    private Dictionary<Vector3I, Tile> grid = new();

    private const int GRID_WIDTH = 15;
    private const int GRID_HEIGHT = 15;
    private const float TILE_SIZE = 1.0f; // Ajuste conforme a escala do seu mesh

    public override void _Ready()
    {
        GenerateGrid();
        Vector3I posicaoInicial = new Vector3I(6, 0, 14);
        InstanciarJogadorNaGrid(posicaoInicial);
    }

    public void GenerateGrid()
    {
        grid.Clear();

        // Instancia temporariamente o tile para medir o tamanho
        Tile tileTemp = TileScene.Instantiate<Tile>();
        AddChild(tileTemp); // Precisa estar na scene para que as propriedades de transformação funcionem

        // Supondo que o tile use um MeshInstance3D como filho direto
        Aabb bounds = tileTemp.GetChild<MeshInstance3D>(0).GetAabb();
        Vector3 tileSize = bounds.Size;

        RemoveChild(tileTemp);
        tileTemp.QueueFree(); // Remove o tile temporário da cena

        // Começa a instanciar os tiles reais com base no tamanho detectado
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int z = 0; z < GRID_HEIGHT; z++)
            {
                Vector3I gridPos = new Vector3I(x, 0, z);
                Vector3 worldPos = new Vector3(x * tileSize.X, 0, z * tileSize.Z);

                Tile tile = TileScene.Instantiate<Tile>();
                tile.Position = worldPos;
                tile.Posicao = gridPos;
                tile.Estado = TileState.Livre;
                tile.Ocupante = null;

                AddChild(tile);
                grid[gridPos] = tile;
            }
        }
    }


    public Tile GetTile(Vector3I pos)
    {
        return grid.TryGetValue(pos, out var tile) ? tile : null;
    }

    public List<Tile> GetVizinhos(Vector3I pos)
    {
        List<Tile> vizinhos = new();
        Vector3I[] direcoes = new Vector3I[]
        {
            new Vector3I(1, 0, 0),
            new Vector3I(-1, 0, 0),
            new Vector3I(0, 0, 1),
            new Vector3I(0, 0, -1)
        };

        foreach (Vector3I dir in direcoes)
        {
            Vector3I vizinhoPos = pos + dir;
            if (grid.ContainsKey(vizinhoPos))
                vizinhos.Add(grid[vizinhoPos]);
        }

        return vizinhos;
    }

    public bool OcuparTile(Vector3I pos, IOcupanteTile ocupante)
    {
        if (!grid.ContainsKey(pos)) return false;

        Tile tile = grid[pos];
        if (!tile.EstaLivre()) return false;

        tile.Estado = TileState.Ocupado;
        tile.Ocupante = ocupante;
        ocupante.PosicaoNaGrid = pos;

        return true;
    }

    public bool LiberarTile(Vector3I pos)
    {
        if (!grid.ContainsKey(pos))
            return false;

        Tile tile = grid[pos];
        tile.Estado = TileState.Livre;
        tile.Ocupante = null;
        return true;
    }
    public void InstanciarJogadorNaGrid(Vector3I gridPos)
    {
        if (!grid.TryGetValue(gridPos, out var tile))
        {
            GD.PrintErr("Tile inválido: " + gridPos);
            return;
        }

        var jogadorRaw = JogadorScene.Instantiate();
        if (jogadorRaw is not CharacterBody3D jogador)
        {
            GD.PrintErr("O jogador instanciado não é um CharacterBody3D!");
            return;
        }

        jogador.Position = tile.Position;
        AddChild(jogador);

        var target = GetTree().Root.FindChild("Target_Player2", true, false) as Node3D;
        if (target == null)
        {
            GD.PrintErr("Target_Player2 não encontrado!");
            return;
        }

        // Verificações defensivas
        if (!IsInstanceValid(target))
        {
            GD.PrintErr("Target_Player2 não é válido.");
            return;
        }

        // Salva os dados para o deferred
        _targetToReparent = target;
        _jogadorToParent = jogador;

        CallDeferred(nameof(ReparentTarget));

        tile.Estado = TileState.Ocupado;
    }

    // Campos privados para segurar referências temporárias
    private Node3D _targetToReparent;
    private Node3D _jogadorToParent;

    private void ReparentTarget()
    {
        if (_targetToReparent == null || _jogadorToParent == null)
            return;

        var oldParent = _targetToReparent.GetParent();
        if (oldParent != null)
            oldParent.RemoveChild(_targetToReparent);

        _jogadorToParent.AddChild(_targetToReparent);
        _targetToReparent.Position = Vector3.Zero;

        // Atualiza manualmente a referência da CameraRig
        CameraRig cameraRig = GetTree().Root.FindChild("CameraRig", true, false) as CameraRig;
        if (cameraRig != null)
            cameraRig.target = _targetToReparent;

        _targetToReparent = null;
        _jogadorToParent = null;
    }

    //Interface


    
}
