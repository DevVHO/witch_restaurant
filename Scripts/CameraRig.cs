    using Godot;
    using System;

public partial class CameraRig : Node3D
{
    [Export] public NodePath TargetPath;
    [Export] public float FollowSpeed = 5.0f;
    [Export] public float FixedY = 0.5f;

    [Export] public float ZoomStep = 0.5f;
    [Export] public float ZoomMin = 2f;
    [Export] public float ZoomMax = 20f;

    [Export] public float FreeMoveSpeed = 5.0f; // Velocidade da câmera destravada

    private Camera3D camera3d;
    public Node3D target;
    private bool isLocked = true;
    public bool IsLocked => isLocked; // Travado por padrão

    public override void _Ready()
    {
        target = GetNode<Node3D>(TargetPath);
        camera3d = GetNode<Camera3D>("Camera3D");
        camera3d.Projection = Camera3D.ProjectionType.Orthogonal;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isLocked && target != null)
        {
            Vector3 targetPos = target.GlobalPosition;
            Vector3 desiredPos = new Vector3(targetPos.X, FixedY, targetPos.Z);
            GlobalPosition = GlobalPosition.Lerp(desiredPos, (float)(FollowSpeed * delta));
        }
        else
        {
            HandleFreeCameraMove(delta);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Lock_Person"))
        {
            isLocked = !isLocked;
            GD.Print($"Camera Lock: {(isLocked ? "ON" : "OFF")}");
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (Input.IsActionJustPressed("Scroll_Mouse_Up"))
                Zoom(-ZoomStep);

            if (Input.IsActionJustPressed("Scroll_Mouse_Down"))
                Zoom(ZoomStep);
        }
    }

    private void Zoom(float amount)
    {
        if (camera3d == null || camera3d.Projection != Camera3D.ProjectionType.Orthogonal)
            return;

        camera3d.Size = Mathf.Clamp(camera3d.Size + amount, ZoomMin, ZoomMax);
    }

    private void HandleFreeCameraMove(double delta)
    {
        Vector3 direction = Vector3.Zero;

        if (Input.IsActionPressed("Walk_Left"))
            direction.X += 1;
        if (Input.IsActionPressed("Walk_Right"))
            direction.X -= 1;
        if (Input.IsActionPressed("Walk_Front"))
            direction.Z -= 1;
        if (Input.IsActionPressed("Walk_Back"))
            direction.Z += 1;

        if (direction != Vector3.Zero)
        {
            direction = direction.Normalized();
            GlobalPosition += direction * FreeMoveSpeed * (float)delta;
        }
    }
    
        
}
