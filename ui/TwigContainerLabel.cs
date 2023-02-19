using Godot;
using System;

public class TwigContainerLabel : Node2D
{
    private TwigContainer twigContainer;
    private Label label;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");

        var twigContainerNodes = GetTree().GetNodesInGroup("twig_container");
        if (twigContainerNodes.Count > 0)
        {
            twigContainer = twigContainerNodes[0] as TwigContainer;
        }
    }

    public override void _Process(float delta)
    {
        if (twigContainer == null)
        {
            return;
        }
        label.Text = twigContainer.CurrentTwigs + " / " + twigContainer.MaxTwigs;
        label.Modulate = new Color(label.Modulate, label.Modulate.a + ((twigContainer.CurrentTwigs == 0 ? .2f : 1f) - label.Modulate.a) * .2f);
        GlobalPosition = twigContainer.GlobalPosition;
    }
}
