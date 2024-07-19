using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

#pragma warning disable CS0618 // Type or member is obsolete

namespace SlugcatEyebrowRaise;

public static class Hooks
{
    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }


    private static bool _isInit;

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, ModOptions.Instance);

            if (_isInit) return;

            _isInit = true;

            _ = Enums.Sounds.VineBoom;

            ResourceLoader.LoadSprites();

            ApplyHooks();

            var mod = ModManager.ActiveMods.First(mod => mod.id == Plugin.MOD_ID);

            Plugin.MOD_NAME = mod.name;
            Plugin.VERSION = mod.version;
            Plugin.AUTHORS = "forthbridge";
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError(ex.Message + "\n" + ex.StackTrace);
        }
        finally
        {
            orig(self);
        }
    }

    public static void ApplyHooks()
    {
        On.Player.Update += Player_Update;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

        On.RoomCamera.DrawUpdate += RoomCamera_DrawUpdate;

        On.Menu.RainEffect.LightningSpike += RainEffect_LightningSpike;

        On.AssetManager.ResolveFilePath_string_bool += (orig, path, mods) =>
        {
            if (ModOptions.enableIllustrations.Value)
            {
                return orig(path, mods);
            }

            if (!path.Contains("Illustrations") && !path.Contains("Scenes"))
            {
                return orig(path, mods);
            }

            var thisMod = ModManager.ActiveMods.FirstOrDefault(x => x.id == Plugin.MOD_ID);

            if (thisMod is null)
            {
                return orig(path, mods);
            }

            var index = ModManager.ActiveMods.IndexOf(thisMod);
            ModManager.ActiveMods.Remove(thisMod);

            var result = orig(path, mods);

            ModManager.ActiveMods.Insert(index, thisMod);

            return result;
        };

        try
        {
            IL.Player.Die += Player_Die;
            IL.HUD.TextPrompt.Update += TextPrompt_Update;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("IL Hook Error: " + e.Message + "\n" + e.StackTrace);
        }
    }


    private static SoundID GetVineBoomSoundID() => ModOptions.vineBoomLoud.Value ? Enums.Sounds.VineBoomLoud : Enums.Sounds.VineBoom;

    private const int MAX_NUMBER_OF_PLAYERS = 4;

    private const float SHAKE_DURATION = 1.5f;
    private const float SHAKE_INTENSITY_NORMAL = 0.15f;
    private const float SHAKE_INTENSITY_LOUD = 1.0f;

    private static readonly bool[] isPlayerKeyPressed = new bool[MAX_NUMBER_OF_PLAYERS];
    private static readonly bool[] isPlayerEyebrowRaised = new bool[MAX_NUMBER_OF_PLAYERS];
    private static readonly int[] playerEyebrowRaiseLevel = new int[MAX_NUMBER_OF_PLAYERS];
    private static readonly float[] playerEyebrowRaiseDurationTimer = new float[MAX_NUMBER_OF_PLAYERS];
    private static readonly float[] raiseTimers = new float[MAX_NUMBER_OF_PLAYERS];

    private static float cameraZoomAmount;
    private static float shakeTimer;
    private static float zoomTimer;


    private static void RainEffect_LightningSpike(On.Menu.RainEffect.orig_LightningSpike orig, Menu.RainEffect self, float newInt, float dropOffFrames)
    {
        orig(self, newInt, dropOffFrames);

        if (!(self.lightningIntensity > 0.3f)) return;
        self.menu.PlaySound(GetVineBoomSoundID(), 0.0f, self.lightningIntensity + 0.2f, 0.5f);
    }


    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);

        HandlePlayerInput(self, ModOptions.player1Keybind.Value, 0);
        HandlePlayerInput(self, ModOptions.player2Keybind.Value, 1);
        HandlePlayerInput(self, ModOptions.player3Keybind.Value, 2);
        HandlePlayerInput(self, ModOptions.player4Keybind.Value, 3);

        if (zoomTimer < Time.time)
        {
            cameraZoomAmount = 0.0f;
        }
    }


    private static void HandlePlayerInput(Player player, KeyCode keyCode, int targetPlayerIndex)
    {
        var playerIndex = player.playerState.playerNumber;
        if (playerIndex != targetPlayerIndex) return;

        if (Input.GetKey(keyCode) || (playerIndex == 0 && Input.GetKey(ModOptions.keyboardKeybind.Value)))
        {
            if (!isPlayerKeyPressed[playerIndex] || ModOptions.playEveryFrame.Value)
            {
                player.room.PlaySound(GetVineBoomSoundID(), player.mainBodyChunk, false, ModOptions.eyebrowRaiseVolume.Value / 100.0f, 1.0f);

                playerEyebrowRaiseDurationTimer[playerIndex] = Time.time + (ModOptions.eyebrowRaiseDuration.Value / 10.0f);

                EyebrowRaiseExplosion(player);

                if (ModOptions.cameraShake.Value)
                {
                    shakeTimer = Time.time + SHAKE_DURATION;
                }

                if (ModOptions.zoomCamera.Value && player.room.game.Players.Count == 1)
                {
                    cameraZoomAmount = (ModOptions.eyebrowRaiseZoom.Value / 100.0f);
                    zoomTimer = Time.time + (ModOptions.eyebrowRaiseZoomDuration.Value / 10.0f);
                }
            }

            isPlayerEyebrowRaised[playerIndex] = true;

            if (!player.isSlugpup)
            {
                isPlayerKeyPressed[playerIndex] = true;
            }
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


    // This is based on artificer's parry code, with quite a few adjustments!
    private static void EyebrowRaiseExplosion(Player player)
    {
        var pos2 = player.firstChunk.pos;

        if (ModOptions.vineBoomCosmetics.Value)
        {
            player.room.AddObject(new Explosion.ExplosionLight(pos2, 100.0f, 0.2f, 16, Color.white));
            player.room.AddObject(new ShockWave(pos2, 1000f, 0.05f, 2, false));

            for (var l = 0; l < 10; l++)
            {
                var a2 = Custom.RNV();
                player.room.AddObject(new Spark(pos2 + a2 * Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 3, 6));
            }
        }

        if (!ModOptions.vineBoomExplosion.Value) return;

        var weapons = new List<Weapon>();

        foreach (var objectLayer in player.room.physicalObjects)
        {
            foreach (var physicalObject in objectLayer)
            {
                if (physicalObject is Weapon weapon)
                {
                    if (weapon?.mode == Weapon.Mode.Thrown && Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                    {
                        weapons.Add(weapon);
                    }
                }

                if (physicalObject is Creature creature && creature != player)
                {
                    if (Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (Custom.Dist(pos2, creature.firstChunk.pos) < 60f || player.room.VisualContact(player.abstractCreature.pos, creature.abstractCreature.pos)))
                    {
                        player.room.socialEventRecognizer.WeaponAttack(null, player, creature, true);
                        creature.SetKillTag(player.abstractCreature);

                        ApplyKnockback(pos2, creature, player);
                    }
                }
            }
        }

        if (weapons.Count > 0 && player.room.game.IsArenaSession)
        {
            player.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
        }

        foreach (var weapon in weapons)
        {
            weapon.ChangeMode(Weapon.Mode.Free);
            weapon.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, weapon.firstChunk.pos)) * 200.0f;
            weapon.SetRandomSpin();
        }

    }


    private static void ApplyKnockback(Vector2 pos2, Creature creature, Player player)
    {
        // Do not affect players if friendly fire is off
        if (creature is Player && !ModOptions.eyebrowRaiseFriendlyFire.Value) return;

        // Do not affect held creatures held by the player, unless the option is enabled
        if (!ModOptions.affectsCarried.Value)
        {
            if (creature.grabbedBy.Any(t => t.grabber == player)) return;
        }

        // Do not affect carried / carrying players
        if (creature is Player playerCreature)
        {
            // Creature is on our back
            if (!player.isSlugpup && playerCreature == player.slugOnBack.slugcat) return;

            // We are on the creature's back
            if (!playerCreature.isSlugpup && playerCreature.slugOnBack.slugcat == player) return;

            // Do not affect players holding us
            if (player.grabbedBy.Any(t => !ModOptions.affectsCarried.Value && t.grabber == playerCreature)) return;
        }

        creature.firstChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * ModOptions.eyebrowRaisePower.Value;

        // Scavengers have a unique stun mechanic(?)
        if (creature is Scavenger scavenger)
        {
            scavenger.HeavyStun(80);
        }
        else if (creature is not Player)
        {
            creature.Stun(80);
        }

        // Force tentacle plants to release you
        if (creature is TentaclePlant)
        {
            for (var num5 = 0; num5 < creature.grasps.Length; num5++)
            {
                creature.ReleaseGrasp(num5);
            }
        }
    }


    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        var playerIndex = self.player.playerState.playerNumber;
        var raiseLevel = playerEyebrowRaiseLevel[playerIndex];

        if (Time.time - raiseTimers[playerIndex] > 1.0f / ModOptions.animationFrameRate.Value)
        {
            raiseTimers[playerIndex] = Time.time;

            if (isPlayerEyebrowRaised[playerIndex])
            {
                if (raiseLevel < ModOptions.animationFrameCount.Value)
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

        var face = GetFace(self, raiseLevel);
        if (face != null) SetFaceSprite(sLeaser, face);
    }


    private static string? GetFace(PlayerGraphics self, int raiseLevel)
    {
        if (raiseLevel == 0) return null;
        
        var name = self.player.SlugCatClass;
        var face = "default";

        if (self.player.dead)
        {
            face = "dead";
        }
        else if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
        {
            face = "artificer";
        }
        else if (name == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
        {
            face = "saint";
        }

        if (self.blink > 0 && raiseLevel == ModOptions.animationFrameCount.Value && !self.player.dead)
        {
            face += "_blink";
        }
        else
        {
            face += "_" + raiseLevel;
        }

        return Plugin.MOD_ID + "_" + face;
    }


    private static void SetFaceSprite(RoomCamera.SpriteLeaser sLeaser, string spriteName)
    {
        if (!Futile.atlasManager.DoesContainElementWithName(spriteName))
        {
            Plugin.Logger.LogError($"Missing sprite ({spriteName})! Please check the sprites directory under the mod's folder");
            return;
        }
        sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(spriteName);
        sLeaser.sprites[9].scaleX = 1;
    }



    // Henpemaz's magic

    private static void RoomCamera_DrawUpdate(On.RoomCamera.orig_DrawUpdate orig, RoomCamera self, float timeStacker, float timeSpeed)
    {
        if (shakeTimer > Time.time)
        {
            self.screenShake = ModOptions.vineBoomLoud.Value ? SHAKE_INTENSITY_LOUD : SHAKE_INTENSITY_NORMAL;
        }

        var zoom = 1f;
        var zoomed = false;
        var offset = Vector2.zero;

        #region Follow & Zoom

        // Have camera follow the player
        if (self.room != null && cameraZoomAmount > 0f)
        {
            zoom = cameraZoomAmount * 20f;
            zoomed = true;
            
            var creature = self.followAbstractCreature == null ? null : self.followAbstractCreature.realizedCreature;
            
            if (creature != null)
            {
                var value = Vector2.Lerp(creature.bodyChunks[0].lastPos, creature.bodyChunks[0].pos, timeStacker);
                if (creature.inShortcut)
                {
                    var vector = self.room.game.shortcuts.OnScreenPositionOfInShortCutCreature(self.room, creature);
                    if (vector != null)
                    {
                        value = vector.Value;
                    }
                }
                offset = new Vector2(self.cameraNumber * 6000f, 0f) + (value - (self.pos + self.sSize / 2f));
            }

        }

        // Zoom in
        if (zoomed)
        {
            // 11 useful layers, the reset is HUD
            for (var i = 0; i < 11; i++)
            {
                self.SpriteLayers[i].scale = 1.0f;
                self.SpriteLayers[i].SetPosition(Vector2.zero);
                self.SpriteLayers[i].ScaleAroundPointRelative(self.sSize / 2f, zoom, zoom);
            }
            self.offset = offset;
        }
        else
        {
            // Unzoom camera on effect slider to 0 or maybe if ChangeRoom didnt call
            for (var i = 0; i < 11; i++)
            {
                self.SpriteLayers[i].scale = 1f;
                self.SpriteLayers[i].SetPosition(Vector2.zero);
            }
            self.offset = new Vector2(self.cameraNumber * 6000.0f, 0.0f);
        }

        var randomSeed = 0;

        if (zoomed)
        {
            // deterministic random shake
            randomSeed = Random.seed;
            Random.seed = randomSeed;
        }

        orig(self, timeStacker, timeSpeed);

        #endregion

        if (zoomed)
        {
            Random.seed = randomSeed;
            var shakeOffset = Vector2.Lerp(self.lastPos, self.pos, timeStacker);

            if (self.microShake > 0f)
            {
                shakeOffset += Custom.RNV() * 8f * self.microShake * Random.value;
            }
        
            if (!self.voidSeaMode)
            {
                shakeOffset.x = Mathf.Clamp(shakeOffset.x, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f - 20f, self.CamPos(self.currentCameraPosition).x + self.hDisplace + 8f + 20f);
                shakeOffset.y = Mathf.Clamp(shakeOffset.y, self.CamPos(self.currentCameraPosition).y + 8f - 7f, self.CamPos(self.currentCameraPosition).y + 33f);
            }
            else
            {
                shakeOffset.y = Mathf.Min(shakeOffset.y, -528f);
            }

            shakeOffset = new Vector2(Mathf.Floor(shakeOffset.x), Mathf.Floor(shakeOffset.y));
            shakeOffset.x -= 0.02f;
            shakeOffset.y -= 0.02f;

            var magicOffset = self.CamPos(self.currentCameraPosition) - shakeOffset;
            var textureOffset = shakeOffset + magicOffset;

            //Vector4 center = new Vector4(
            //	(-shakeOffset.x - 0.5f + self.levelGraphic.width / 2f + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
            //	(-shakeOffset.y + 0.5f + self.levelGraphic.height / 2f + self.CamPos(self.currentCameraPosition).y) / self.sSize.y,
            //	(-shakeOffset.x - 0.5f + self.levelGraphic.width / 2f + self.CamPos(self.currentCameraPosition).x) / self.sSize.x,
            //	(-shakeOffset.y + 0.5f + self.levelGraphic.height / 2f + self.CamPos(self.currentCameraPosition).y) / self.sSize.y);

            var center = new Vector4(
                (magicOffset.x + self.levelGraphic.width / 2f) / self.sSize.x,
                (magicOffset.y + 2f + self.levelGraphic.height / 2f) / self.sSize.y,
                (magicOffset.x + self.levelGraphic.width / 2f) / self.sSize.x,
                (magicOffset.y + 2f + self.levelGraphic.height / 2f) / self.sSize.y);

            shakeOffset += self.offset;
            
            var spriteRectPos = new Vector4(
                (-shakeOffset.x + textureOffset.x) / self.sSize.x,
                (-shakeOffset.y + textureOffset.y) / self.sSize.y,
                (-shakeOffset.x + self.levelGraphic.width + textureOffset.x) / self.sSize.x,
                (-shakeOffset.y + self.levelGraphic.height + textureOffset.y) / self.sSize.y);

            //spriteRectPos -= new Vector4(17f / self.sSize.x, 18f / self.sSize.y, 17f / self.sSize.x, 18f / self.sSize.y) * (1f - 1f / zoom);

            spriteRectPos -= center;
            spriteRectPos *= zoom;
            spriteRectPos += center;

            Shader.SetGlobalVector("_spriteRect", spriteRectPos);


            if (self.room is not null)
            {
                var zooming = (1f - 1f / zoom) * new Vector2(self.sSize.x / self.room.PixelWidth, self.sSize.y / self.room.PixelHeight);

                Shader.SetGlobalVector("_camInRoomRect", new Vector4(
                    shakeOffset.x / self.room.PixelWidth + zooming.x / 2f,
                    shakeOffset.y / self.room.PixelHeight + zooming.y / 2f,
                    self.sSize.x / self.room.PixelWidth - zooming.x,
                    self.sSize.y / self.room.PixelHeight - zooming.y));
            }

            Shader.SetGlobalVector("_screenSize", self.sSize);
        }
    }



    // Death IL Hooks

    private static void Player_Die(ILContext il)
    {
        var c = new ILCursor(il);

        while (c.TryGotoNext(MoveType.AfterLabel,
            i => i.MatchLdsfld<SoundID>("Inv_GO")
            ))
        {
            c.Index += 8;
            c.EmitDelegate<Action<Player>>((p) =>
            {
                p.room.PlaySound(GetVineBoomSoundID(), p.mainBodyChunk);
            });
            break;
        }

        while (c.TryGotoNext(MoveType.AfterLabel,
            x => x.MatchLdsfld<SoundID>("UI_Slugcat_Die")
            ))
        {
            c.Remove();
            c.Emit<Enums.Sounds>(OpCodes.Ldsfld, GetVineBoomSoundID().ToString());
            break;
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
            c.Emit<Enums.Sounds>(OpCodes.Ldsfld, GetVineBoomSoundID().ToString());
            break;
        }
    }
}
