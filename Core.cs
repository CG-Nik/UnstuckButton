using MelonLoader;
using TabletUILib;

[assembly: MelonInfo(typeof(UnstuckButton.Core), "UnstuckButton", "1.0.0", "CGNik", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace UnstuckButton
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnLateInitializeMelon()
        {
            TabletBubble unstuckButton = TabletUI.AddBubble("Unstuck Button", "Unstuck");
            unstuckButton.NeedsServer("UnstuckButton");
            unstuckButton.ServerAction("Get Unstuck", "Unstuck", (Player player) => player.PlayerCharacter.Health.Health = 0f);
        }
    }
}