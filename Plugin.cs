using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ShipPlusA.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using TerminalApi.Classes;
using UnityEngine;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace ShipPlusAMod
{
    [BepInPlugin(modGUID,modName,modVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class ShipModBase : BaseUnityPlugin
    {
        private const string modGUID = "Anonymouse.ShipPlusAMod";
        private const string modName = "Skip Plus";
        private const string modVersion = "1.0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static ShipModBase Instance;
        public static List<IShockableWithGun> entities = new List<IShockableWithGun>();

        internal static ManualLogSource Logger;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            Logger.LogInfo("Ship Plus awaken :DD");
            harmony.PatchAll(typeof(ShipModBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(PatcherToolPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));

            AddCommand("zapwave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    Logger.LogWarning("Bought ZAP Wave protection");
                    return "Bought ZAP WAVE protection\nTo use type: wave\n";
                },
                Category = "Other",
                Description = "Use to buy ZAP WAVE tool that protects the ship.\nType 'wave' to use."
            });
            AddCommand("wave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    Logger.LogWarning("USED wave");
                    List<GameObject> lasers = RoundManagerPatch.lasers;
                    if(lasers.Count == 0)
                    {
                        return "Can't use while in space... Land...\n\n";
                    }
                    foreach(GameObject laser in lasers)
                    {
                        PatcherTool l=laser.GetComponent<PatcherTool>();
                        // l.ActivateItemServerRpc(true, true);
                        l.ScanGun();
                    }
                    return "Used ZAP WAVE\n\n Lasers used:"+lasers.Count;
                },
                Category = "Other",
                Description = "Use ZAP WAVE to protect allies before they die!!!"
            });
        }
    }


}
