using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using ShipPlusA.Patches;
using ShipPlusA.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using TerminalApi.Classes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ShipPlusAMod.ShipModBase;
using static TerminalApi.Events.Events;
using static TerminalApi.TerminalApi;

namespace ShipPlusAMod
{
    [BepInPlugin(modGUID,modName,modVersion)]
    [BepInDependency("atomic.terminalapi")]
    [BepInDependency("LethalNetworkAPI")]
    public class ShipModBase : BaseUnityPlugin
    {
        private const string modGUID = "Anonymouse.ShipPlusAMod";
        private const string modName = "Skip Plus";
        private const string modVersion = "1.0.0.0";
        public static List<Upgrades> upgrades = new List<Upgrades>();
        public static Vector3 center = new Vector3(0.6f, 7.3623f, -14.177f);
       // Vector3 front = new Vector3(-5.0311f, 5.4649f, -14.1322f);
      //  Vector3 rear = new Vector3(8.894f, 7.2597f, -14.0808f);
        public static bool bought = false;
        private const int spawnAmount = 5;
        private static int zapGunID = 15;
        public static List<GameObject> lasers = new List<GameObject>();
        public static List<ushort> shockingEntities = new List<ushort>();
        private readonly Harmony harmony = new Harmony(modGUID);
        private static ShipModBase Instance;
        public static ShipLights lights;
        public static int upgradeLevel = 0;
        public static bool HostCheck => NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        internal static ManualLogSource Logger;
        static ShipLights shipLightsScript = null;
        static int used=0;
        LethalServerMessage<GameObject> serverShockMessage = new LethalServerMessage<GameObject>("shock");
        LethalClientMessage<GameObject> clientShockMessage = new LethalClientMessage<GameObject>("shock", onReceivedFromClient: RShock);
        LethalClientMessage<int> clientBalanceMessage = new LethalClientMessage<int>("balance", onReceivedFromClient: updateBalance);
        LethalClientMessage<int> clientUpgradeMessage = new LethalClientMessage<int>("upgrade", onReceivedFromClient: updateUpgrade);
        LethalClientMessage<int> clientUpdateUsesMessage = new LethalClientMessage<int>("uses", onReceivedFromClient: updateUses);
        LethalClientMessage<float> clientUpdateCountDown = new LethalClientMessage<float>("cd", onReceivedFromClient: updateCd);
        //config
        private static ConfigEntry<bool> configDebug;
        public static bool checkShow()
        {
            return configDebug.Value;
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            //config
            configDebug = Config.Bind("General", 
                                                "DebugMessages",
                                                false,
                                                "Whether or not to show the debug messages");
            //rest

            //setup upgrades
            // amount = max amount of targets
            // radius = radius of looking for target around ship
            upgrades.Add(new Upgrades { amount = 2, radius = 25f, price = 0, time = 3, uses=2 });
            upgrades.Add(new Upgrades { amount = 3, radius = 35f, price = 750, time = 4, uses = 2 });
            upgrades.Add(new Upgrades { amount = 5, radius = 40f, price = 850, time = 5, uses = 3 });
            upgrades.Add(new Upgrades { amount = 7, radius = 50f, price = 1200, time = 8, uses = 3 });


            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            if(checkShow())Logger.LogInfo("Ship Plus awaken :DD");
            harmony.PatchAll(typeof(ShipModBase));
            harmony.PatchAll(typeof(StartOfRoundPatch));

            AddCommand("zapwave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                    if (terminal.groupCredits < 700) return "Not enoguh credits to buy\n";
                    Logger.LogWarning("Bought ZAP Wave protection");

                    clientBalanceMessage.SendAllClients((terminal.groupCredits - 30));
                    return "Bought ZAP WAVE protection\nTo use type: wave\n";
                },
                Category = "Other",
                Description = "* 30 *\nUse to buy ZAP WAVE tool that protects the ship.\nType 'wave' to use."
            });
            AddCommand("waveup", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    if (bought == false)
                    {
                        return "You have to buy ZAP WAVE first to upgrade it\n";
                    }
                    if (upgradeLevel == upgrades.Count - 1)
                    {
                        return "You already got MAX upgraded ZAP WAVE\n";
                    }
                    Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                    if (terminal.groupCredits < upgrades[upgradeLevel+1].price) return "Not enoguh credits to buy\nPrice *"+ upgrades[upgradeLevel + 1].price+"*\n";
                    if(checkShow())Logger.LogWarning("Bought ZAP Wave upgrade");
                    upgradeLevel += 1;
                    clientBalanceMessage.SendAllClients(terminal.groupCredits-upgrades[upgradeLevel].price);
                    clientUpgradeMessage.SendAllClients(upgradeLevel);
                    return "Bought ZAP WAVE upgrade\nRadius " + upgrades[upgradeLevel].radius + "\nShock time " + upgrades[upgradeLevel].time + "\nTargets " + upgrades[upgradeLevel].amount + "\n";
                },
                Category = "Other",
                //Description = "* " + upgrades[upgradeLevel+1>upgrades.Count-1? upgradeLevel + 1: upgradeLevel].price +" *\nUse to upgrade ZAP WAVE. Upgrade: \nRadius "+ upgrades[upgradeLevel + 1 > upgrades.Count - 1 ? upgradeLevel + 1 : upgradeLevel] + " "++" "
                Description = "Use to upgrade ZAP WAVE. Upgrade: \nRadius ?\nShock time ?\nTargets ?\n"
            });
            AddCommand("wave", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    if (checkShow()) Logger.LogWarning("USED wave cmd");
                     if (!bought)
                     {
                         return "You need to buy it first...\n";
                    }
                    if (used >= upgrades[upgradeLevel].uses)
                    {
                        return "The ship's energy is almost exhausted!\nYou can't use it now\n";
                    }
                    if (!StartOfRoundPatch.moznaUzyc)
                    {
                        return "You need to wait "+ StartOfRoundPatch.czasDoOdblokowania.ToString("F1") + "s before using wave again\n";
                    }
                    PlayerControllerB objectName = StartOfRound.Instance.localPlayerController;

                    if (checkShow()) Logger.LogWarning(">>>>>>>>>>> name "+ objectName);
                    var (targets, enemyAITargets) = getShockableAround();
                    targets = targets.OrderBy(e => Vector3.Distance(e.GetShockableTransform().transform.position, center)).ToArray();
                    enemyAITargets = enemyAITargets.OrderBy(e => Vector3.Distance(e.transform.position, center)).ToArray();
                    if(targets.Length==0 && enemyAITargets.Length == 0)
                    {
                        return "There are no targets around the ship to zap\n";
                    }
                    for (int i = 0; i < targets.Length; i++)
                    {
                        clientShockMessage.SendAllClients(targets[i].GetShockableTransform().gameObject);
                    }
                    for (int i = 0; i < enemyAITargets.Length; i++)
                    {
                        clientShockMessage.SendAllClients(enemyAITargets[i].gameObject);
                    }

                    clientUpdateUsesMessage.SendAllClients(used+1);
                    clientUpdateCountDown.SendAllClients(upgrades[upgradeLevel].time+2);
                    return "Used ZAP WAVE\n\n Try stun (max" + upgrades[upgradeLevel].amount +" targets): \npl: "+ targets.Count()+"\n enemies: "+(enemyAITargets.Count())+"\nUsed waves ["+ used+ "/"+ upgrades[upgradeLevel].uses+ "]\n";
                },
                Category = "Other",
                Description = "Use ZAP WAVE to protect allies before they die!!!"
            });
        }

        static void RShock(GameObject objs, ulong clientId)
        {
            if (HostCheck)
            {

                if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>lights off???? ");
                if (shipLightsScript == null&& FindObjectOfType<ShipLights>()!=null) {
                    shipLightsScript = FindObjectOfType<ShipLights>();
                    shipLightsScript.ToggleShipLights();
                }
                else
                {

                    if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>lights else "+(shipLightsScript==null)+"\nscript??? "+(FindObjectOfType<ShipLights>() == null));
                }
            }
            if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>GOT RPC METHOD CALL, count of objs "+ (objs==null));
            if (objs.GetComponent<PlayerControllerB>() == null)
            {

                if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>GOT RPC METHOD CALL FOR ENEMY??? ");
                EnemyAICollisionDetect e = objs.GetComponentInChildren<EnemyAICollisionDetect>();
                EnemyAICollisionDetect e2 = objs.GetComponent<EnemyAICollisionDetect>();
                if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>check if null  " + (e == null));
                if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>check if null  " + (e2 == null));
                shootClient(e!=null?e:e2, clientId.GetPlayerController());


            }
            else
            {
                if (checkShow()) Logger.LogWarning(">>>>>>>>>>>>rpc for player only ");
                shootClient(objs.GetComponent<PlayerControllerB>(), clientId.GetPlayerController());
            }
        }

        (IShockableWithGun[] shockableTargets, EnemyAI[] enemyAITargets) getShockableAround()
        {
            float radius = upgrades[upgradeLevel].radius;
            List<IShockableWithGun> targets = new List<IShockableWithGun>();
            List<EnemyAI> enemies = new List<EnemyAI>();
            // Użyj SphereCast
            Collider[] hitColliders = Physics.OverlapSphere(ShipModBase.center, radius);
            shockingEntities.Clear();
            // Iteruj przez znalezione collidery
            foreach (var hitCollider in hitColliders)
            {
                Transform hitTransform = hitCollider.transform;
                IShockableWithGun shockableComponent = hitTransform.GetComponent<IShockableWithGun>();
                EnemyAI enemyAiComponent = hitTransform.GetComponent<EnemyAI>();
                if (enemyAiComponent!=null&& enemyAiComponent.stunnedByPlayer==null && !shockingEntities.Contains(enemyAiComponent.NetworkBehaviourId))
                {
                    enemies.Add(enemyAiComponent);
                    shockingEntities.Add(enemyAiComponent.NetworkBehaviourId);
                }
                if (shockableComponent != null && shockableComponent.CanBeShocked())
                {
                    Vector3 shockablePosition = shockableComponent.GetShockablePosition();

                    if (shockableComponent is PlayerControllerB)
                    {
                        PlayerControllerB playerController = (PlayerControllerB)shockableComponent;

                        if (!(playerController.isInHangarShipRoom) && !shockingEntities.Contains(playerController.NetworkBehaviourId))
                        {
                            targets.Add(playerController);
                            shockingEntities.Add(playerController.NetworkBehaviourId);
                        }
                    }
                    else
                    {
                        //targets.Add(shockableComponent);
                    }
                }
            }
            shockingEntities.Clear();
            return (targets.ToArray(), enemies.ToArray());
        }

        static IEnumerator shoot(LightningScript sc,IShockableWithGun shock, PlayerControllerB sender)
        {
            
            float elapsedTime = 0f;
            Vector3 up1 = shock.GetShockablePosition();
            Vector3 pos1 = new Vector3(up1.x, up1.y - 1, up1.z);
            if (checkShow()) Logger.LogWarning(">>>>>>>> script?? "+(sc==null));
            sc.ShootLightning(center, pos1);
            shock.ShockWithGun(sender);
            
            while (elapsedTime < ShipModBase.upgrades[ShipModBase.upgradeLevel].time)
            {
                Vector3 up = shock.GetShockablePosition();
                Vector3 pos = new Vector3(up.x,up.y-1,up.z);
                // Wywołaj funkcję shoot() w każdym ticku
                sc.updateLoc(center, pos);
                // Zaktualizuj czas
                elapsedTime += Time.deltaTime;

                // Poczekaj na kolejny tick
                yield return null;
            }
            shock.StopShockingWithGun();
            yield return new WaitForSeconds(0.001f);
            shock.StopShockingWithGun();
            sc.gameObject.SetActive(false);
        }
        static void shootClient(IShockableWithGun target,PlayerControllerB sender)
        {
            GameObject myScriptObject = StartOfRoundPatch.laser.Find(e=>e.gameObject.activeSelf==false);
            if (myScriptObject != null)
            {

                if (checkShow()) Logger.LogWarning(">>>>>>>> script not null");
                myScriptObject.SetActive(true);
                if (checkShow()) Logger.LogWarning(">>>>>>>> shot");
                LightningScript sc = myScriptObject.GetComponent<LightningScript>();
                if (checkShow()) Logger.LogWarning(">>>>>>>> check " + (sc == null));
                sc.StartCoroutine(shoot(sc, target,sender));

            }
            else
            {

                if (checkShow()) Logger.LogWarning(">>>>>>>> script null, not free laser from count of "+ StartOfRoundPatch.laser.Count);
            }
        }

        public static void updateBalance(int balance, ulong clientId)
        {
            if (checkShow()) Logger.LogInfo("===============sync balance===============");
            if (bought == false) { 
                bought = true;
                RoundManager.Instance.StartCoroutine(StartOfRoundPatch.reCreateLasers(RoundManager.Instance));
            }
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (checkShow()) Logger.LogInfo("===============null???? =============== " + (terminal == null));
            terminal.groupCredits = balance;
        }
        public static void updateUses(int uses, ulong clientId)
        {
            if (checkShow()) Logger.LogInfo("===============sync uses==============="+used);
            used = uses;
        }
        public static void updateCd(float cd, ulong clientId)
        {
            if (checkShow()) Logger.LogInfo("===============sync cd==============="+cd);
            StartOfRoundPatch.coundDown(cd);
        }
        public static void updateUpgrade(int level, ulong clientId)
        {
            if(checkShow())Logger.LogInfo("===============sync level=============== "+level);
            if (bought == false) bought = true;
            if(checkShow())Logger.LogInfo("===============null???? =============== ");
            RoundManager.Instance.StartCoroutine(StartOfRoundPatch.reCreateLasers(RoundManager.Instance));
            upgradeLevel = level;
            used = 0;
        }

        public class BalanceNetwork
        {
            public int balance { get; set; }
        }
        public class ShotNetwork
        {
            public IShockableWithGun target;
        }
        public class Upgrades
        {
            public float radius;
            public int amount;
            public float time;
            public float uses;
            public int price;
        }
    }


}
