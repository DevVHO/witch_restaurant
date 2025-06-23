using Game.Interfaces;
using Godot;
using System;

public partial class KitchenObject : Node3D, IOcupanteTile
{
    public  Vector3I PosicaoNaGrid { get; set; }
}
