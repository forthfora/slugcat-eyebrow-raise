using IL.Menu;
using IL.MoreSlugcats;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Net;
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
        }


        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            MachineConnector.SetRegisteredOI(SlugcatEyebrowRaise.MOD_ID, Options.instance);
            Enums.Sounds.RegisterValues();
            ResourceLoader.LoadSprites();

            try
            {
                //IL.PlayerGraphics.PlayerObjectLooker.LookAtPoint += PlayerObjectLooker_LookAtPoint;
                //IL.PlayerGraphics.PlayerObjectLooker.LookAtObject += PlayerObjectLooker_LookAtObject;

                IL.Player.Die += Player_Die;
                IL.HUD.TextPrompt.Update += TextPrompt_Update;

            }
            catch (Exception ex)
            {
                SlugcatEyebrowRaise.Logger.LogError(ex);
            }
        }

        private static void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);

            Enums.Sounds.UnregisterValues();
        }

        private const int MAX_NUMBER_OF_PLAYERS = 4;
        private const int ANIMATION_FRAMERATE = 30;
        private const int FRAME_COUNT = 3;

        private readonly static bool[] isPlayerKeyPressed = new bool[MAX_NUMBER_OF_PLAYERS];
        private readonly static bool[] isPlayerEyebrowRaised = new bool[MAX_NUMBER_OF_PLAYERS];
        private readonly static int[] playerEyebrowRaiseLevel = new int[MAX_NUMBER_OF_PLAYERS];

        private static float raiseTimer = 0.0f;

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            isPlayerKeyPressed[0] = Input.GetKey(Options.player1Keybind.Value);
            isPlayerKeyPressed[1] = Input.GetKey(Options.player2Keybind.Value);
            isPlayerKeyPressed[2] = Input.GetKey(Options.player3Keybind.Value);
            isPlayerKeyPressed[3] = Input.GetKey(Options.player4Keybind.Value);

            for (int i = 0; i < MAX_NUMBER_OF_PLAYERS; i++)
            {
                if (!isPlayerKeyPressed[i])
                {
                    isPlayerEyebrowRaised[i] = false;
                }
            }
        }
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            int playerIndex = self.player.playerState.playerNumber;
            int raiseLevel = playerEyebrowRaiseLevel[playerIndex];

            if (Time.time - raiseTimer > 1.0f / ANIMATION_FRAMERATE)
            {
                raiseTimer = Time.time;

                if (isPlayerKeyPressed[playerIndex])
                {
                    if (raiseLevel < FRAME_COUNT)
                    {
                        raiseLevel++;
                    }
                }
                else if (raiseLevel > 0)
                {
                    raiseLevel--;
                }

                playerEyebrowRaiseLevel[playerIndex] = raiseLevel;
            }

            string? face = GetFace(self, raiseLevel);
            if (face != null) SetFaceSprite(sLeaser, face);

            if (!isPlayerKeyPressed[playerIndex]) return;

            self.LookAtNothing();

            if (isPlayerEyebrowRaised[playerIndex]) return;

            isPlayerEyebrowRaised[playerIndex] = !Options.playEveryFrame.Value;
            self.player.room.PlaySound(GetVineBoomSoundID(), self.player.mainBodyChunk);
        }

        private static string? GetFace(PlayerGraphics self, int raiseLevel)
        {
            if (raiseLevel == 0) return null;

            SlugcatStats.Name name = self.player.SlugCatClass;
            string face = "default";

            //if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            //{
            //    face = "artificer";
            //}
            //else if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            //{
            //    face = "saint";
            //}

            if (self.blink > 0 && raiseLevel == FRAME_COUNT)
            {
                face += "_blink";
            }
            else
            {
                face += "_" + raiseLevel;
            }

            return face;
        }

        private static void SetFaceSprite(RoomCamera.SpriteLeaser sLeaser, string spriteName)
        {
            if (!Futile.atlasManager.DoesContainElementWithName(spriteName))
            {
                SlugcatEyebrowRaise.Logger.LogError($"Missing sprite ({spriteName})! Please check the sprites directory under the mod's folder");
                return;
            }
            sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(spriteName);
            sLeaser.sprites[9].scaleX = 1;
        }

        private static void PlayerObjectLooker_LookAtPoint(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.Before);
            c.EmitDelegate<Action<PlayerGraphics.PlayerObjectLooker>>(ob =>
            {
                if (isPlayerEyebrowRaised[ob.owner.player.playerState.playerNumber])
                {
                    c.Emit(OpCodes.Ret);
                }
            });
        }

        private static void PlayerObjectLooker_LookAtObject(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.Before);
            c.EmitDelegate<Action<PlayerGraphics.PlayerObjectLooker>>(ob =>
            {
                if (isPlayerEyebrowRaised[ob.owner.player.playerState.playerNumber])
                {
                    c.Emit(OpCodes.Ret);
                }
            });
        }

        #region Death IL Hooks
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
