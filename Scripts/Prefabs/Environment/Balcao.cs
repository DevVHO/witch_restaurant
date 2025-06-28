using Godot;
using System;
using Game.Interfaces;
using Game.Enums;

public partial class Balcao : KitchenObject, IInteragivel
{
    private Marker3D spawnPoint;
    private Item itemEmCima; // Item diretamente no balcão (pode ser bandeja ou outro)
    [Export] public PackedScene BandejaScene;

    public override void _Ready()
    {
        spawnPoint = GetNode<Marker3D>("SpawnPoint");
    }

    public void ConfigurarBandeja()
    {
        if (BandejaScene == null || spawnPoint == null) return;

        var bandeja = (Bandeja)BandejaScene.Instantiate();
        spawnPoint.AddChild(bandeja);
        bandeja.Position = Vector3.Zero;

        itemEmCima = bandeja;
    }

    public bool PossuiItem()
    {
        return itemEmCima != null;
    }

    public Item RetirarItem()
    {
        if (itemEmCima == null)
        {
            GD.PrintErr("Não há item para retirar do balcão.");
            return null;
        }

        var temp = itemEmCima;
        itemEmCima = null;
        return temp;
    }

    public void ReceberItem(Item item)
    {
        if (item == null)
        {
            GD.PrintErr("Item nulo recebido no balcão!");
            return;
        }

        spawnPoint.AddChild(item);
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
        item.Scale = Vector3.One;

        itemEmCima = item;
    }

    public void Interact(PlayerControl player)
    {
        var bandejaNoBalcao = ObterBandejaNoBalcao();

        if (bandejaNoBalcao != null)
        {
            // Se o jogador tem um copo na mão e quer colocar na bandeja
            if (player.PossuiItem() && player.itemNaMao is Copo copo)
            {
                bool adicionou = bandejaNoBalcao.AdicionarItem(copo);
                if (adicionou)
                {
                    player.EntregarItem(); // remove copo da mão do jogador
                    GD.Print("Copo adicionado na bandeja no balcão.");
                }
                else
                {
                    GD.Print("Não foi possível adicionar o copo na bandeja (bandeja cheia).");
                }
                return;
            }
            // Se o jogador está de mão vazia e o balcão tem uma bandeja, pode pegar a bandeja inteira
            else if (!player.PossuiItem() && PossuiItem())
            {
                var item = RetirarItem();
                player.ReceberItem(item);
                return;
            }
        }
        else
        {
            // Se não há bandeja no balcão, interação padrão para pegar ou colocar item no balcão
            if (PossuiItem() && player.MaoVazia())
            {
                var item = RetirarItem();
                player.ReceberItem(item);
            }
            else if (!PossuiItem() && !player.MaoVazia())
            {
                var item = player.EntregarItem();
                ReceberItem(item);
            }
        }
    }

    public Bandeja ObterBandejaNoBalcao()
    {
        // Procura uma Bandeja entre os filhos do spawnPoint (não só filhos diretos do balcão)
        foreach (Node child in spawnPoint.GetChildren())
        {
            if (child is Bandeja bandeja)
                return bandeja;
        }
        return null;
    }
    
}
