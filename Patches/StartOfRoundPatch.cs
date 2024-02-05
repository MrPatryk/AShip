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
using Unity;
using UnityEngine;
using UnityEngine.Windows;

namespace ShipPlusA.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        public static List<GameObject> laser = new List<GameObject>();

        public static bool moznaUzyc = true;
        private static float czasOstatniegoUzycia;
        public static float czasDoOdblokowania = 0f;
        private static float countDown = 5f;
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void updateCountdown()
        {
            if (!moznaUzyc)
            {
                float czasOdOstatniegoUzycia = Time.time - czasOstatniegoUzycia;
                czasDoOdblokowania = countDown - czasOdOstatniegoUzycia;

                if (czasOdOstatniegoUzycia >= countDown)
                {
                    moznaUzyc = true;
                }
            }
        }
        public static void coundDown(float cd)
        {
            countDown = cd;
            czasOstatniegoUzycia = Time.time;
            moznaUzyc = false;
        }
        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPostfix]
        static void refr(ref RoundManager __instance)
        {
            if(ShipModBase.checkShow()) if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>open door call");
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>check host");
            //if (HostCheck) return;
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>host ...");
            __instance.StartCoroutine(reCreateLasers(__instance));
        }
        [HarmonyPatch("ShipHasLeft")]
        [HarmonyPostfix]
        static void refclose(ref RoundManager __instance)
        {
            if(ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>close door call");
            //laser.Clear();
        }
        public static System.Collections.IEnumerator reCreateLasers(RoundManager __instance)
        {
            for(int i=0; i < laser.Count; i++)
            {
                GameObject.Destroy(laser[i]);
            }
            laser.Clear();
            yield return new WaitForSeconds(0.2f);
            Transform existingChild = __instance.transform.Find("LightningScript");

            if (ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>check");
            if (existingChild == null)
            {
                if (ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>no child, creating...");
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
                if (ShipPlusAMod.ShipModBase.checkShow()) ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>had child, doing nothing...");

            }
        }
    }
}
