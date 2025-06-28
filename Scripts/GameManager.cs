using Godot;
using System;

public partial class GameManager : Node3D
{
    private GridManager gridManager;
    public int BandejasAtuais { get; private set; } = 0;
    [Export] public int LimiteMaximoDeBandejas = 3;
    public override void _Ready()
    {
        gridManager = GetParent().GetNode<GridManager>("GridManager");
        gridManager.GenerateGrid();
        Vector3I posicaoInicial = new Vector3I(6, 0, 14);
        gridManager.InstanciarJogadorNaGrid(posicaoInicial);
    }
}
