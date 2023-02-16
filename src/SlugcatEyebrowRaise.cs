using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;
using System.Runtime.CompilerServices;
using System.Security;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace SlugcatEyebrowRaise
{
    [BepInPlugin(MOD_ID + "." + AUTHOR, "Slugcat Eyebrow Raise", VERSION)]
    internal class SlugcatEyebrowRaise : BaseUnityPlugin
    {
        public const string VERSION = "1.0.5";
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
