using System;
using System.Reflection;
using Alta.Character;
using Alta.Networking.Scripts.Player;
using Alta.Networking.Servers;
using MelonLoader;
using TabletUILib;

[assembly: MelonInfo(typeof(UnstuckButton.Core), "UnstuckButton", "1.0.0", "CGNik", null)]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace UnstuckButton
{
    public class Core : MelonMod
    {
        private static Action<PlayerList> playerKill;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnLateInitializeMelon()
        {
            TabletBubble unstuckButton = TabletUI.AddBubble("Unstuck Button", "Unstuck");
            unstuckButton.NeedsServer("UnstuckButton");
            unstuckButton.ServerAction("Get Unstuck", "Unstuck", KillPlayer);
        }

        private void KillPlayer(Player player)
        {
            if (!NetworkSceneManager.IsServer || player == null || player.UserInfo == null)
                return;

            var character = player.PlayerController as PlayerCharacter;
            var health = character != null ? character.Health : null;
            if (character == null || !ReferenceEquals(character.NetworkPlayer, player) ||
                !character.IsAlive || character.IsDowned || health == null || health.Health <= 0f)
                return;

            var settings = ServerHandler.Current?.ServerConfig?.Settings;
            if (settings == null)
                return;

            try
            {
                if (playerKill == null)
                {
                    var module = Type.GetType("TavernLib.ModulePorts.PlayerCommandModule, TavernLib");
                    var command = module?.GetMethod("KillPlayer", BindingFlags.Static | BindingFlags.NonPublic,
                        null, new[] { typeof(PlayerList) }, null);
                    if (command == null)
                    {
                        LoggerInstance.Warning("Get Unstuck requires TavernLib's player kill command on the server.");
                        return;
                    }

                    playerKill = (Action<PlayerList>)Delegate.CreateDelegate(typeof(Action<PlayerList>), command);
                }

                var previousSelection = PlayerList.LastList;
                var damageMultiplier = settings.PlayerDamageTakenMultiplier;
                var avoidDamage = PlayerHealth.ArePlayersAvoidingDamage.Value;
                var ignoreDamage = character.IsIgnoringDamage;
                try
                {
                    playerKill(new PlayerList(new IPlayer[] { player }));
                }
                finally
                {
                    // PlayerList changes the console selection, and the kill handler temporarily changes damage settings.
                    new PlayerList(previousSelection);
                    settings.PlayerDamageTakenMultiplier = damageMultiplier;
                    if (character != null)
                        character.IsIgnoringDamage = ignoreDamage;
                    PlayerHealth.ArePlayersAvoidingDamage.Value = avoidDamage;
                }
            }
            catch (Exception error)
            {
                LoggerInstance.Error("Get Unstuck could not run player kill: " + error);
            }
        }
    }
}
