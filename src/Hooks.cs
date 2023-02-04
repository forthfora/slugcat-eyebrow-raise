using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace VineBoomDeath
{
    internal static class Hooks
    {
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            IL.HUD.TextPrompt.Update += TextPrompt_Update;
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            MachineConnector.SetRegisteredOI(VineBoomDeath.MOD_ID, Options.instance);
            Enums.Sounds.RegisterValues();
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.Sounds.UnregisterValues();
        }

        private static void TextPrompt_Update(ILContext il)
        {
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld<SoundID>("HUD_Game_Over_Prompt")
                ))
            {
                c.MoveAfterLabels();
                c.Remove();
                c.Emit<Enums.Sounds>(OpCodes.Ldsfld, "VineBoomLoud");
            }
        }
    }
}
