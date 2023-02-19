using Godot;

public class PauseMenu : CanvasLayer
{
    public override void _Ready()
    {
        if (OS.HasFeature("web"))
        {
            GetNode("CenterContainer/VBoxContainer/ExitButton").QueueFree();
        }
        Visible = GetTree().Paused;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_pause"))
        {
            Visible = GetTree().Paused = !GetTree().Paused;
        }
    }

    public void _on_ResumeButton_pressed()
    {
        Visible = GetTree().Paused = false;
    }

    public void _on_RestartButton_pressed()
    {
        GetTree().ReloadCurrentScene();
        GetTree().Paused = false;
    }

    public void _on_MainMenuButton_pressed()
    {
        GetTree().ChangeScene("res://scenes/MainMenu.tscn");
        GetTree().Paused = false;
    }

    public void _on_ExitButton_pressed()
    {
        GetTree().Quit();
    }
}
