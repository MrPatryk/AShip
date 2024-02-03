using GameNetcodeStuff;
using HarmonyLib;
using LC_API.GameInterfaceAPI.Features;
using ShipPlusA.Scripts;
using ShipPlusAMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace ShipPlusA.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        public static List<GameObject> laser = new List<GameObject>();

        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPostfix]
        static void refr(ref RoundManager __instance)
        {
            if(ShipModBase.checkShow()) if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>open door call");
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>check host");
            //if (HostCheck) return;
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>host ...");
            Transform existingChild = __instance.transform.Find("LightningScript");

            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>check");
            if (existingChild == null)
            {
                if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>nie ma dziecka, tworze");
                for (int i = 0; i < ShipModBase.upgrades[ShipModBase.upgradeLevel].amount; i++)
                {
                    GameObject childOb = i == 0 ? new GameObject("LightningScript") : new GameObject("LightningScript" + i);
                    childOb.AddComponent<LightningScript>();
                    childOb.transform.SetParent(__instance.transform);
                    laser.Add(childOb);
                }

            }
            else
            {
                if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>ma juz dziecko");

            }
        }
        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPostfix]
        static void refclose(ref RoundManager __instance)
        {
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>close door call");
            laser.Clear();
        }
    }
}
