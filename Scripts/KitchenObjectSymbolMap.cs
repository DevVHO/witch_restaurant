using Godot;
using System.Collections.Generic;
using Game.Enums;

public partial class KitchenObjectSymbolMap : Node
{
    [Export] public PackedScene FogaoScene { get; set; }
    [Export] public PackedScene SinkScene { get; set; }
    [Export] public PackedScene TableScene { get; set; }
    [Export] public PackedScene ChairScene { get; set; }
    [Export] public PackedScene BalcaoScena { get; set; }
    public Dictionary<char, PackedScene> CriarMapaCenas()
    {
        return new Dictionary<char, PackedScene>
        {
            { 'F', FogaoScene },
            { 'S', SinkScene },
            { 'T', TableScene },
            { 'C', ChairScene },
            { 'B', BalcaoScena}
        };
    }

    public Dictionary<char, TileState> CriarMapaEstados()
    {
        return new Dictionary<char, TileState>
        {
            { 'V', TileState.Livre },
            { 'F', TileState.Bloqueado },
            { 'S', TileState.Bloqueado },
            { 'T', TileState.Ocupado },
            { 'C', TileState.Bloqueado },
            { 'B', TileState.Bloqueado}
        };
    }
}
