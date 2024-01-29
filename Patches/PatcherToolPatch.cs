using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using HarmonyLib;
using LC_API.GameInterfaceAPI.Features;
using ShipPlusAMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TerminalApi;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

namespace ShipPlusA.Patches
{
    [HarmonyPatch(typeof(PatcherTool))]
    internal class PatcherToolPatch
    {

        /*      [HarmonyPatch("ItemActivate")]
                [HarmonyTranspiler]
                static IEnumerable<CodeInstruction> ItemActivationTranspiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);

                    for (int i = 0; i < instructionList.Count; i++)
                    {
                        // Szukaj instrukcji, która sprawdza warunek "playerHeldBy == null"
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 &&
                            instructionList[i + 1].opcode == OpCodes.Ldfld &&
                            instructionList[i + 2].opcode == OpCodes.Brfalse_S)
                        {
                            // Zastąp instrukcje sprawdzające warunek
                            instructionList[i].opcode = OpCodes.Ldc_I4_0; // Wstaw false na stos
                            instructionList[i + 1].opcode = OpCodes.Nop; // Usuń kod porównujący (Ldfld)
                            instructionList[i + 2].opcode = OpCodes.Nop; // Usuń skok warunkowy (Brfalse_S)
                        }
                    }

                    return instructionList.AsEnumerable();
                }*/
        /*        static IEnumerable<CodeInstruction> ItemActivationTranspiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);

                    for (int i = 0; i < instructionList.Count; i++)
                    {
                        // Znajdź kod, który ładowuje wartość boolowską na stos przed warunkiem if
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i + 1].opcode == OpCodes.Ceq)
                        {
                            // Zastąp ładowanie wartości boolowskiej na stosie wartością true
                            instructionList[i].opcode = OpCodes.Ldc_I4_1; // Ldc_I4_1 oznacza true
                            instructionList[i + 1].opcode = OpCodes.Nop; // Usuń kod porównujący (Ceq)
                        }
                    }

                    return instructionList.AsEnumerable();
                }*/

        /*        static IEnumerable<CodeInstruction> ItemActivationTranspiler(IEnumerable<CodeInstruction> instructions)
                {
                    List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);

                    for (int i = 0; i < instructionList.Count; i++)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 &&
                            instructionList[i + 1].opcode == OpCodes.Ldfld &&
                            instructionList[i + 2].opcode == OpCodes.Brfalse_S)
                        {
                            instructionList[i].opcode = OpCodes.Ldc_I4_1; 
                            instructionList[i + 1].opcode = OpCodes.Nop; 
                            instructionList[i + 2].opcode = OpCodes.Nop; 
                        }
                    }

                    return instructionList.AsEnumerable();
                }*/
        /*        [HarmonyPatch("ItemActivate")]
                [HarmonyPrefix]
                public static bool activateItem(PatcherTool __instance)
                {
                    ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>activateItem called");
                    ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>is null??? " + __instance.playerHeldBy == null);
                    ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>is isScanning??? " + __instance.isScanning);
                    Vector3 pos = new Vector3(-5.0311f, 5.4649f, -14.1322f);
                    if (Vector3.Distance(__instance.transform.position, pos) > 2)
                    {
                        return true;
                    }
                    __instance.gunOverheat = 0f;

                    if (__instance.scanGunCoroutine != null)
                    {
                        __instance.StopCoroutine(__instance.scanGunCoroutine);
                        __instance.scanGunCoroutine = null;
                    }
                    if (__instance.beginShockCoroutine != null)
                    {
                        __instance.StopCoroutine(__instance.beginShockCoroutine);
                        __instance.beginShockCoroutine = null;
                    }
                    if (__instance.isShocking)
                    {
                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>Stop shocking gun");
                        __instance.StopShockingAnomalyOnClient(failed: true);
                    }
                    else if (__instance.isScanning)
                    {
                        __instance.SwitchFlashlight(on: false);
                        __instance.gunAudio.Stop();
                        __instance.currentUseCooldown = 0.5f;
                        if (__instance.scanGunCoroutine != null)
                        {
                            __instance.StopCoroutine(__instance.scanGunCoroutine);
                            __instance.scanGunCoroutine = null;
                        }
                        __instance.isScanning = false;
                    }
                    else
                    {
                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>Start scanning gun1111");
                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>is instance null??? " + __instance == null);
                        __instance.isScanning = true;
                        __instance.sentStopShockingRPC = false;
                        __instance.scanGunCoroutine = __instance.StartCoroutine(__instance.ScanGun());
                        __instance.currentUseCooldown = 0.5f;
                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>Use patcher tool");

                        __instance.PlayRandomAudio(__instance.mainAudio, __instance.activateClips);
                        __instance.SwitchFlashlight(on: true);
                    }
                    return false;
                }*/
        [HarmonyPatch("ShockPatcherToolClientRpc")]
        [HarmonyPrefix]
        public static void clrpc(NetworkObjectReference netObject,PatcherTool __instance)
        {

            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>ShockPatcherToolClientRpc pre123");
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>> held1 " + __instance.playerHeldBy == null);
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>> held2 " + netObject == null);
            if (netObject.TryGet(out var networkObject))
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< shock3 " + networkObject.gameObject.GetComponentInChildren<IShockableWithGun>() == null);
            }
        }
        [HarmonyPatch("LateUpdate")]
        [HarmonyPrefix]
        public static void lupdate(PatcherTool __instance)
        {
            if (__instance.isShocking == true)
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<<LATE UPDATE pre");
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.bendRandomizerShift);
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.gunRandom == null);
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.bendChangeSpeedMultiplier);
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.maxChangePerFrame);
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.maxChangePerFrame);
                ShipPlusAMod.ShipModBase.Logger.LogInfo("<<<<<<<<<<<<<<<< " + __instance.maxChangePerFrame);
            }
        }

        [HarmonyPatch("ScanGun")]
        [HarmonyPrefix]
        public static bool zapgunPatch(PlayerControllerB ___playerHeldBy, int ___anomalyMask, RaycastHit ___hit, PatcherTool __instance)
        {
            
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>CALLED ZIP");
            Vector3 pos = new Vector3(-5.0311f, 5.4649f, -14.1322f);
            if (Vector3.Distance(__instance.transform.position, pos) >2)
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>NOT THAT POSITION");
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>1 "+ __instance.transform.position.x+" "+__instance.transform.position.y+" "+ __instance.transform.position.z);
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>2 " + pos.x + " " + pos.y + " " + pos.z);
                return true;
            }
            RaycastHit[] raycastEnemies = new RaycastHit[12];
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>Scan OWN AAAAA");
            Player inTerminal = Player.ActiveList.FirstOrDefault(e => e.PlayerController.inTerminalMenu == true);
            if (inTerminal == null)
            {

                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>NIKT W TERMINALU?????????????????????????????????");
                return false;
            }
            __instance.playerHeldBy = inTerminal.PlayerController;

            for (int i = 0; i < 12; i++)
            {
                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>Scan Bbbbbbbbbbbbbbbb");
                // Skopiuj parametry, jeśli chcesz nad nimi pracować

                // Ustaw promień SphereCast
                float radius = 25f;

                // Użyj SphereCast
                Collider[] hitColliders = Physics.OverlapSphere(pos, radius);

                // Iteruj przez znalezione collidery
                foreach (var hitCollider in hitColliders)
                {
                    // Uzyskaj transformację hitCollidera
                    Transform hitTransform = hitCollider.transform;

                    // Sprawdź, czy collider zawiera komponent IShockableWithGun
                    IShockableWithGun shockableComponent = hitTransform.GetComponent<IShockableWithGun>();

                    // Jeśli znaleziono IShockableWithGun i można go zelektryzować
                    if (shockableComponent != null && shockableComponent.CanBeShocked() && !ShipModBase.entities.Contains(shockableComponent))
                    {
                        Vector3 shockablePosition = shockableComponent.GetShockablePosition();
                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>>>>Got shockable transform name : " + shockableComponent.GetShockableTransform().gameObject.name);

                        // Sprawdź, czy to jest instancja PlayerControllerB
                        if (shockableComponent is PlayerControllerB)
                        {
                            PlayerControllerB playerController = (PlayerControllerB)shockableComponent;
                            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>znalazlem gracza " + playerController.playerUsername);

                            if ((playerController.isInHangarShipRoom))
                            {
                                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>jest w statku");
                            }
                            else
                            {
                                ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>NIE jest w statku");

                                ShipModBase.entities.Add(shockableComponent);
                                // Tutaj możesz wykonać operacje dla obiektów wokół HangarShipDoor
                                // ...
                                // Przykład: Uruchom Coroutine fire, jeśli znajdziesz PatcherTool
                                PatcherTool tool = __instance;
                                if (tool != null)
                                {

                                    tool.timesUsed++;
                                    tool.sentStopShockingRPC = false;
                                    tool.gunRandom = new System.Random(tool.playerHeldBy.playersManager.randomMapSeed + tool.timesUsed);
                                    tool.gunOverheat = 0f;
                                    tool.shockedTargetScript = shockableComponent;
                                    tool.currentDifficultyMultiplier = shockableComponent.GetDifficultyMultiplier();
                                    tool.InitialDifficultyValues();
                                    tool.bendMultiplier = 0f;
                                    tool.bendRandomizerShift = 0f;

                                    tool.insertedBattery.charge = 1;
                                    tool.insertedBattery.empty = false;
                                    tool.StartCoroutine(fire(tool, shockableComponent, 3f));
                                }
                            }
                        }

                        ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>leci shock");
                    }
                }
                //yield return new WaitForSeconds(0.125f);
            }
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>Zap gun light off!!!");
            //__instance.playerHeldBy = null;
            return false;
        }
        static IEnumerator fire(PatcherTool tool, IShockableWithGun shockableComponent, float time)
        {
            // Coroutine beginShockCoroutine = tool.StartCoroutine(tool.beginShockGame(shockableComponent));


            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>fireeee");

            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>step 0");
            tool.effectAnimator.SetTrigger("Shock");
            tool.gunAudio.PlayOneShot(tool.detectAnomaly);
            tool.isShocking = true;
            tool.isScanning = false;
            tool.playerHeldBy.inShockingMinigame = true;
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>step 1");
            Transform shockableTransform = shockableComponent.GetShockableTransform();
            tool.playerHeldBy.shockingTarget = shockableTransform;
            //tool.playerHeldBy.isCrouching = false;
            //tool.playerHeldBy.playerBodyAnimator.SetBool("crouching", value: false);
            //tool.playerHeldBy.turnCompass.LookAt(shockableTransform);
            Vector3 zero = Vector3.zero;

            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>step 2");
            //zero.y = playerHeldBy.turnCompass.localEulerAngles.y;
            //playerHeldBy.turnCompass.localEulerAngles = zero;
            yield return new WaitForSeconds(0.55f);
            tool.StartShockAudios();
            tool.isBeingUsed = true;
            shockableComponent.ShockWithGun(tool.playerHeldBy);
            //tool.playerHeldBy.inSpecialInteractAnimation = true;
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>step 3");
            //playerHeldBy.playerBodyAnimator.SetBool("HoldPatcherTool", value: true);
            tool.SwitchFlashlight(on: false);
            tool.gunAnimator.SetTrigger("Shock");
            tool.lightningObject.SetActive(value: true);
            tool.lightningVisible = true;
            tool.ShockPatcherToolServerRpc(shockableComponent.GetNetworkObject());

            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>step 4");

            yield return new WaitForSeconds(time);
            tool.StopShockingAnomalyOnClient();
            tool.isShocking = false;
            tool.playerHeldBy.inShockingMinigame = false;
            ShipPlusAMod.ShipModBase.Logger.LogInfo(">>>>>>>>>>>>>>>>>>>stop");
            // tool.StopCoroutine(beginShockCoroutine);
            // Code to execute after the delay
            ShipModBase.entities.Clear();
        }
    }
    
}
