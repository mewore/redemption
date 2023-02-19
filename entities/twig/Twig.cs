using Godot;
using System;

public class Twig : Node2D
{
    private static readonly Vector2 HALF = Vector2.One * .5f;

    RayCast2D rayCast2D;
    Player player;

    float detectionRangeSquared;

    bool following = false;
    bool delivered = false;
    public bool NeedsLabel => !following && !delivered;

    [Export]
    private float followSpeed = .5f;

    TwigContainer twigContainer;

    TileMap map;

    private int positionId = 0;
    public int PositionId => positionId;

    private Sprite sprite;
    private float now = 0f;

    public override void _Ready()
    {
        var mapNodes = GetTree().GetNodesInGroup("map");
        if (mapNodes.Count > 0)
        {
            map = mapNodes[0] as TileMap;
            SnapToMap(GlobalPosition);
        }

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

        sprite = GetNode<Sprite>("Sprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
        if (delivered || player == null)
        {
            return;
        }

        Vector2 distance = player.GlobalPosition - GlobalPosition;
        if (following)
        {
            if (player.Overencumbered)
            {
                player.ReleaseTwig();
                SnapToMap(player.GlobalPosition);
                following = false;
                return;
            }
            GlobalPosition += distance * followSpeed;
            return;
        }
        sprite.Frame = Mathf.RoundToInt(now % 1f);

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
        if (following = player.PickTwigUp())
        {
            sprite.Frame = 0;
        }
    }

    public void _on_Player_DroppedAllTwigs()
    {
        following = false;
        SnapToMap(player.GlobalPosition);
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
        sprite.Frame = 0;
    }

    private void SnapToMap(Vector2 targetPosition)
    {
        if (map == null)
        {
            GD.Print("NOT Snapping to map?!");
            return;
        }
        Vector2 cellSize = map.ToGlobal(map.CellSize);
        targetPosition /= GlobalScale;
        Vector2 cellPosition = (targetPosition / map.CellSize - HALF).Round();
        Position = (cellPosition + HALF) * map.CellSize;
        positionId = (((int)cellPosition.x + (1 << 14)) << 15) + (int)cellPosition.y + (1 << 14);
    }
}
