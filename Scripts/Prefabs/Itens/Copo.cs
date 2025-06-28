using Godot;
using Game.Enums;

public partial class Copo : Item
{
    public CopoState EstadoAtual { get; private set; } = CopoState.Vazio;

    public void AdicionarIngrediente(string ingrediente)
    {
        if (EstadoAtual == CopoState.Vazio && ingrediente == "SucoLaranja")
        {
            EstadoAtual = CopoState.SucoLaranja;
        }
        else if (EstadoAtual == CopoState.SucoLaranja && ingrediente == "Gelo")
        {
            EstadoAtual = CopoState.SucoLaranjaComGelo;
        }
        else if (EstadoAtual == CopoState.SucoLaranja && ingrediente == "Limao")
        {
            EstadoAtual = CopoState.SucoLaranjaComLimao;
        }
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        var visual = GetNodeOrNull<MeshInstance3D>("Visual");
        if (visual == null)
        {
            GD.PrintErr("Mesh 'Visual' não encontrado no copo.");
            return;
        }

        var material = new StandardMaterial3D();

        switch (EstadoAtual)
        {
            case CopoState.Vazio:
                material.AlbedoColor = new Color(1, 1, 1);
                break;
            case CopoState.SucoLaranja:
                material.AlbedoColor = new Color(1.0f, 0.5f, 0.0f);
                break;
            case CopoState.SucoLaranjaComGelo:
                material.AlbedoColor = new Color(1.0f, 0.6f, 0.2f);
                break;
            case CopoState.SucoLaranjaComLimao:
                material.AlbedoColor = new Color(1.0f, 0.5f, 0.0f).Lerp(new Color(0.7f, 1.0f, 0.2f), 0.4f);
                break;
        }

        visual.SetSurfaceOverrideMaterial(0, material);
    }
}
