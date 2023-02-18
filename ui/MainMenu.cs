using Godot;

public class MainMenu : Node
{
    public override void _Ready()
    {
        GetNode<Label>("MenuContainer/Title").Text = (string)ProjectSettings.GetSetting("application/config/name");
    }


    public void _on_StartButton_pressed()
    {
        GetTree().ChangeScene(Global.CurrentLevelPath);
    }
}
