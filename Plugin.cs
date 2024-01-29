using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LC_API.GameInterfaceAPI.Features;
using LC_API.Networking;
using LC_API.Networking.Serializers;
using ShipPlusA.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using TerminalApi.Classes;
using Unity.Netcode;
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

        public static Vector3 center = new Vector3(0.6f, 7.3623f, -14.177f);//6.3623f
       // Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
      //  Vector3 rear = new Vector3(8.894f, 7.2597f, -14.0808f);
        public static bool bought = false;
        private const int spawnAmount = 5;
        private static int zapGunID = 15;
        public static List<GameObject> lasers = new List<GameObject>();
        private readonly Harmony harmony = new Harmony(modGUID);
        private static ShipModBase Instance;
        public static List<IShockableWithGun> entities = new List<IShockableWithGun>();
        public static ShipLights lights;
        public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
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
            harmony.PatchAll(typeof(StartOfRoundPatch));
            Network.RegisterAll();
            AddCommand("zapwave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                    if(terminal.groupCredits<30) return "Not enoguh credits to buy\n";
                    Logger.LogWarning("Bought ZAP Wave protection");

                    Network.Broadcast("LCAPI_SYNC_BALANCE", new BalanceNetwork() { balance = terminal.groupCredits - 30 });
                    updateBalance(terminal.groupCredits - 30);
                    Network.Broadcast("LCAPI_SYNC_SPAWN");
                    spawnLasers();
                    return "Bought ZAP WAVE protection\nTo use type: wave\n";
                },
                Category = "Other",
                Description = "* 30 *\nUse to buy ZAP WAVE tool that protects the ship.\nType 'wave' to use."
            });
            AddCommand("wave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    Logger.LogWarning("USED wave cmd");
                    if (!bought)
                    {
                        return "You need to buy it...\n";
                    }
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
        [NetworkMessage("LCAPI_SYNC_SPAWN")]
        public static void Spawnandler(ulong sender)
        {
            spawnLasers();
        }
        public static void spawnLasers()
        {
            bought = true;
            //setup light variable
            Collider[] hitColliders = Physics.OverlapSphere(center, 35f);

            // Iteruj przez znalezione collidery
            foreach (var hitCollider in hitColliders)
            {
                // Uzyskaj transformację hitCollidera
                Transform hitTransform = hitCollider.transform;

                // Sprawdź, czy collider zawiera komponent IShockableWithGun
                ShipLights l = hitTransform.GetComponent<ShipLights>();
                if (l != null)
                {
                    lights = l;
                    return;
                }
            }
            //spawning
            if (HostCheck)
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>IS HOST.... continue");
                Player.LocalPlayer.PlayerController.StartCoroutine(spawnRPC());
                return;
            }
            else
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>IS CLIENT");

                Player.LocalPlayer.PlayerController.StartCoroutine(spawnDelay());
            }


            
        }//yield return new WaitUntil(() => shipHasLanded);
        static IEnumerator spawnRPC()
        {
            yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded == true);
            lasers.Clear();
            //mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            //Vector3 LocationInfo = ((Component)__instance.playersManager.localPlayerController).transform.position;
            Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
            Vector3 rear = new Vector3(8.894f, 7.2597f, -14.0808f);
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>try spawn for server...");
            for (int i = 0; i < spawnAmount; i++)
            {

                ShipPlusAMod.ShipModBase.Logger.LogInfo("=================spawning laser id " + i);
                GameObject val = UnityEngine.Object.Instantiate<GameObject>(StartOfRound.Instance.allItemsList.itemsList[zapGunID].spawnPrefab, center, Quaternion.identity);
                val.GetComponent<GrabbableObject>().fallTime = 0f;
                val.AddComponent<ScanNodeProperties>().scrapValue = 0;
                val.GetComponent<GrabbableObject>().SetScrapValue(0);
                val.GetComponent<PatcherTool>().playerHeldBy = Player.HostPlayer.PlayerController;


                val.GetComponent<NetworkObject>().Spawn(false);
                lasers.Add(val);

            }
        }
        static IEnumerator spawnDelay()
        {
            yield return new WaitUntil(() => StartOfRound.Instance.shipHasLanded == true);
            ShipPlusAMod.ShipModBase.Logger.LogInfo("================try spawn for client================");
            yield return new WaitForSeconds(4f);
            lasers.Clear();
            //Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
            Collider[] hitColliders = Physics.OverlapSphere(center, 4f);

            // Iteruj przez znalezione collidery
            foreach (var hitCollider in hitColliders)
            {
                // Uzyskaj transformację hitCollidera
                Transform hitTransform = hitCollider.transform;

                // Sprawdź, czy collider zawiera komponent IShockableWithGun
                PatcherTool gun = hitTransform.GetComponent<PatcherTool>();
                ShipLights l= hitTransform.GetComponent<ShipLights>();
                // Jeśli znaleziono IShockableWithGun i można go zelektryzować
                if (gun != null)
                {
                    gun.GetComponent<PatcherTool>().playerHeldBy = Player.HostPlayer.PlayerController;
                    lasers.Add(hitTransform.gameObject);
                }
                if(l!= null)
                {
                    lights = l;
                }
            }
            ShipPlusAMod.ShipModBase.Logger.LogInfo("================end client================ "+ lasers.Count);
        }
        [NetworkMessage("LCAPI_SYNC_BALANCE")]
        public static void ExampleHandler(ulong sender, BalanceNetwork b)
        {
            ShipPlusAMod.ShipModBase.Logger.LogInfo("===============sync balance rpc===============");
            updateBalance(b.balance);
        }
        public static void updateBalance(int balance)
        {
            ShipPlusAMod.ShipModBase.Logger.LogInfo("===============sync balance===============");
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            terminal.groupCredits = balance;
        }
        public class BalanceNetwork
        {
            public int balance { get; set; }
        }
    }


}
