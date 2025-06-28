using Godot;
using System.Collections.Generic;
using Game.Interfaces;
using Game.Enums;
using System.Linq;

public partial class EstanteCopos : KitchenObject, IInteragivel
{
    [Export] public PackedScene CopoScene;
    [Export] public Node3D[] Slots;
    private bool estaEnchendoBandeja = true;

    private Dictionary<Node3D, Copo> slotParaCopo = new(); // marker -> copo


    public override void _Ready()
    {
        foreach (var slot in Slots)
        {
            var instanciado = CopoScene.Instantiate();
            if (instanciado is Copo copoInstanciado)
            {
                slot.AddChild(copoInstanciado);
                copoInstanciado.Position = Vector3.Zero;
                slotParaCopo[slot] = copoInstanciado;
            }
            else
            {
                GD.PrintErr("CopoScene não está instanciando um objeto do tipo Copo!");
            }
        }
    }

    public bool TemCopoDisponivel()
    {
        return slotParaCopo.Count > 0;
    }

    public Copo RetirarCopo()
    {
        if (!TemCopoDisponivel()) return null;

        var par = slotParaCopo.First();
        slotParaCopo.Remove(par.Key);
        par.Key.RemoveChild(par.Value);
        return par.Value;
    }

    public void DevolverCopo(Copo copo)
    {
        // Só aceita copo vazio
        if (copo.EstadoAtual != CopoState.Vazio)
        {
            GD.Print("Só é possível devolver copos vazios na estante.");
            return;
        }

        foreach (var slot in Slots)
        {
            if (!slotParaCopo.ContainsKey(slot))
            {
                var paiAtual = copo.GetParent();
                if (paiAtual != null)
                    paiAtual.RemoveChild(copo);

                slot.AddChild(copo);
                copo.Position = Vector3.Zero;
                slotParaCopo[slot] = copo;
                return;
            }
        }

        GD.Print("Não há espaço na estante para devolver o copo.");
    }

    public void Interact(PlayerControl player)
    {
        if (player.MaoVazia())
        {
            if (TemCopoDisponivel())
            {
                var copo = RetirarCopo();
                player.ReceberItem(copo);
            }
            return;
        }

        var item = player.EntregarItem();

        if (item is Bandeja bandeja)
        {
            int coposNaBandeja = bandeja.GetItens().Count(i => i is Copo);
            int capacidadeBandeja = bandeja.ItemSlots.Length;

            // Atualiza o estado de enchimento/esvaziamento baseado na bandeja
            if (coposNaBandeja == 0)
                estaEnchendoBandeja = true;
            else if (coposNaBandeja == capacidadeBandeja)
                estaEnchendoBandeja = false;

            if (estaEnchendoBandeja)
            {
                // Só enche se não estiver cheia e a estante tiver copos
                if (coposNaBandeja < capacidadeBandeja && TemCopoDisponivel())
                {
                    var copoParaMover = RetirarCopo();
                    bool adicionou = bandeja.AdicionarItem(copoParaMover);

                    if (!adicionou)
                    {
                        DevolverCopo(copoParaMover);
                        GD.Print("Não conseguiu adicionar copo na bandeja.");
                    }
                    else
                    {
                        GD.Print("Adicionado um copo na bandeja a partir da estante.");
                    }
                }
                else
                {
                    GD.Print("Bandeja cheia ou estante sem copos para encher.");
                }
            }
            else
            {
                // Modo esvaziar: devolve copos da bandeja até ficar vazia
                if (coposNaBandeja > 0)
                {
                    var coposList = bandeja.GetItens().Where(i => i is Copo).Cast<Copo>().ToList();
                    var copoParaDevolver = coposList[0];
                    bandeja.GetItens().Remove(copoParaDevolver);
                    DevolverCopo(copoParaDevolver);
                    GD.Print("Devolvido um copo da bandeja para a estante.");
                }
                else
                {
                    GD.Print("Bandeja vazia, pronto para encher novamente.");
                }
            }

            player.ReceberItem(item);
            return;
        }

       if (item is Copo copoSimples)
        {
            if (copoSimples.EstadoAtual == CopoState.Vazio)
            {
                if (slotParaCopo.Count < Slots.Length)
                {
                    DevolverCopo(copoSimples);
                    GD.Print("Copo vazio devolvido para a estante.");
                }
                else
                {
                    GD.Print("Estante cheia. Não foi possível devolver o copo.");
                    player.ReceberItem(copoSimples);
                }
            }
            else
            {
                GD.Print("Só copos vazios podem ser devolvidos na estante.");
                player.ReceberItem(copoSimples);
            }
            return;
        }


        GD.Print("Item não é um copo nem bandeja. Ignorado.");
        player.ReceberItem(item);
    }



}
