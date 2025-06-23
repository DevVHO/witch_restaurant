// Tile.cs
using Godot;
using Game.Enums;
using Game.Interfaces;

public partial class Tile : Node3D
{
    public Vector3I Posicao { get; set; }
    public TileState Estado { get; set; }
    public IOcupanteTile Ocupante { get; set; }
    

    public bool EstaLivre() => Estado == TileState.Livre && Ocupante == null;
}
