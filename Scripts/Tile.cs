using Godot;
using Game.Enums;
using Game.Interfaces;

public partial class Tile : Node3D
{
    public Vector3I Posicao { get; set; }

    private TileState _estado;
    public TileState Estado
    {
        get => _estado;
        set
        {
            _estado = value;
            AtualizarVisual();
        }
    }

    public IOcupanteTile Ocupante { get; set; }

    public bool EstaLivre() => Estado == TileState.Livre && Ocupante == null;

    private void AtualizarVisual()
    {
        var mesh = GetNodeOrNull<MeshInstance3D>("Mesh");
        if (mesh == null)
        {
            GD.PrintErr("MeshInstance3D não encontrado no Tile.");
            return;
        }

        // Garante que o material seja único para esse tile
        if (mesh.MaterialOverride == null || mesh.MaterialOverride is not ShaderMaterial)
        {
            if (mesh.GetActiveMaterial(0) is ShaderMaterial shaderMatOriginal)
                mesh.MaterialOverride = shaderMatOriginal.Duplicate() as ShaderMaterial;
            else
            {
                GD.PrintErr("Tile não possui ShaderMaterial ativo.");
                return;
            }
        }

        var material = mesh.MaterialOverride as ShaderMaterial;

        // Define cor com base no estado
        Color cor = new Color(1, 1, 1); // Branco padrão

        switch (Estado)
        {
            case TileState.Livre:
                cor = new Color(0.9f, 0.9f, 0.9f); // quase branco
                break;
            case TileState.Ocupado:
                cor = new Color(1f, 0.6f, 0.1f); // laranja
                break;
            case TileState.Bloqueado:
                cor = new Color(0.8f, 0.1f, 0.1f); // vermelho escuro
                break;
        }

        material.SetShaderParameter("base_color", cor);
    }
}