using Godot;
using System;

public class PlayerCapacityLabel : Node2D
{
    private Player player;
    private Label label;
    private Vector2 offset;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");

        var playerNodes = GetTree().GetNodesInGroup("player");
        if (playerNodes.Count > 0)
        {
            player = playerNodes[0] as Player;
        }
        offset = GlobalPosition - GetNode<Node2D>("Position2D").GlobalPosition;
    }

    public override void _Process(float delta)
    {
        if (player == null)
        {
            return;
        }
        label.Text = player.CurrentTwigs + " / " + player.MaxTwigs;
        label.Modulate = new Color(label.Modulate, label.Modulate.a + ((player.CurrentTwigs == 0 ? .2f : 1f) - label.Modulate.a) * .2f);
        GlobalPosition = player.GlobalPosition + offset;
    }
}
