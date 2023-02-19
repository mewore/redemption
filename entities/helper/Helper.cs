using Godot;
using System;

public class Helper : Node2D
{

    RayCast2D rayCast2D;
    Player player;

    float detectionRangeSquared;
    bool following = false;

    [Export]
    private float followSpeed = .2f;

    Vector2 hoverOffset;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        var playerNodes = GetTree().GetNodesInGroup("player");
        if (playerNodes.Count > 0)
        {
            player = playerNodes[0] as Player;
        }
        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        detectionRangeSquared = (rayCast2D.CastTo * GlobalScale).LengthSquared();
        float hoverRange = Mathf.Sqrt(detectionRangeSquared) * .15f;
        float angle = GD.Randf() * Mathf.Pi * 2f;
        hoverOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (GD.Randf() * hoverRange);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (player == null)
        {
            return;
        }

        Vector2 distance = player.GlobalPosition - GlobalPosition;
        if (following)
        {

            GlobalPosition += (distance + hoverOffset) * followSpeed;
            return;
        }

        if (!player.CanCarryMoreTwigs || distance.LengthSquared() > detectionRangeSquared)
        {
            return;
        }
        rayCast2D.CastTo = distance / GlobalScale;
        rayCast2D.ForceRaycastUpdate();
        if (rayCast2D.IsColliding())
        {
            return;
        }
        following = true;
        player.AddHelper();
        CanvasItem sprite = GetNode<CanvasItem>("Sprite");
        sprite.Modulate = new Color(sprite.Modulate, .5f);
    }

    public bool Disappear()
    {
        if (!following)
        {
            return false;
        }
        player.RemoveHelper();
        QueueFree();
        return true;
    }
}
