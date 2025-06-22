// Tile.cs
using Godot;
using Game.Enums;

public partial class Tile : Node3D
{
    public Vector3I Posicao { get; set; }
    public TileState Estado { get; set; }
    public KitchenObject Ocupante { get; set; }

    public bool EstaLivre() => Estado == TileState.Livre && Ocupante == null;
}
