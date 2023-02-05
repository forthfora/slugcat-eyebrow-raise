using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace SlugcatEyebrowRaise
{
    [BepInPlugin(MOD_ID + AUTHOR, MOD_ID, VERSION)]
    internal class SlugcatEyebrowRaise : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";
        public const string MOD_ID = "slugcateyebrowraise";
        public const string AUTHOR = "forthbridge";

        public static new ManualLogSource Logger { get; private set; } = null!;

        public void OnEnable()
        {
            Logger = base.Logger;
            Hooks.ApplyHooks();
        }
    }
}
