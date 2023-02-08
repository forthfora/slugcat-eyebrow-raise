using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using Menu.Remix.MixedUI;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

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
            "When checked, the camera shakes under the sheer weight of raising your eyebrow.",
            null, "", "Camera Shake?"));

        public static Configurable<bool> zoomCamera = instance.config.Bind("zoomCamera", true, new ConfigurableInfo(
            "When checked, makes the camera zoom in on slugcat whenever they raise their eyebrow." +
            "\n(does not work well for jolly coop, will always focus on the first slugcat to enter the room on screen)",
            null, "", "Zoom Camera?"));


        public static Configurable<bool> vineBoomLoud = instance.config.Bind("vineBoomLoud", false, new ConfigurableInfo(
            "When checked, makes the vine boom literally 1000 times louder in the config file!" +
            "\nWARNING: EXTREMELY LOUD, I AM NOT RESPONSIBLE FOR ANY HEARING LOSS INCURRED",
            null, "", "Vine Boom Louder? " +
            "\n(MUCH LOUDER)"));

        public static Configurable<bool> playEveryFrame = instance.config.Bind("playEveryFrame", false, new ConfigurableInfo(
            "NEVER ENABLE THIS OPTION AND HOLD THE BUTTON, CAN CAUSE EXTREME DAMAGE TO BOTH EARS AND YOUR SANITY" + 
            "\nWhen checked, makes the vine boom sound play and stack every frame as long as the button is held down.",
            null, "", "DO NOT ENABLE"));

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

        private const int NUMBER_OF_TABS = 3;

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[NUMBER_OF_TABS];
            int tabIndex = -1;

            AddTab(ref tabIndex, "General");

            AddCheckBox(vineBoomExplosion, (string)vineBoomExplosion.info.Tags[0]);
            AddCheckBox(vineBoomCosmetics, (string)vineBoomCosmetics.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddCheckBox(cameraShake, (string)cameraShake.info.Tags[0]);
            AddCheckBox(zoomCamera, (string)zoomCamera.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);

            AddCheckBox(vineBoomLoud, (string)vineBoomLoud.info.Tags[0]);
            AddCheckBox(playEveryFrame, (string)playEveryFrame.info.Tags[0]);
            DrawCheckBoxes(ref Tabs[tabIndex]);


            AddNewLine(12);
            DrawBox(ref Tabs[tabIndex]);

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

            AddTab(ref tabIndex, "Animation");

            AddNewLine(2);

            // Requires stripped assembly to compile atm
            openSpritesDirectoryButton = new OpSimpleButton(new Vector2(pos.x + (marginX.y - marginX.x) / 2.0f - 125.0f, pos.y), new Vector2(250.0f, 30.0f), "OPEN SPRITES DIRECTORY")
            {
                colorEdge = new UnityEngine.Color(1f, 1f, 1f, 1f),
                colorFill = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.5f),
                description = "Opens the sprites directory." +
                "\nYou can add custom face sprites here!"
            };
            openSpritesDirectoryButton.OnClick += openSpritesDirectoryButton_OnClick;
            Tabs[tabIndex].AddItems(openSpritesDirectoryButton);

            AddNewLine();

            AddSlider(animationFrameRate, (string)animationFrameRate.info.Tags[0]);
            AddSlider(animationFrameCount, (string)animationFrameCount.info.Tags[0]);
            DrawSliders(ref Tabs[tabIndex]);

            AddNewLine(11);
            DrawBox(ref Tabs[tabIndex]);
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
