using System.Collections.Generic;
using Godot;

public class Level : Node2D
{
    [Export]
    private PackedScene twigCountLabelScene;

    private TileMap map;
    private Vector2[] exitCells;

    private Player player;
    private Camera2D camera;

    private Twig[] twigs;
    private Node twigCountLabelContainer;
    private List<TwigCountLabel> twigCountLabels = new List<TwigCountLabel>();

    private Dictionary<int, int> twigCount = new Dictionary<int, int>();
    private Dictionary<int, Vector2> twigPositions = new Dictionary<int, Vector2>();

    private float now;
    private Label timeLabel;

    public override void _Ready()
    {
        map = GetNode<TileMap>("Game/TileMap");

        TileMap exitTileMap = GetNode<TileMap>("Game/ExitTileMap");
        Godot.Collections.Array usedCells = exitTileMap.GetUsedCells();
        exitCells = new Vector2[usedCells.Count];
        for (int index = 0; index < exitCells.Length; index++)
        {
            exitCells[index] = (Vector2)usedCells[index];
        }
        exitTileMap.QueueFree();

        player = GetNode<Player>("Game/Player");
        camera = GetNode<Camera2D>("Game/Camera2D");

        var twigNodes = GetTree().GetNodesInGroup("twig");
        twigs = new Twig[twigNodes.Count];
        for (int index = 0; index < twigs.Length; index++)
        {
            twigs[index] = (Twig)twigNodes[index];
        }
        twigCountLabelContainer = GetNode("TwigCountLabels");
        timeLabel = GetNode<Label>("Hud/TimeContainer/TimeLabel");
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
        int seconds = Mathf.FloorToInt(now);
        timeLabel.Text = (seconds / 60) + ":" + ((seconds % 60) / 10) + ((seconds % 60) % 10);
        Vector2 playerPos = player.GlobalPosition;
        if (playerPos.x > camera.LimitRight || playerPos.x < camera.LimitLeft || playerPos.y > camera.LimitBottom || playerPos.y < camera.LimitTop)
        {
            Global.WinLevel(Global.CurrentLevel);
            GetTree().ChangeScene(Global.CurrentLevelPath);
        }
    }

    public override void _Process(float delta)
    {
        camera.Position = player.Position;

        // TODO: Do this only when something relevant has changed (twigs have been dropped, etc.)
        twigCount.Clear();
        twigPositions.Clear();
        foreach (Twig twig in twigs)
        {
            if (!twig.NeedsLabel)
            {
                continue;
            }
            twigCount[twig.PositionId] = (twigCount.ContainsKey(twig.PositionId) ? twigCount[twig.PositionId] : 0) + 1;
            twigPositions[twig.PositionId] = twig.GlobalPosition;
        }

        int labelIndex = 0;
        foreach (int key in twigCount.Keys)
        {
            if (twigCount[key] <= 1)
            {
                continue;
            }
            if (labelIndex >= twigCountLabels.Count)
            {
                TwigCountLabel twigCountLabel = twigCountLabelScene.Instance<TwigCountLabel>();
                twigCountLabels.Add(twigCountLabel);
                twigCountLabelContainer.AddChild(twigCountLabel);
            }
            twigCountLabels[labelIndex].GlobalPosition = twigPositions[key];
            twigCountLabels[labelIndex].TwigCount = twigCount[key];
            labelIndex++;
        }
        for (; labelIndex < twigCountLabels.Count; labelIndex++)
        {
            twigCountLabels[labelIndex].TwigCount = 0;
        }
    }

    public void _on_TwigContainer_AllTwigsCollected()
    {
        foreach (Vector2 cell in exitCells)
        {
            map.SetCell((int)cell.x, (int)cell.y, -1);
        }
    }
}
