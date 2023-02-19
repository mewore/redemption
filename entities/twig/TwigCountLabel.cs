using Godot;
using System;

public class TwigCountLabel : Node2D
{
    private Label label;

    public int TwigCount
    {
        set
        {
            label.Text = value.ToString();
            Visible = value > 0;
        }
    }

    public override void _Ready()
    {
        Visible = false;
        label = GetNode<Label>("Label");
    }
}
