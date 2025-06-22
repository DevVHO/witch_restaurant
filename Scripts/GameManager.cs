using Godot;
using System;

public partial class GameManager : Node3D
{
    private GridManager gridManager;   
    public override void _Ready()
    {
        gridManager = GetParent().GetNode<GridManager>("GridManager");
    }
}
