using System.Collections.Generic;
using System.Diagnostics;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace SlugcatEyebrowRaise
{   
    // Based on the options script from SBCameraScroll by SchuhBaum
    // https://github.com/SchuhBaum/SBCameraScroll/blob/Rain-World-v1.9/SourceCode/MainModOptions.cs
    public class Options : OptionInterface
    {
        public static Options instance = new Options();

        #region Options

        public static Configurable<bool> vineBoomExplosion = instance.config.Bind("vineBoomExplosion", true, new ConfigurableInfo(
            "When checked, raising your eyebrow generates an extremely powerful artificer-like explosion around the player that knocks creatures away." +
            "\nIndependent of the cosmetic effect.",
            null, "", "Vine Boom Explosion?"));

        public static Configurable<bool> vineBoomCosmetics = instance.config.Bind("vineBoomCosmetics", true, new ConfigurableInfo(
            "When checked, enables a small cosmetic effect whenever you raise your eyebrow!",
            null, "", "Vine Boom Cosmetics?"));


        public static Configurable<bool> cameraShake = instance.config.Bind("cameraShake", true, new ConfigurableInfo(
            "When checked, the camera shakes under the sheer power of raising one eyebrow!",
            null, "", "Camera Shake?"));

        public static Configurable<bool> zoomCamera = instance.config.Bind("zoomCamera", true, new ConfigurableInfo(
            "When checked, makes the camera zoom in on slugcat whenever they raise their eyebrow." +
            "\n(disabled for co-op modes as the camera cannot decide which player to zoom in on)",
            null, "", "Zoom Camera? (Experimental)"));


        public static Configurable<bool> vineBoomLoud = instance.config.Bind("vineBoomLoud", false, new ConfigurableInfo(
            "When checked, makes the vine boom literally 1000 times louder in the config file!" +
            "\nWARNING: EXTREMELY LOUD",
            null, "", "Vine Boom Louder? " +
            "\n(MUCH LOUDER)"));

        public static Configurable<bool> playEveryFrame = instance.config.Bind("playEveryFrame", false, new ConfigurableInfo(
            "ESPECIALLY DO NOT ENABLE BOTH THIS AND 'Vine Boom Louder?' AT THE SAME TIME" + 
            "\nWhen checked, makes the vine boom sound play and stack every frame as long as the button is held down.",
            null, "", "DO NOT ENABLE AND HOLD" +
            "\nTHE EYEBROW RAISE BUTTON"));

        public static Configurable<bool> eyebrowRaiseFriendlyFire = instance.config.Bind("eyebrowRaiseFriendlyFire", true, new ConfigurableInfo(
            "When checked, the eyebrow raise explosion will affect other players (if the explosion is enabled of course!)",
            null, "", "Eyebrow Raise Friendly Fire?"));

        public static Configurable<bool> affectsCarried = instance.config.Bind("affectsCarried", false, new ConfigurableInfo(
            "When checked, the eyebrow raise explosion will affect creatures you are carrying." +
            "\nThis could cause problems if you were using a squidcada, for example.",
            null, "", "Affects Carried Creatures?"));

        public static Configurable<bool> enableIllustrations = instance.config.Bind("enableIllustrations", true, new ConfigurableInfo(
            "When checked, gives a lot of in game illustrations an eyebrow raise!",
            null, "", "Eyebrow Raise Illustrations?"));

        public static Configurable<bool> disableGraphicsOverride = instance.config.Bind("disableGraphicsOverride", false, new ConfigurableInfo(
            "Disables the asset loader override, may fix compatibility with certain mods." +
            "\nWill likely break the illustration loading and the relevant illustration config!",
            null, "", "Disable Asset Override?"));

        public static Configurable<int> eyebrowRaiseVolume = instance.config.Bind("eyebrowRaiseVolume", 100, new ConfigurableInfo(
            "The volume of the raise when THE BUTTON is pressed - does not affect menus!",
            new ConfigAcceptableRange<int>(0, 200), "", "Eyebrow Raise Volume"));

        public static Configurable<int> eyebrowRaiseDuration = instance.config.Bind("eyebrowRaiseDuration", 25, new ConfigurableInfo(
            "How long does the eyebrow raise last?" +
            "\nMeasured in 10ths of seconds.",
            new ConfigAcceptableRange<int>(1, 100), "", "Eyebrow Raise Duration"));

        public static Configurable<int> eyebrowRaiseZoom = instance.config.Bind("eyebrowRaiseZoom", 15, new ConfigurableInfo(
            "How much does the eyebrow raise zoom in?",
            new ConfigAcceptableRange<int>(10, 100), "", "Eyebrow Raise Zoom"));

        public static Configurable<int> eyebrowRaiseZoomDuration = instance.config.Bind("eyebrowRaiseZoomDuration", 10, new ConfigurableInfo(
            "How long does the eyebrow raise zoom last?" +
            "\nMeasured in 10ths of seconds.",
            new ConfigAcceptableRange<int>(1, 100), "", "Eyebrow Raise Zoom Duration"));

        public static Configurable<KeyCode> keyboardKeybind = instance.config.Bind("keyboardKeybind", KeyCode.LeftAlt, new ConfigurableInfo(
            "Keybind to trigger the eyebrow raise for player 1.", null, "", "Keyboard Keybind"));

        public static Configurable<KeyCode> player1Keybind = instance.config.Bind("player1Keybind", KeyCode.Joystick1Button4, new ConfigurableInfo(
            "Keybind to trigger the eyebrow raise for player 1.", null, "", "Player 1 Keybind"));

        public static Configurable<KeyCode> player2Keybind = instance.config.Bind("player2Keybind", KeyCode.Joystick2Button4, new ConfigurableInfo(
            "Keybind to trigger the eyebrow raise for player 2", null, "", "Player 2 Keybind"));

        public static Configurable<KeyCode> player3Keybind = instance.config.Bind("player3Keybind", KeyCode.Joystick3Button4, new ConfigurableInfo(
            "Keybind to trigger the eyebrow raise for player 3.", null, "", "Player 3 Keybind"));

        public static Configurable<KeyCode> player4Keybind = instance.config.Bind("player4Keybind", KeyCode.Joystick4Button4, new ConfigurableInfo(
            "Keybind to trigger the eyebrow raise for player 4.", null, "", "Player 4 Keybind"));

        public static Configurable<int> eyebrowRaisePower = instance.config.Bind("eyebrowRaisePower", 100, new ConfigurableInfo(
            "How powerful is the knockback effect of the eyebrow raise? (30 is equivalent to artificer's parry)",
            new ConfigAcceptableRange<int>(1, 1000), "", "Eyebrow Raise Power"));

        public static Configurable<int> animationFrameRate = instance.config.Bind("animationFrameRate", 20, new ConfigurableInfo(
            "The frame rate of the eyebrow raise animation.",
            new ConfigAcceptableRange<int>(1, 60), "", "Frame Rate"));

        public static Configurable<int> animationFrameCount = instance.config.Bind("animationFrameCount", 3, new ConfigurableInfo(
            "The number of frames the eyebrow raise animation contains, with the count being equal to the last frame.",
            new ConfigAcceptableRange<int>(1, 10), "", "Frame Count"));

        private OpSimpleButton? openSpritesDirectoryButton;
        #endregion

        #region Parameters
        private readonly float spacing = 20f;
        private readonly float fontHeight = 20f;
        private readonly int numberOfCheckboxes = 2;
        private readonly float checkBoxSize = 60.0f;
        private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;
        #endregion

        #region Variables
        private Vector2 marginX = new();
        private Vector2 pos = new();

        private readonly List<float> boxEndPositions = new();

        private readonly List<Configurable<bool>> checkBoxConfigurables = new();
        private readonly List<OpLabel> checkBoxesTextLabels = new();

        private readonly List<Configurable<string>> comboBoxConfigurables = new();
        private readonly List<List<ListItem>> comboBoxLists = new();
        private readonly List<bool> comboBoxAllowEmpty = new();
        private readonly List<OpLabel> comboBoxesTextLabels = new();

        private readonly List<Configurable<int>> sliderConfigurables = new();
        private readonly List<string> sliderMainTextLabels = new();
        private readonly List<OpLabel> sliderTextLabelsLeft = new();
        private readonly List<OpLabel> sliderTextLabelsRight = new();

        private readonly List<OpLabel> textLabels = new();
        #endregion

        private const int NUMBER_OF_TABS = 4;

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[NUMBER_OF_TABS];
            int tabIndex = -1;

            #region General
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

            AddSlider(eyebrowRaisePower, (string)eyebrowRaisePower.info.Tags[0], "1", "1000");
            DrawSliders(ref Tabs[tabIndex]);

            AddCheckBox(vineBoomLoud, (string)vineBoomLoud.info.Tags[0]);
            AddCheckBox(playEveryFrame, (string)playEveryFrame.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddNewLine(1);
            DrawBox(ref Tabs[tabIndex]);
            #endregion

            #region Input
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
            #endregion

            #region Cosmetics

            AddTab(ref tabIndex, "Cosmetics");

            AddNewLine(2);

            // Requires stripped assembly to compile atm
            openSpritesDirectoryButton = new OpSimpleButton(new Vector2(pos.x + (marginX.y - marginX.x) / 2.0f - 125.0f, pos.y), new Vector2(250.0f, 30.0f), "OPEN SPRITES DIRECTORY")
            {
                colorEdge = new UnityEngine.Color(1f, 1f, 1f, 1f),
                colorFill = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.5f),
                description = "You can replace the PNGs in this folder with your own custom face sprites!"
            };
            openSpritesDirectoryButton.OnClick += openSpritesDirectoryButton_OnClick;
            Tabs[tabIndex].AddItems(openSpritesDirectoryButton);

            AddNewLine();

            AddSlider(animationFrameRate, (string)animationFrameRate.info.Tags[0], "1", "60");
            AddSlider(animationFrameCount, (string)animationFrameCount.info.Tags[0], "1", "10");
            DrawSliders(ref Tabs[tabIndex]);

            AddCheckBox(disableGraphicsOverride, (string)disableGraphicsOverride.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddNewLine(8);
            DrawBox(ref Tabs[tabIndex]);
            #endregion


            #region Volume & Zoom
            AddTab(ref tabIndex, "Volume & Zoom");

            AddSlider(eyebrowRaiseVolume, (string)eyebrowRaiseVolume.info.Tags[0], "0%", "please don't");
            AddSlider(eyebrowRaiseDuration, (string)eyebrowRaiseDuration.info.Tags[0], "0.1s", "10.0s");

            AddSlider(eyebrowRaiseZoom, (string)eyebrowRaiseZoom.info.Tags[0], "10%", "100%");
            AddSlider(eyebrowRaiseZoomDuration, (string)eyebrowRaiseZoomDuration.info.Tags[0], "0.1s", "10.0s");
            DrawSliders(ref Tabs[tabIndex]);
            
            AddNewLine(6);
            DrawBox(ref Tabs[tabIndex]);
            #endregion
        }

        private void openSpritesDirectoryButton_OnClick(UIfocusable trigger) => Process.Start(AssetManager.ResolveDirectory(ResourceLoader.SPRITES_DIRPATH));


        #region UI Elements
        private void AddTab(ref int tabIndex, string tabName)
        {
            tabIndex++;
            Tabs[tabIndex] = new OpTab(this, tabName);
            InitializeMarginAndPos();

            AddNewLine();
            AddTextLabel("Slugcat Eyebrow Raise", bigText: true);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLine(0.5f);
            AddTextLabel("Version " + SlugcatEyebrowRaise.VERSION, FLabelAlignment.Left);
            AddTextLabel("by " + SlugcatEyebrowRaise.AUTHOR, FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[tabIndex]);

            AddNewLine();
            AddBox();
        }

        private void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        private void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y); // end position > start position
            AddNewLine();
        }

        private void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;

            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }

        private void AddCheckBox(Configurable<bool> configurable, string text)
        {
            checkBoxConfigurables.Add(configurable);
            checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
        }

        private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
        {
            if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

            float width = marginX.y - marginX.x;
            float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
            pos.y -= checkBoxSize;
            float _posX = pos.x;

            for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
            {
                Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
                OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
                {
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(checkBox);
                _posX += CheckBoxWithSpacing;

                OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
                checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
                checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
                tab.AddItems(checkBoxLabel);

                if (checkBoxIndex < checkBoxConfigurables.Count - 1)
                {
                    if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                    {
                        AddNewLine();
                        pos.y -= checkBoxSize;
                        _posX = pos.x;
                    }
                    else
                    {
                        _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                    }
                }
            }

            checkBoxConfigurables.Clear();
            checkBoxesTextLabels.Clear();
        }

        private void AddComboBox(Configurable<string> configurable, List<ListItem> list, string text, bool allowEmpty = false)
        {
            OpLabel opLabel = new(new Vector2(), new Vector2(0.0f, fontHeight), text, FLabelAlignment.Center, false);
            comboBoxesTextLabels.Add(opLabel);
            comboBoxConfigurables.Add(configurable);
            comboBoxLists.Add(list);
            comboBoxAllowEmpty.Add(allowEmpty);
        }

        private void DrawComboBoxes(ref OpTab tab)
        {
            if (comboBoxConfigurables.Count != comboBoxesTextLabels.Count) return;
            if (comboBoxConfigurables.Count != comboBoxLists.Count) return;
            if (comboBoxConfigurables.Count != comboBoxAllowEmpty.Count) return;

            float offsetX = (marginX.y - marginX.x) * 0.1f;
            float width = (marginX.y - marginX.x) * 0.4f;

            for (int comboBoxIndex = 0; comboBoxIndex < comboBoxConfigurables.Count; ++comboBoxIndex)
            {
                AddNewLine(1.25f);
                pos.x += offsetX;

                OpLabel opLabel = comboBoxesTextLabels[comboBoxIndex];
                opLabel.pos = pos;
                opLabel.size += new Vector2(width, 2f); // size.y is already set
                pos.x += width;

                Configurable<string> configurable = comboBoxConfigurables[comboBoxIndex];
                OpComboBox comboBox = new(configurable, pos, width, comboBoxLists[comboBoxIndex])
                {
                    allowEmpty = comboBoxAllowEmpty[comboBoxIndex],
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(opLabel, comboBox);

                // don't add a new line on the last element
                if (comboBoxIndex < comboBoxConfigurables.Count - 1)
                {
                    AddNewLine();
                    pos.x = marginX.x;
                }
            }

            comboBoxesTextLabels.Clear();
            comboBoxConfigurables.Clear();
            comboBoxLists.Clear();
            comboBoxAllowEmpty.Clear();
        }

        private void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
        {
            sliderConfigurables.Add(configurable);
            sliderMainTextLabels.Add(text);
            sliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
            sliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
        }

        private void DrawSliders(ref OpTab tab)
        {
            if (sliderConfigurables.Count != sliderMainTextLabels.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsLeft.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsRight.Count) return;

            float width = marginX.y - marginX.x;
            float sliderCenter = marginX.x + 0.5f * width;
            float sliderLabelSizeX = 0.2f * width;
            float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

            for (int sliderIndex = 0; sliderIndex < sliderConfigurables.Count; ++sliderIndex)
            {
                AddNewLine(2f);

                OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
                opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                Configurable<int> configurable = sliderConfigurables[sliderIndex];
                OpSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), (int)sliderSizeX)
                {
                    size = new Vector2(sliderSizeX, fontHeight),
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(slider);

                opLabel = sliderTextLabelsRight[sliderIndex];
                opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                AddTextLabel(sliderMainTextLabels[sliderIndex]);
                DrawTextLabels(ref tab);

                if (sliderIndex < sliderConfigurables.Count - 1)
                {
                    AddNewLine();
                }
            }

            sliderConfigurables.Clear();
            sliderMainTextLabels.Clear();
            sliderTextLabelsLeft.Clear();
            sliderTextLabelsRight.Clear();
        }

        private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        private void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }
        #endregion
    }
}
