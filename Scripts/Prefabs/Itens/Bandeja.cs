using Godot;
using Game.Interfaces;
using System.Collections.Generic;

public partial class Bandeja : Item, IInteragivel
{
    [Export] public Node3D[] ItemSlots;
    private List<Item> itensNaBandeja = new();

    public void Interact(PlayerControl player)
    {
        // Jogador quer pegar um copo da bandeja
        if (!player.MaoVazia() || itensNaBandeja.Count == 0)
            return;

        // Procurar o primeiro item que seja Copo
        Item copoParaPegar = null;
        foreach (var item in itensNaBandeja)
        {
            if (item is Copo)
            {
                copoParaPegar = item;
                break;
            }
        }

        if (copoParaPegar == null)
            return;

        itensNaBandeja.Remove(copoParaPegar);
        player.ReceberItem(copoParaPegar);
        GD.Print("Jogador pegou um copo da bandeja.");
        AtualizarPosicoesItens();
    }


    public bool AdicionarItem(Item item)
    {
        GD.Print($"Tentando adicionar item. Slots usados: {itensNaBandeja.Count}/{ItemSlots.Length}");
        if (itensNaBandeja.Count >= ItemSlots.Length)
        {
            GD.Print("Bandeja cheia!");
            return false;
        }

        // Remover o item do pai anterior com segurança antes de adicionar
        var parent = item.GetParent();
        if (parent != null)
        {
            parent.RemoveChild(item);
        }
        
        // Apenas adiciona na lista
        itensNaBandeja.Add(item);

        // Não faça AddChild aqui! Isso será feito em AtualizarPosicoesItens

        AtualizarPosicoesItens();
        GD.Print("Item adicionado com sucesso.");
        return true;
    }




    private void AtualizarPosicoesItens()
    {
        for (int i = 0; i < itensNaBandeja.Count; i++)
        {
            var item = itensNaBandeja[i];

            // Só troca de pai se necessário
            if (item.GetParent() != ItemSlots[i])
            {
                item.GetParent()?.RemoveChild(item);
                ItemSlots[i].AddChild(item);
                item.Transform = Transform3D.Identity;
            }
        }
    }


    public override void _Ready()
    {
        AtualizarPosicoesItens();
    }

    public List<Item> GetItens() => itensNaBandeja;
}
