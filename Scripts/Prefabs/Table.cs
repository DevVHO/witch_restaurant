using Godot;
using System;
using Game.Interfaces;

public partial class Table : KitchenObject
{
    private Marker3D pontoDeColocacao;
    private Node3D itemSobreMesa;
    public override void _Ready()
    {

    }

    public bool TemItem()
    {
        return itemSobreMesa != null;
    }

    public void ColocarItem(Node3D item)
    {
        if (TemItem())
        {
            GD.Print("A mesa já tem um item.");
            return;
        }

        itemSobreMesa = item;

        if (pontoDeColocacao != null)
        {
            itemSobreMesa.GlobalTransform = pontoDeColocacao.GlobalTransform;
        }
        else
        {
            itemSobreMesa.Position = GlobalPosition + new Vector3(0, 1f, 0); // fallback
        }

        AddChild(itemSobreMesa);
    }

    public Node3D RetirarItem()
    {
        if (!TemItem()) return null;

        Node3D item = itemSobreMesa;
        RemoveChild(itemSobreMesa);
        itemSobreMesa = null;
        return item;
    }

    /// <summary>
    /// Retorna a direção da frente da mesa (baseada em RotacaoGraus).
    /// Serve para saber de qual lado o jogador pode interagir.
    /// </summary>
    public Vector3I GetDirecaoFrontal()
    {
        return RotacaoGraus switch
        {
            0 => new Vector3I(0, 0, 1),
            90 => new Vector3I(1, 0, 0),
            180 => new Vector3I(0, 0, -1),
            270 => new Vector3I(-1, 0, 0),
            _ => new Vector3I(0, 0, 1)
        };
    }

    public Vector3I GetTileDeInteracao()
    {
        return PosicaoNaGrid + GetDirecaoFrontal();
    }
    
}
