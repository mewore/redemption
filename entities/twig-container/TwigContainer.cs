using Godot;

public class TwigContainer : Node2D
{
    [Signal]
    delegate void AllTwigsCollected();

    [Export]
    private PackedScene twigSpotScene = null;

    [Export]
    private float revolutionsPerTwigSpot = .1f;

    [Export]
    private float spinSpeed = -.2f;

    [Export(PropertyHint.ExpEasing)]
    private float distanceEasing = -.5f;

    int nextTwigSpotIndex = 0;
    Node2D[] twigSpots;
    float[] twigSpotDistances;
    Node2D[] twigs;

    float now = 0f;
    float spotAngleStep = 0f;

    [Export]
    private float twigFollowSpeed = .5f;

    public override void _Ready()
    {
        twigSpots = new Node2D[GetTree().GetNodesInGroup("twig").Count];
        twigSpotDistances = new float[twigSpots.Length];
        GD.Print("Twig count: " + twigSpots.Length);
        float maxSpotDistance = GetNode<Node2D>("SpotRange").Position.Length();
        if (twigSpots.Length > 1)
        {
            spotAngleStep = revolutionsPerTwigSpot * Mathf.Pi * 2f;
        }
        for (int index = 0; index < twigSpots.Length; index++)
        {
            twigSpots[index] = twigSpotScene.Instance<Node2D>();
            AddChild(twigSpots[index]);
            twigSpotDistances[index] = Mathf.Ease((float)(index + 1) / twigSpots.Length, distanceEasing) * maxSpotDistance;
        }
        twigs = new Node2D[twigSpots.Length];
        RefreshSpotPositions();
    }

    public override void _Process(float delta)
    {
        now += delta;
        RefreshSpotPositions();
    }

    private void RefreshSpotPositions()
    {
        float angle = now * spinSpeed * Mathf.Pi * 2f;
        for (int index = 0; index < twigSpots.Length; index++)
        {
            twigSpots[index].Position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * twigSpotDistances[index];
            angle += spotAngleStep;
        }
        for (int index = 0; index < nextTwigSpotIndex; index++)
        {
            twigs[index].GlobalPosition += (twigSpots[index].GlobalPosition - twigs[index].GlobalPosition) * twigFollowSpeed;
        }
    }

    public void ReserveSpot(Node2D twig)
    {
        twigSpots[nextTwigSpotIndex].Visible = false;
        twigs[nextTwigSpotIndex++] = twig;
        if (nextTwigSpotIndex == twigs.Length)
        {
            EmitSignal(nameof(AllTwigsCollected));
        }
    }
}
