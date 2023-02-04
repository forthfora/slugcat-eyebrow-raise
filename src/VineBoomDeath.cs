using BepInEx;
using System.Security.Permissions;
using BepInEx.Logging;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace VineBoomDeath
{
    [BepInPlugin("forthbridge.vineboomdeath", "VineBoomDeath", "1.0.0")]
    internal class VineBoomDeath : BaseUnityPlugin
    {
        public static new ManualLogSource Logger { get; private set; } = null!;


        public void OnEnable()
        {
            Logger = base.Logger;

            Hooks.ApplyHooks();
        }
    }
}
