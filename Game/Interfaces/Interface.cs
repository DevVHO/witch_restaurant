using Godot;
using Game.Enums;

namespace Game.Interfaces
{
    public interface IOcupanteTile
    {
        Vector3I PosicaoNaGrid { get; set; }
    }
}