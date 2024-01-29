using GameNetcodeStuff;
using HarmonyLib;
using LC_API.GameInterfaceAPI.Features;
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
     /*   public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;


        [HarmonyPatch("openingDoorsSequence")]
        [HarmonyPostfix]
        static void refr(ref RoundManager __instance)
        {
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>open door call");
            if (HostCheck) return;
            Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
            Vector3 center = new Vector3(0f, 6.3623f, -12.0808f);
            Collider[] hitColliders = Physics.OverlapSphere(center, 3f);

            // Iteruj przez znalezione collidery
            foreach (var hitCollider in hitColliders)
            {
                // Uzyskaj transformację hitCollidera
                Transform hitTransform = hitCollider.transform;

                // Sprawdź, czy collider zawiera komponent IShockableWithGun
                PatcherTool gun = hitTransform.GetComponent<PatcherTool>();

                // Jeśli znaleziono IShockableWithGun i można go zelektryzować
                if (gun != null)
                {
                    gun.GetComponent<PatcherTool>().playerHeldBy = Player.HostPlayer.PlayerController;
                    RoundManagerPatch.lasers.Add(hitTransform.gameObject);
                }
            }
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>lasers: " + RoundManagerPatch.lasers.Count + " / " + hitColliders.Length);
            return;
        }*/
    }
}
