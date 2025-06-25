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

        for (int z = 0; z < layoutMapa.Length; z++)
        {
            string linha = layoutMapa[z];
            for (int x = 0; x < linha.Length; x++)
            {

                char simbolo = linha[x];
                Vector3I gridPos = new Vector3I(x, 0, z);
                Vector3 worldPos = new Vector3(x * tileSize.X, 0, z * tileSize.Z);

                Tile tile = null;

                // 1. Caso símbolo seja 'V', instancia o tile padrão
                if (simbolo == 'V')
                {
                    tile = TileScene.Instantiate<Tile>();
                    tile.Position = worldPos;
                    tile.Posicao = gridPos;
                    tile.Estado = TileState.Livre;
                    AddChild(tile);
                }
                else
                {
                    // 2. Para outros símbolos, instancia o prefab correspondente
                    if (mapaKitchenObjects.TryGetValue(simbolo, out var cena) && cena != null)
                    {
                        var instance = cena.Instantiate();

                        if (instance is KitchenObject obj)
                        {
                            obj.Position = worldPos;
                            AddChild(obj);

                            // 3. Cria tile para acompanhar a lógica, mesmo se não visual
                            tile = TileScene.Instantiate<Tile>();
                            tile.Position = worldPos;
                            tile.Posicao = gridPos;
                            tile.Ocupante = obj;

                            // Define o estado baseado no mapa de estados
                            if (mapaEstadosTile.TryGetValue(simbolo, out var estado))
                            {
                                tile.Estado = estado;
                            }
                            else
                            {
                                tile.Estado = TileState.Bloqueado; // fallback seguro
                            }

                            AddChild(tile);
                        }
                        else
                        {
                            GD.PrintErr($"Instância de símbolo '{simbolo}' NÃO É KitchenObject! Tipo real: {instance.GetType()}");
                        }
                    }
                    else
                    {
                        GD.PrintErr($"Nenhuma cena encontrada para símbolo '{simbolo}'");
                        continue;
                    }
                }

                // 4. Sempre adiciona tile à grid lógica
                if (tile != null)
                {
                    grid[gridPos] = tile;
                }
            }
        }
    }


    private string[] layoutMapa = new string[]
    {
        "VVVVVVVVVVVVVVV",
        "VVVVVVVVVCTCVVV",
        "VCTCVVVVVVVVVVV",
        "VVVVVVVVVCTCVVV",
        "VVVVVVVVVVVVVVV",
        "VVVVVVVVVVVVVVV",
        "VVCTCVVVVVCTCVV",
        "VVVVVVVVVVVVVVV",
        "BBBBBVVVVVVVVVV",
        "VVVVVVVVVVVVVVV",
        "VVVVBVVVVVVVVVV",
        "VVVVBVVVVVVVVVV",
        "BBBBBVVVVVCTCVV",
        "VVVVVVVVVVVVVVV",
        "VVVVVVVVVVVVVVV"
    };



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
