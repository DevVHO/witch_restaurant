using Godot;
using System;
using Game.Interfaces;
using Game.Enums;

public partial class Fogao : KitchenObject
{
    public Vector3I DirecaoFrenteGrid()
    {
        // Converte a frente do objeto em direção na grid
        Vector3 frente = -Transform.Basis.Z;
        return new Vector3I(
            Mathf.RoundToInt(frente.X),
            0,
            Mathf.RoundToInt(frente.Z)
        );
    }

}
