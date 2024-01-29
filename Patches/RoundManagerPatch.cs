using HarmonyLib;
using LC_API.GameInterfaceAPI.Features;
using ShipPlusAMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using Unity.Netcode;
using UnityEngine;

namespace ShipPlusA.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        /*static int zapGunID = 15;
        static int spawnAmount = 5;
        public static List<GameObject> lasers = new List<GameObject>();
        //internal static GameObject mapPropsContainer;
        public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;

        [HarmonyPatch("SpawnMapObjects")]
        [HarmonyPostfix]
        static void spawnLasers(ref RoundManager __instance)
        {
            if (!HostCheck) {
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>NOT HOST, RETURN");
               
            }

            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>IS HOST.... continue");
            lasers.Clear();
            //mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            //Vector3 LocationInfo = ((Component)__instance.playersManager.localPlayerController).transform.position;
            Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
            Vector3 rear = new Vector3(8.894f, 7.2597f, -14.0808f);
            Vector3 center = new Vector3(0f, 6.3623f, -12.0808f);
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>try spawn...");
            for (int i=0;i<spawnAmount;i++)
            {

                ShipPlusAMod.ShipModBase.Logger.LogInfo("=================spawning laser id "+i);
                GameObject val = UnityEngine.Object.Instantiate<GameObject>(StartOfRound.Instance.allItemsList.itemsList[zapGunID].spawnPrefab, center, Quaternion.identity);
                val.GetComponent<GrabbableObject>().fallTime = 0f;
                val.AddComponent<ScanNodeProperties>().scrapValue = 0;
                val.GetComponent<GrabbableObject>().SetScrapValue(0);
                val.GetComponent<PatcherTool>().playerHeldBy = Player.HostPlayer.PlayerController;
                    
                
                val.GetComponent<NetworkObject>().Spawn(false);
                lasers.Add(val);

                ShipPlusAMod.ShipModBase.Logger.LogInfo("================spawned item===================");
            }
        }*/
    }
}
