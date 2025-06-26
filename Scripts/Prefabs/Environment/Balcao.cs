using Godot;
using System;
using Game.Interfaces;
using Game.Enums;

public partial class Balcao : KitchenObject,IInteragivel // Certifique-se de herdar de KitchenObject!
{
    private Node3D spawnPoint;
    private Node3D itemEmCima;
    [Export] public PackedScene BandejaScene;
    private Node3D suporteNoBalcao; 


    public override void _Ready()
    {
        spawnPoint = GetNode<Node3D>("SpawnPoint");
        // Não instanciar
        // emos aqui mais!
    }
    

    public void ConfigurarBandeja()
    {
        if (BandejaScene == null || spawnPoint == null) return;

        var bandeja = (Node3D)BandejaScene.Instantiate();
        spawnPoint.AddChild(bandeja);
        bandeja.Position = Vector3.Zero;

        suporteNoBalcao = bandeja; // agora salva no campo correto
    }


    public bool TemItem()
    {
        return itemEmCima != null;
    }

    public Node3D RetirarItem()
    {
        var temp = itemEmCima;
        itemEmCima = null;
        return temp;
    }

    public void ReceberItem(Node3D item)
    {
        GD.Print($"Recebendo item: {item.Name}");

        if (item == null)
        {
            GD.Print("Erro: item nulo recebido.");
            return;
        }

        // Coloca como filho do balcão
        AddChild(item);

        // Alinha o item com o balcão visualmente
        item.Position = Vector3.Zero;
        item.Rotation = Vector3.Zero;
        item.Scale = Vector3.One;

        GD.Print("Item recebido e posicionado sobre o balcão.");
    }


    public bool PossuiItem()
    {
        return itemEmCima != null;
    }
    public void Interact(PlayerControl player)
    {
        if (PossuiItem() && player.MaoVazia())
        {
            // Pegar o item do balcão
            var item = RetirarItem();
            player.ReceberItem(item);
        }
        else if (!PossuiItem() && !player.MaoVazia())
        {
            // Soltar o item do jogador no balcão
            var item = player.EntregarItem();
            ReceberItem(item);
        }
    }


}
