using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace VineBoomDeath
{
    internal static class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            Enums.Sounds.RegisterValues();

            IL.HUD.TextPrompt.Update += TextPrompt_Update;
            IL.Player.Die += Player_Die;
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.Sounds.UnregisterValues();
        }

        private static void Player_Die(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld<SoundID>("UI_Slugcat_Die")
                );
            c.Remove();
            c.Emit<Enums.Sounds>(OpCodes.Ldsfld, "UI_Slugcat_VineBoomDie");
        }

        private static void TextPrompt_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld<SoundID>("HUD_Game_Over_Prompt")
                );
            c.Remove();
            c.Emit<Enums.Sounds>(OpCodes.Ldsfld, "UI_Slugcat_VineBoomDie");
        }
    }
}
