namespace SlugcatEyebrowRaise
{
    internal class Enums
    {
        public class Sounds
        {
            public static SoundID? VineBoom = new SoundID("VineBoom", true);
            public static SoundID? VineBoomLoud = new SoundID("VineBoomLoud", true);

            public static void RegisterValues()
            {
                VineBoom = new SoundID("VineBoom", true);
                VineBoomLoud = new SoundID("VineBoomLoud", true);
            }

            public static void UnregisterValues()
            {
                VineBoom?.Unregister();
                VineBoomLoud?.Unregister();
            }
        }
    }
}
