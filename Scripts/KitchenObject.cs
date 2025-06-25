using Game.Interfaces;
using Godot;
using System;
using Game.Enums;

public abstract partial class KitchenObject : Node3D, IOcupanteTile
{
    public Vector3I PosicaoNaGrid { get; set; }
    public int RotacaoGraus { get; private set; } = 0;
    public virtual void AplicarRotacao(int graus)
    {
        RotacaoGraus = graus % 360;
        Rotation = new Vector3(0, Mathf.DegToRad(RotacaoGraus), 0);
    }
}
