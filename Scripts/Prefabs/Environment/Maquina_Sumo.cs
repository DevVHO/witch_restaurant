using Godot;
using Game.Interfaces;
using System.Collections.Generic;
using System.Linq;

public partial class Maquina_Sumo : KitchenObject, IInteragivel
{
    [Export] public Node3D[] Slots;

    private List<SlotProcesso> processos = new();

    public override void _Process(double delta)
    {
        for (int i = 0; i < processos.Count; i++)
        {
            var proc = processos[i];
            if (proc.TempoRestante > 0)
            {
                proc.TempoRestante -= (float)delta;
                if (proc.TempoRestante <= 0)
                {
                    proc.Copo.AdicionarIngrediente("SucoLaranja");
                    GD.Print("Suco pronto no copo!");
                }
            }
        }
    }

    public void Interact(PlayerControl player)
    {
        // Jogador quer pegar um copo da máquina
        if (player.MaoVazia())
        {
            var pronto = processos.FirstOrDefault(p => p.TempoRestante <= 0);
            if (pronto != null)
            {
                pronto.Slot.RemoveChild(pronto.Copo);
                player.ReceberItem(pronto.Copo);
                processos.Remove(pronto);
                GD.Print("Jogador pegou o copo pronto da máquina.");
            }
            else
            {
                GD.Print("Nenhum copo pronto para retirada.");
            }
            return;
        }

        // Jogador quer colocar um copo
        var item = player.EntregarItem();
        if (item is Copo copo)
        {
            foreach (var slot in Slots)
            {
                bool ocupado = processos.Any(p => p.Slot == slot);
                if (!ocupado)
                {
                    slot.AddChild(copo);
                    copo.Position = Vector3.Zero;

                    processos.Add(new SlotProcesso
                    {
                        Copo = copo,
                        Slot = slot,
                        TempoRestante = 8f
                    });

                    GD.Print("Copo colocado. Suco sendo preparado...");
                    return;
                }
            }

            GD.Print("Todos os slots estão ocupados.");
            player.ReceberItem(copo);
        }
        else
        {
            GD.Print("Item não é um copo.");
            player.ReceberItem(item);
        }
    }

    private class SlotProcesso
    {
        public Copo Copo;
        public float TempoRestante = 8f; // 8 segundos para preparar
        public Node3D Slot;
    }

}
