using System.Diagnostics;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace SlugcatEyebrowRaise;

public sealed class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();


    public static Configurable<bool> vineBoomExplosion = Instance.config.Bind("vineBoomExplosion", true, new ConfigurableInfo(
        "When checked, raising your eyebrow generates an extremely powerful artificer-like explosion around the player that knocks creatures away." +
        "\nIndependent of the cosmetic effect.",
        null, "", "Vine Boom Explosion?"));

    public static Configurable<bool> vineBoomCosmetics = Instance.config.Bind("vineBoomCosmetics", true, new ConfigurableInfo(
        "When checked, enables a small cosmetic effect whenever you raise your eyebrow!",
        null, "", "Vine Boom Cosmetics?"));


    public static Configurable<bool> cameraShake = Instance.config.Bind("cameraShake", true, new ConfigurableInfo(
        "When checked, the camera shakes under the sheer power of raising one eyebrow!",
        null, "", "Camera Shake?"));

    public static Configurable<bool> zoomCamera = Instance.config.Bind("zoomCamera", true, new ConfigurableInfo(
        "When checked, makes the camera zoom in on slugcat whenever they raise their eyebrow." +
        "\n(disabled for co-op modes as the camera cannot decide which player to zoom in on)",
        null, "", "Zoom Camera? (Experimental)"));


    public static Configurable<bool> vineBoomLoud = Instance.config.Bind("vineBoomLoud", false, new ConfigurableInfo(
        "When checked, makes the vine boom literally 1000 times louder in the config file!" +
        "\nWARNING: EXTREMELY LOUD",
        null, "", "Vine Boom Louder? " +
                  "\n(MUCH LOUDER)"));

    public static Configurable<bool> playEveryFrame = Instance.config.Bind("playEveryFrame", false, new ConfigurableInfo(
        "ESPECIALLY DO NOT ENABLE BOTH THIS AND 'Vine Boom Louder?' AT THE SAME TIME" +
        "\nWhen checked, makes the vine boom sound play and stack every frame as long as the button is held down.",
        null, "", "DO NOT ENABLE AND HOLD" +
                  "\nTHE EYEBROW RAISE BUTTON"));

    public static Configurable<bool> eyebrowRaiseFriendlyFire = Instance.config.Bind("eyebrowRaiseFriendlyFire", true, new ConfigurableInfo(
        "When checked, the eyebrow raise explosion will affect other players (if the explosion is enabled of course!)",
        null, "", "Eyebrow Raise Friendly Fire?"));

    public static Configurable<bool> affectsCarried = Instance.config.Bind("affectsCarried", false, new ConfigurableInfo(
        "When checked, the eyebrow raise explosion will affect creatures you are carrying." +
        "\nThis could cause problems if you were using a squidcada, for example.",
        null, "", "Affects Carried Creatures?"));

    public static Configurable<bool> enableIllustrations = Instance.config.Bind("enableIllustrations", true, new ConfigurableInfo(
        "When checked, gives a lot of in game illustrations an eyebrow raise!",
        null, "", "Eyebrow Raise Illustrations?"));

    public static Configurable<int> eyebrowRaiseVolume = Instance.config.Bind("eyebrowRaiseVolume", 100, new ConfigurableInfo(
        "The volume of the raise when THE BUTTON is pressed - does not affect menus!",
        new ConfigAcceptableRange<int>(0, 200), "", "Eyebrow Raise Volume"));

    public static Configurable<int> eyebrowRaiseDuration = Instance.config.Bind("eyebrowRaiseDuration", 25, new ConfigurableInfo(
        "How long does the eyebrow raise last?" +
        "\nMeasured in 10ths of seconds.",
        new ConfigAcceptableRange<int>(1, 100), "", "Eyebrow Raise Duration"));

    public static Configurable<int> eyebrowRaiseZoom = Instance.config.Bind("eyebrowRaiseZoom", 15, new ConfigurableInfo(
        "How much does the eyebrow raise zoom in?",
        new ConfigAcceptableRange<int>(10, 100), "", "Eyebrow Raise Zoom"));

    public static Configurable<int> eyebrowRaiseZoomDuration = Instance.config.Bind("eyebrowRaiseZoomDuration", 10, new ConfigurableInfo(
        "How long does the eyebrow raise zoom last?" +
        "\nMeasured in 10ths of seconds.",
        new ConfigAcceptableRange<int>(1, 100), "", "Eyebrow Raise Zoom Duration"));

    public static Configurable<KeyCode> keyboardKeybind = Instance.config.Bind("keyboardKeybind", KeyCode.LeftAlt, new ConfigurableInfo(
        "Keybind to trigger the eyebrow raise for player 1.", null, "", "Keyboard Keybind"));

    public static Configurable<KeyCode> player1Keybind = Instance.config.Bind("player1Keybind", KeyCode.Joystick1Button4, new ConfigurableInfo(
        "Keybind to trigger the eyebrow raise for player 1.", null, "", "Player 1 Keybind"));

    public static Configurable<KeyCode> player2Keybind = Instance.config.Bind("player2Keybind", KeyCode.Joystick2Button4, new ConfigurableInfo(
        "Keybind to trigger the eyebrow raise for player 2", null, "", "Player 2 Keybind"));

    public static Configurable<KeyCode> player3Keybind = Instance.config.Bind("player3Keybind", KeyCode.Joystick3Button4, new ConfigurableInfo(
        "Keybind to trigger the eyebrow raise for player 3.", null, "", "Player 3 Keybind"));

    public static Configurable<KeyCode> player4Keybind = Instance.config.Bind("player4Keybind", KeyCode.Joystick4Button4, new ConfigurableInfo(
        "Keybind to trigger the eyebrow raise for player 4.", null, "", "Player 4 Keybind"));

    public static Configurable<int> eyebrowRaisePower = Instance.config.Bind("eyebrowRaisePower", 100, new ConfigurableInfo(
        "How powerful is the knockback effect of the eyebrow raise? (30 is equivalent to artificer's parry)",
        new ConfigAcceptableRange<int>(1, 1000), "", "Eyebrow Raise Power"));

    public static Configurable<int> animationFrameRate = Instance.config.Bind("animationFrameRate", 20, new ConfigurableInfo(
        "The frame rate of the eyebrow raise animation.",
        new ConfigAcceptableRange<int>(1, 60), "", "Frame Rate"));

    public static Configurable<int> animationFrameCount = Instance.config.Bind("animationFrameCount", 3, new ConfigurableInfo(
        "The number of frames the eyebrow raise animation contains, with the count being equal to the last frame.",
        new ConfigAcceptableRange<int>(1, 10), "", "Frame Count"));



    private const int NUMBER_OF_TABS = 4;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[NUMBER_OF_TABS];
        var tabIndex = -1;

        // General

        AddTab(ref tabIndex, "General");

        AddCheckBox(vineBoomExplosion, (string)vineBoomExplosion.info.Tags[0]);
        AddCheckBox(zoomCamera, (string)zoomCamera.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(eyebrowRaiseFriendlyFire, (string)eyebrowRaiseFriendlyFire.info.Tags[0]);
        AddCheckBox(affectsCarried, (string)affectsCarried.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);


        AddCheckBox(vineBoomCosmetics, (string)vineBoomCosmetics.info.Tags[0]);
        AddCheckBox(cameraShake, (string)cameraShake.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddCheckBox(enableIllustrations, (string)enableIllustrations.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(2);

        AddIntSlider(eyebrowRaisePower, (string)eyebrowRaisePower.info.Tags[0], "1", "1000");
        DrawIntSliders(ref Tabs[tabIndex]);

        AddCheckBox(vineBoomLoud, (string)vineBoomLoud.info.Tags[0]);
        AddCheckBox(playEveryFrame, (string)playEveryFrame.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(1);
        DrawBox(ref Tabs[tabIndex]);


        // Input

        AddTab(ref tabIndex, "Input");

        Tabs[tabIndex].AddItems(
            new OpLabel(new Vector2(40f, 450f), new Vector2(100f, 34f), "Keyboard")
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = keyboardKeybind.info.description
            },
            new OpKeyBinder(keyboardKeybind, new Vector2(160f, 452f), new Vector2(146f, 30f), false)
        );

        Tabs[tabIndex].AddItems(
            new OpLabel(new Vector2(40f, 350f), new Vector2(100f, 34f), "Player 1")
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = player1Keybind.info.description
            },
            new OpKeyBinder(player1Keybind, new Vector2(160f, 352f), new Vector2(146f, 30f), false)
        );

        Tabs[tabIndex].AddItems(
            new OpLabel(new Vector2(40f, 250f), new Vector2(100f, 34f), "Player 2")
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = player2Keybind.info.description
            },
            new OpKeyBinder(player2Keybind, new Vector2(160f, 252f), new Vector2(146f, 30f), false)
        );

        Tabs[tabIndex].AddItems(
            new OpLabel(new Vector2(40f, 150f), new Vector2(100f, 34f), "Player 3")
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = player3Keybind.info.description
            },
            new OpKeyBinder(player3Keybind, new Vector2(160f, 152f), new Vector2(146f, 30f), false)
        );

        Tabs[tabIndex].AddItems(
            new OpLabel(new Vector2(40f, 50f), new Vector2(100f, 34f), "Player 4")
            {
                alignment = FLabelAlignment.Right,
                verticalAlignment = OpLabel.LabelVAlignment.Center,
                description = player4Keybind.info.description
            },
            new OpKeyBinder(player4Keybind, new Vector2(160f, 52f), new Vector2(146f, 30f), false)
        );

        AddNewLine(21);
        DrawBox(ref Tabs[tabIndex]);


        // Cosmetics

        AddTab(ref tabIndex, "Cosmetics");

        AddNewLine(2);
        AddNewLine();

        AddIntSlider(animationFrameRate, (string)animationFrameRate.info.Tags[0], "1", "60");
        AddIntSlider(animationFrameCount, (string)animationFrameCount.info.Tags[0], "1", "10");
        DrawIntSliders(ref Tabs[tabIndex]);

        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(8);
        DrawBox(ref Tabs[tabIndex]);


        // Volume & Zoom

        AddTab(ref tabIndex, "Volume & Zoom");

        AddIntSlider(eyebrowRaiseVolume, (string)eyebrowRaiseVolume.info.Tags[0], "0%", "please don't");
        AddIntSlider(eyebrowRaiseDuration, (string)eyebrowRaiseDuration.info.Tags[0], "0.1s", "10.0s");

        AddIntSlider(eyebrowRaiseZoom, (string)eyebrowRaiseZoom.info.Tags[0], "10%", "100%");
        AddIntSlider(eyebrowRaiseZoomDuration, (string)eyebrowRaiseZoomDuration.info.Tags[0], "0.1s", "10.0s");
        DrawIntSliders(ref Tabs[tabIndex]);

        AddNewLine(6);
        DrawBox(ref Tabs[tabIndex]);
    }
}
