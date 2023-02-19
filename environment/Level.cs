using Godot;

public class Level : Node2D
{
    private TileMap map;
    private Vector2[] exitCells;

    private Player player;
    private Camera2D camera;

    public override void _Ready()
    {
        map = GetNode<TileMap>("Game/TileMap");

        TileMap exitTileMap = GetNode<TileMap>("Game/ExitTileMap");
        Godot.Collections.Array usedCells = exitTileMap.GetUsedCells();
        exitCells = new Vector2[usedCells.Count];
        int index = 0;
        foreach (Vector2 cell in usedCells)
        {
            exitCells[index++] = cell;
        }
        exitTileMap.QueueFree();

        player = GetNode<Player>("Game/Player");
        camera = GetNode<Camera2D>("Game/Camera2D");
    }

    public override void _PhysicsProcess(float delta)
    {
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
    }

    public void _on_TwigContainer_AllTwigsCollected()
    {
        foreach (Vector2 cell in exitCells)
        {
            map.SetCell((int)cell.x, (int)cell.y, -1);
        }
    }
}
