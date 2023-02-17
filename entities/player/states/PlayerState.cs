public partial class PlayerState : State
{
    protected const string ACTIVE = "Active";
    protected const string WINNING = "Winning";

    protected Player player;

    public override void _Ready()
    {
        player = GetOwner<Player>();
    }
}
