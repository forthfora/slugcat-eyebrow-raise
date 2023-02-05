using IL.Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace SlugcatEyebrowRaise
{
    internal static class Hooks
    {
        private static SoundID? GetVineBoomSoundID() => Options.vineBoomBassBoosted.Value ? Enums.Sounds.VineBoomLoud : Enums.Sounds.VineBoom;
        private static string GetVineBoomStringID() => Options.vineBoomBassBoosted.Value ? "VineBoomLoud" : "VineBoom";

        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.Player.Update += Player_Update;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

            IL.Player.Die += Player_Die;
            IL.HUD.TextPrompt.Update += TextPrompt_Update;
        }


        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            MachineConnector.SetRegisteredOI(SlugcatEyebrowRaise.MOD_ID, Options.instance);
            Enums.Sounds.RegisterValues();
            ResourceLoader.LoadSprites();
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.Sounds.UnregisterValues();
        }

        private static bool isEyebrowRaised = false;
        private static bool wasEyebrowRaised = false;

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (Input.GetKey(KeyCode.LeftAlt))
            {
                self.room.PlaySound(GetVineBoomSoundID(), self.mainBodyChunk);
                isEyebrowRaised = true;                
            }
            else
            {
                isEyebrowRaised = false;
                wasEyebrowRaised = false;
            }
        }
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!isEyebrowRaised || wasEyebrowRaised) return;

            wasEyebrowRaised = !Options.playEveryFrame.Value;

            sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("default_crouching");
            self.objectLooker.currentMostInteresting = null;
            self.LookAtNothing();
        }

        #region Death Hooks
        private static void Player_Die(ILContext il)
        {
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld<SoundID>("UI_Slugcat_Die")
                ))
            {
                c.Remove();
                c.Emit<Enums.Sounds>(OpCodes.Ldsfld, GetVineBoomStringID());
            }
        }

        private static void TextPrompt_Update(ILContext il)
        {
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.AfterLabel,
                i => i.MatchLdsfld<SoundID>("HUD_Game_Over_Prompt")
                ))
            {
                c.Remove();
                c.Emit<Enums.Sounds>(OpCodes.Ldsfld, GetVineBoomStringID());
            }
        }
        #endregion
    }
}
