using Godot;
using Game.Enums;

namespace Game.Interfaces
{
    public interface IOcupanteTile
    {
        Vector3I PosicaoNaGrid { get; set; }
    }
        public interface IInteragivel
    {
        void Interact(PlayerControl player);
    }

}