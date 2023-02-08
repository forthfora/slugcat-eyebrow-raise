using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SlugcatEyebrowRaise
{
    internal static class Hooks
    {
        private static SoundID? GetVineBoomSoundID() => Options.vineBoomLoud.Value ? Enums.Sounds.VineBoomLoud : Enums.Sounds.VineBoom;
        private static string GetVineBoomStringID() => Options.vineBoomLoud.Value ? "VineBoomLoud" : "VineBoom";

        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;

            On.Player.Update += Player_Update;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

            On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;
        }

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            MachineConnector.SetRegisteredOI(SlugcatEyebrowRaise.MOD_ID, Options.instance);
            Enums.Sounds.RegisterValues();
            ResourceLoader.LoadSprites();

            try
            {
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

        private const int ANIMATION_FRAMERATE = 20;
        private const int FRAME_COUNT = 3;

        private const float MAX_ZOOM = 0.15f;
        private const float ZOOM_DURATION = 1.0f;

        private const float EYEBROW_RAISE_MIN_DURATION = 2.5f;

        private const float SHAKE_DURATION = 1.5f;
        private const float SHAKE_INTENSITY_NORMAL = 0.25f;
        private const float SHAKE_INTENSITY_LOUD = 1.0f;

        private readonly static bool[] isPlayerKeyPressed = new bool[MAX_NUMBER_OF_PLAYERS];
        private readonly static bool[] isPlayerEyebrowRaised = new bool[MAX_NUMBER_OF_PLAYERS];
        private readonly static int[] playerEyebrowRaiseLevel = new int[MAX_NUMBER_OF_PLAYERS];
        private readonly static float[] playerEyebrowRaiseDurationTimer = new float[MAX_NUMBER_OF_PLAYERS];
        private readonly static float[] raiseTimers = new float[MAX_NUMBER_OF_PLAYERS];

        private static float cameraZoomAmount = 0.0f;
        private static float shakeTimer = 0.0f;
        private static float zoomTimer = 0.0f;

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            HandlePlayerInput(self, Options.player1Keybind.Value, 0);
            HandlePlayerInput(self, Options.player2Keybind.Value, 1);
            HandlePlayerInput(self, Options.player3Keybind.Value, 2);
            HandlePlayerInput(self, Options.player4Keybind.Value, 3);

            if (zoomTimer < Time.time)
            {
                cameraZoomAmount = 0.0f;
            }
        }

        private static void HandlePlayerInput(Player player, KeyCode keyCode, int targetPlayerIndex)
        {
            int playerIndex = player.playerState.playerNumber;
            if (playerIndex != targetPlayerIndex) return;

            if (Input.GetKey(keyCode) || (playerIndex == 0 && Input.GetKey(Options.keyboardKeybind.Value)))
            {
                if (!isPlayerKeyPressed[playerIndex] || Options.playEveryFrame.Value)
                {
                    player.room.PlaySound(GetVineBoomSoundID(), player.mainBodyChunk);
                    EyebrowRaiseExplosion(player);

                    if (Options.cameraShake.Value)
                    {
                        shakeTimer = Time.time + SHAKE_DURATION;
                        playerEyebrowRaiseDurationTimer[playerIndex] = Time.time + EYEBROW_RAISE_MIN_DURATION;
                    }

                    if (Options.zoomCamera.Value)
                    {
                        cameraZoomAmount = MAX_ZOOM;
                        zoomTimer = Time.time + ZOOM_DURATION;
                    }
                }

                isPlayerKeyPressed[playerIndex] = true;
                isPlayerEyebrowRaised[playerIndex] = true;

            }
            else
            {
                isPlayerKeyPressed[playerIndex] = false;

                if (playerEyebrowRaiseDurationTimer[playerIndex] < Time.time)
                {
                    isPlayerEyebrowRaised[playerIndex] = false;
                }
            }
        }

        private static void EyebrowRaiseExplosion(Player player)
        {
            Vector2 pos2 = player.firstChunk.pos;

            if (Options.vineBoomCosmetics.Value)
            {
                player.room.AddObject(new Explosion.ExplosionLight(pos2, 100.0f, 0.2f, 16, Color.white));
                player.room.AddObject(new ShockWave(pos2, 500f, 0.05f, 2, false));

                for (int l = 0; l < 10; l++)
                {
                    Vector2 a2 = Custom.RNV();
                    player.room.AddObject(new Spark(pos2 + a2 * Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 3, 6));
                }
            }

            if (!Options.vineBoomExplosion.Value) return;

            List<Weapon> list = new List<Weapon>();
            for (int m = 0; m < player.room.physicalObjects.Length; m++)
            {
                for (int n = 0; n < player.room.physicalObjects[m].Count; n++)
                {
                    if (player.room.physicalObjects[m][n] is Weapon)
                    {
                        Weapon weapon = player.room.physicalObjects[m][n] as Weapon;
                        if (weapon.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                        {
                            list.Add(weapon);
                        }
                    }
                    if (player.room.physicalObjects[m][n] is Creature && player.room.physicalObjects[m][n] != player)
                    {
                        Creature creature = player.room.physicalObjects[m][n] as Creature;
                        if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || player.room.VisualContact(player.abstractCreature.pos, creature.abstractCreature.pos)))
                        {
                            player.room.socialEventRecognizer.WeaponAttack(null, player, creature, true);
                            creature.SetKillTag(player.abstractCreature);
                            if (creature is Scavenger)
                            {
                                (creature as Scavenger).HeavyStun(80);
                            }
                            else
                            {
                                creature.Stun(80);
                            }
                            creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                            if (creature is TentaclePlant)
                            {
                                for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                {
                                    creature.ReleaseGrasp(num5);
                                }
                            }
                        }
                    }
                }
            }
            if (list.Count > 0 && player.room.game.IsArenaSession)
            {
                player.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
            }
            for (int num6 = 0; num6 < list.Count; num6++)
            {
                list[num6].ChangeMode(Weapon.Mode.Free);
                list[num6].firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 1000.0f;
                list[num6].SetRandomSpin();
            }

        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            int playerIndex = self.player.playerState.playerNumber;
            int raiseLevel = playerEyebrowRaiseLevel[playerIndex];

            if (Time.time - raiseTimers[playerIndex] > 1.0f / ANIMATION_FRAMERATE)
            {
                raiseTimers[playerIndex] = Time.time;

                if (isPlayerEyebrowRaised[playerIndex])
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
        }

        private static string? GetFace(PlayerGraphics self, int raiseLevel)
        {
            if (raiseLevel == 0) return null;

            SlugcatStats.Name name = self.player.SlugCatClass;
            string face = "default";

            if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                face = "artificer";
            }
            else if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                face = "saint";
            }

            if (self.blink > 0 && raiseLevel == FRAME_COUNT)
            {
                face += "_blink";
            }
            else
            {
                face += "_" + raiseLevel;
            }

            return SlugcatEyebrowRaise.MOD_ID + "_" + face;
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

        // Henpemaz's magic
        #region Camera Zoom
        private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
        {
            if (shakeTimer > Time.time)
            {
                self.screenShake = Options.vineBoomLoud.Value ? SHAKE_INTENSITY_LOUD : SHAKE_INTENSITY_NORMAL;
            }

            float zoom = 1f;
            bool zoomed = false;
            Vector2 offset = Vector2.zero;
            if (self.room != null && cameraZoomAmount > 0f)
            {
                //zoom = 1f;// self.room.roomSettings.GetEffectAmount(EnumExt_CameraZoomEffect.CameraZoom) * 10f;
                zoom = cameraZoomAmount * 20f;
                zoomed = true;
                Creature creature = (self.followAbstractCreature == null) ? null : self.followAbstractCreature.realizedCreature;
                if (creature != null)
                {
                    //Vector2 testPos = creature.bodyChunks[0].pos + creature.bodyChunks[0].vel + self.followCreatureInputForward * 2f;
                    Vector2 value = Vector2.Lerp(creature.bodyChunks[0].lastPos, creature.bodyChunks[0].pos, timeStacker);
                    if (creature.inShortcut)
                    {
                        Vector2? vector = self.room.game.shortcuts.OnScreenPositionOfInShortCutCreature(self.room, creature);
                        if (vector != null)
                        {
                            //testPos = vector.Value;
                            value = vector.Value;
                        }
                    }
                    offset = new Vector2((float)self.cameraNumber * 6000f, 0f) + (value - (self.pos + self.sSize / 2f));
                }

            }
            if (zoomed)
            {
                for (int i = 0; i < 11; i++) // 11 useful layers the rest if hud
                {
                    self.SpriteLayers[i].scale = 1f;
                    self.SpriteLayers[i].SetPosition(Vector2.zero);
                    self.SpriteLayers[i].ScaleAroundPointRelative(self.sSize / 2f, zoom, zoom);

                    //self.SpriteLayers[i].SetPosition(-offset);
                }
                self.offset = offset;
            }
            else
            {
                // unzoom camera on effect slider to 0 or maybe if changeroom didnt call
                for (int i = 0; i < 11; i++) // 11 useful layers the rest is hud
                {
                    self.SpriteLayers[i].scale = 1f;
                    self.SpriteLayers[i].SetPosition(Vector2.zero);

                    //self.SpriteLayers[i].SetPosition(-offset);
                }
                self.offset = new Vector2((float)self.cameraNumber * 6000f, 0f);
            }

            //self.levelGraphic.scale = zoom;
            int theseed = 0;
            if (zoomed)
            {
                // deterministic random shake
                theseed = UnityEngine.Random.seed;
                UnityEngine.Random.seed = theseed;
            }
            orig(self, timeStacker, timeSpeed);
            if (zoomed)
            {
                // calculate stupid shake again
                // an aternative to this would to spawn a spriteleaser and have it store its drawposition

                UnityEngine.Random.seed = theseed;
                // coppypasta I just need the same exact viewport
                Vector2 vector = Vector2.Lerp(self.lastPos, self.pos, timeStacker);
                if (self.microShake > 0f)
                {
                    vector += RWCustom.Custom.RNV() * 8f * self.microShake * UnityEngine.Random.value;
                }
                if (!self.voidSeaMode)
                {
                    vector.x = Mathf.Clamp(vector.x, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f - 20f, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f + 20f);
                    vector.y = Mathf.Clamp(vector.y, self.CamPos(self.currentCameraPosition).y + 8f - 7f, self.CamPos(self.currentCameraPosition).y + 33f);
                }
                else
                {
                    vector.y = Mathf.Min(vector.y, -528f);
                }
                vector = new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
                vector.x -= 0.02f;
                vector.y -= 0.02f;

                // Magic offsets and magic shader rectangles
                // THIS CRAP BUGS OUT ON SCREEN TRANSITIONS AND i DON'T UNDESTAND WHYYYYYY
                Vector2 magicOffset = self.CamPos(self.currentCameraPosition) - vector;
                //Debug.LogError("magic offset is " + magicOffset);
                //Vector4 center = new Vector4(
                //	(-vector.x - 0.5f + self.levelGraphic.width / 2f + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
                //	(-vector.y + 0.5f + self.levelGraphic.height / 2f + self.CamPos(self.currentCameraPosition).y) / self.sSize.y,
                //	(-vector.x - 0.5f + self.levelGraphic.width / 2f + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
                //	(-vector.y + 0.5f + self.levelGraphic.height / 2f + self.CamPos(self.currentCameraPosition).y) / self.sSize.y);
                Vector4 center = new Vector4(
                    (magicOffset.x + self.levelGraphic.width / 2f) / self.sSize.x,
                    (magicOffset.y + 2f + self.levelGraphic.height / 2f) / self.sSize.y,
                    (magicOffset.x + self.levelGraphic.width / 2f) / self.sSize.x,
                    (magicOffset.y + 2f + self.levelGraphic.height / 2f) / self.sSize.y);
                vector += self.offset;
                Vector4 sprpos = new Vector4(
                    (-vector.x + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
                    (-vector.y + self.CamPos(self.currentCameraPosition).y) / self.sSize.y,
                    (-vector.x + self.levelGraphic.width + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
                    (-vector.y + self.levelGraphic.height + self.CamPos(self.currentCameraPosition).y) / self.sSize.y);

                //sprpos -= new Vector4(17f / self.sSize.x, 18f / self.sSize.y, 17f / self.sSize.x, 18f / self.sSize.y) * (1f - 1f / zoom);
                sprpos -= center;
                sprpos *= zoom;
                sprpos += center;
                Shader.SetGlobalVector("_spriteRect", sprpos);
                Vector2 zooming = (1f - 1f / zoom) * new Vector2(self.sSize.x / self.room.PixelWidth, self.sSize.y / self.room.PixelHeight);
                Shader.SetGlobalVector("_camInRoomRect", new Vector4(vector.x / self.room.PixelWidth + zooming.x / 2f, vector.y / self.room.PixelHeight + zooming.y / 2f,
                    self.sSize.x / self.room.PixelWidth - zooming.x, self.sSize.y / self.room.PixelHeight - zooming.y));
                Shader.SetGlobalVector("_screenSize", self.sSize);
            }
        }
        #endregion

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
