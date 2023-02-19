using Godot;
using System;

public class Twig : Node2D
{
    RayCast2D rayCast2D;
    Player player;

    float detectionRangeSquared;

    bool following = false;

    [Export]
    private float followSpeed = .5f;

    public override void _Ready()
    {
        var playerNodes = GetTree().GetNodesInGroup("player");
        if (playerNodes.Count > 0)
        {
            player = playerNodes[0] as Player;
            player.Connect("DroppedAllTwigs", this, nameof(_on_Player_DroppedAllTwigs));
        }
        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        Vector2 adjustedCastTo = rayCast2D.CastTo * GlobalScale;
        detectionRangeSquared = adjustedCastTo.LengthSquared();
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
            GlobalPosition += distance * followSpeed;
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
        following = player.PickTwigUp();
    }

    public void _on_Player_DroppedAllTwigs()
    {
        following = false;
    }
}
