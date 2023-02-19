using Godot;
using System;

public class Twig : Node2D
{
    RayCast2D rayCast2D;
    Player player;

    float detectionRangeSquared;

    bool following = false;
    bool delivered = false;

    [Export]
    private float followSpeed = .5f;

    TwigContainer twigContainer;

    public override void _Ready()
    {
        var playerNodes = GetTree().GetNodesInGroup("player");
        if (playerNodes.Count > 0)
        {
            player = playerNodes[0] as Player;
            player.Connect("DroppedAllTwigs", this, nameof(_on_Player_DroppedAllTwigs));
            player.Connect("ReachedTwigContainer", this, nameof(_on_Player_ReachedTwigContainer));
        }
        rayCast2D = GetNode<RayCast2D>("RayCast2D");
        detectionRangeSquared = (rayCast2D.CastTo * GlobalScale).LengthSquared();

        var twigContainerNodes = GetTree().GetNodesInGroup("twig_container");
        if (twigContainerNodes.Count > 0)
        {
            twigContainer = twigContainerNodes[0] as TwigContainer;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (delivered || player == null)
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

    public void _on_Player_ReachedTwigContainer()
    {
        if (!following)
        {
            return;
        }

        following = false;
        delivered = true;
        twigContainer.ReserveSpot(this);
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
        GetNode<Sprite>("Sprite").Frame = 0;
    }
}
