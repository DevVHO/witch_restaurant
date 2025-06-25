using Godot;
using System;
using System.Collections.Generic;
using Game.Enums;
using Game.Interfaces;

public partial class GridManager : Node3D
{

    [Export] public NodePath SymbolMapNodePath;
    private KitchenObjectSymbolMap symbolMap;
    private Dictionary<char, PackedScene> mapaKitchenObjects;

    [Export] public PackedScene TileScene { get; set; }
    [Export] public PackedScene JogadorScene { get; set; }

    private Dictionary<Vector3I, Tile> grid = new();
    private Dictionary<char, TileState> mapaEstadosTile;


    private const int GRID_WIDTH = 15;
    private const int GRID_HEIGHT = 15;
    private const float TILE_SIZE = 1.0f; // Ajuste conforme a escala do seu mesh

    public override void _Ready()
    {
        symbolMap = GetNode<KitchenObjectSymbolMap>(SymbolMapNodePath);
        mapaKitchenObjects = symbolMap.CriarMapaCenas();
        mapaEstadosTile = symbolMap.CriarMapaEstados();
        GenerateGrid();
        Vector3I posicaoInicial = new Vector3I(6, 0, 14);
        InstanciarJogadorNaGrid(posicaoInicial);
    }
    private (char simbolo, int rotacaoGraus) InterpretarToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length < 2)
            return (' ', 0);

        char simbolo = token[1];
        int rotacao = 0;

        if (char.IsDigit(token[0]))
            rotacao = int.Parse(token[0].ToString()) * 90;

        return (simbolo, rotacao % 360);
    }
    public void GenerateGrid()
    {
        grid.Clear();

        // Tile temporário para obter tamanho
        Tile tileTemp = TileScene.Instantiate<Tile>();
        AddChild(tileTemp);
        Aabb bounds = tileTemp.GetChild<MeshInstance3D>(0).GetAabb();
        Vector3 tileSize = bounds.Size;
        RemoveChild(tileTemp);
        tileTemp.QueueFree();

        for (int z = 0; z < layoutMapa.GetLength(0); z++)
        {
            for (int x = 0; x < layoutMapa.GetLength(1); x++)
            {
                string token = layoutMapa[z, x];
                var (simbolo, rotacao) = InterpretarToken(token);

                Vector3I gridPos = new Vector3I(x, 0, z);
                Vector3 worldPos = new Vector3(x * tileSize.X, 0, z * tileSize.Z);

                Tile tile = TileScene.Instantiate<Tile>();
                tile.Position = worldPos;
                tile.Posicao = gridPos;

                // Tiles de chão simples
                if (simbolo == 'V')
                {
                    tile.Estado = TileState.Livre;
                    AddChild(tile);
                    grid[gridPos] = tile;
                    continue;
                }

                // Tiles com KitchenObject
                if (mapaKitchenObjects.TryGetValue(simbolo, out var cena) && cena != null)
                {
                    var instance = cena.Instantiate();

                    if (instance is KitchenObject obj)
                    {
                        AddChild(obj); // Primeiro adiciona à árvore de nós (importante!)
                        obj.Position = worldPos;
                        obj.AplicarRotacao(rotacao);

                        // Estado do tile baseado no símbolo
                        tile.Estado = mapaEstadosTile.TryGetValue(simbolo, out var estado)
                            ? estado
                            : TileState.Bloqueado;

                        AddChild(tile);
                        grid[gridPos] = tile;

                        // Ocupar tile com o objeto/ Se o professor não vê, o coração não sente
                        
                        /* if (!OcuparTile(gridPos, obj))
                        {
                            GD.PrintErr($"Erro ao ocupar tile {gridPos} com objeto '{simbolo}'");
                        }
                        */
                    }
                    else
                    {
                        GD.PrintErr($"Instância de símbolo '{simbolo}' NÃO É KitchenObject! Tipo: {instance.GetType()}");
                        continue;
                    }
                }
                else
                {
                    GD.PrintErr($"Símbolo inválido ou cena não encontrada: '{simbolo}'");
                    continue;
                }
            }
        }
    }
    private string[,] layoutMapa = new string[,]
    {
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "0C", "1T", "2C", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "0B", "0B", "0B", "0B", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "0B", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "0B", "VV", "VV", "0B", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "0B", "VV", "VV", "0B", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    { "0B", "0B", "0B", "0B", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV", "VV" },
    };
    //Todos os objetos se incializa olhando para a Direita --->>
    //A cada 1 que eu colocar na frente no character especial é igual +90º


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

        if (jogadorRaw is not PlayerControl jogador)
        {
            GD.PrintErr("Jogador instanciado não é PlayerControl!");
            return;
        }

        jogador.Position = tile.Position;
        jogador.PosicaoNaGrid = gridPos;
        OcuparTile(gridPos, jogador);
        AddChild(jogador);

        var target = GetTree().Root.FindChild("Target_Player2", true, false) as Node3D;
        if (target == null)
        {
            GD.PrintErr("Target_Player2 não encontrado!");
            return;
        }

        if (!IsInstanceValid(target))
        {
            GD.PrintErr("Target_Player2 não é válido.");
            return;
        }

        _targetToReparent = target;
        _jogadorToParent = jogador;

        CallDeferred(nameof(ReparentTarget));
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

    public Vector3I WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.X / TILE_SIZE);
        int y = 0; // se sua grid é 2D no plano XZ, Y fica zero
        int z = Mathf.RoundToInt(worldPos.Z / TILE_SIZE);
        return new Vector3I(x, y, z);
    }
    
}
