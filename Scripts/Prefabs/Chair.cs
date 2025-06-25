using Godot;
using System;
using Game.Interfaces;
using Game.Enums;

public partial class Chair : KitchenObject
{
    public bool Ocupada { get; set; }
    public NPC Ocupante { get; set; }

    public Vector3 GetPosicaoSentar()
    {
        // A posição onde o NPC deve ficar sentado, ajustada com base na rotação
        Vector3 frente = -Transform.Basis.Z; // frente local
        return GlobalTransform.Origin + frente * 0.5f; // 0.5 unidades à frente
    }
    
}

