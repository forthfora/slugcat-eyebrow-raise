namespace VineBoomDeath
{
    internal class Enums
    {
        public class Sounds
        {
            private static SoundID? UI_Slugcat_VineBoomDie = new SoundID("UI_Slugcat_VineBoomDie", true);

            public static void RegisterValues()
            {
                UI_Slugcat_VineBoomDie = new SoundID("UI_Slugcat_VineBoomDie", true);
            }

            public static void UnregisterValues()
            {
                UI_Slugcat_VineBoomDie?.Unregister();
            }
        }
    }
}
