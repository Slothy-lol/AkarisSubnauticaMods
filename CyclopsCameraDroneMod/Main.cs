using HarmonyLib;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Options;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = QModManager.Utility.Logger;
using UnityEngine;
using UWE;
using System.Collections;
using CyclopsCameraDroneMod.QMods;
using MoreCyclopsUpgrades.API;

namespace CyclopsCameraDroneMod.Main
{
    [HarmonyPatch]
    public class Main
    {
        //TODO
        //make visual laser effects finally
        //add scanning functionality
        //add way to defend camera????? Maybe?
        //add way to pick up items
        //add new cool shit, what all exactly this entails is for you to decide and then DM me with.
        //add speed upgrades for all drones somehow, they too fuckin slow
        //more shit I guess, idk

        public static string CameraName = "CyclopsDroneCamera";
        public static float nextUse;
        public static float cooldownTime = 1f;
        public static GameObject CameraDroneLaser;

        [HarmonyPatch]
        public class Postfixes
        {
            [HarmonyPatch(typeof(CyclopsExternalCams), nameof(CyclopsExternalCams.HandleInput))]
            [HarmonyPostfix]
            public static void HandleInputPatch(CyclopsExternalCams __instance, ref bool __result)
            {
                KeyCode droneButton = QMod.Config.droneKey;
                if (!(MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModule.thisTechType) || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrill.thisTechType) || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType)))
                {
                    return;
                }
                if (Input.GetKeyUp(droneButton))
                {
                    __instance.ExitCamera();

                    CoroutineHost.StartCoroutine(CreateAndControl(__instance));

                    __result = true;
                }
            }
            private static IEnumerator CreateAndControl(CyclopsExternalCams __instance)
            {
                var coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.MapRoomCamera, false);
                yield return coroutineTask;
                var prefab = coroutineTask.GetResult();

                coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.Battery, false);
                yield return coroutineTask;
                var batteryPrefab = coroutineTask.GetResult();

                Vector3 position = GetSpawnPosition(__instance.transform.parent.gameObject);
                MapRoomCamera cyclopsCameraDrone = GameObject.Instantiate(prefab, position, Player.main.transform.rotation).GetComponent<MapRoomCamera>();
                cyclopsCameraDrone.gameObject.name = CameraName;

                cyclopsCameraDrone.energyMixin.battery = GameObject.Instantiate(batteryPrefab).GetComponent<Battery>();

                yield return new WaitUntil(() => cyclopsCameraDrone.inputStackDummy != null);
                if (Player.main.currChair != null) { Player.main.ExitPilotingMode(); }
                cyclopsCameraDrone.ControlCamera(Player.main, null);
                Player.main.ExitLockedMode(false, false);
                Player.main.EnterLockedMode(null);
            }
            static Vector3 GetSpawnPosition(GameObject cyclopsObject)
            {
                Matrix4x4 cyclopsMatrix = cyclopsObject.transform.localToWorldMatrix;
                Matrix4x4 playerMatrix = Player.main.transform.localToWorldMatrix;

                Vector4 row1 = new Vector4(cyclopsMatrix[0, 0], cyclopsMatrix[0, 1], cyclopsMatrix[0, 2], playerMatrix[0, 3]);
                Vector4 row2 = new Vector4(cyclopsMatrix[1, 0], cyclopsMatrix[1, 1], cyclopsMatrix[1, 2], playerMatrix[1, 3]);
                Vector4 row3 = new Vector4(cyclopsMatrix[2, 0], cyclopsMatrix[2, 1], cyclopsMatrix[2, 2], playerMatrix[2, 3]);
                Vector4 row4 = new Vector4(cyclopsMatrix[3, 0], cyclopsMatrix[3, 1], cyclopsMatrix[3, 2], playerMatrix[3, 3]);

                Matrix4x4 combinedMatrix = Matrix4x4.identity;
                combinedMatrix.SetRow(0, row1);
                combinedMatrix.SetRow(1, row2);
                combinedMatrix.SetRow(2, row3);
                combinedMatrix.SetRow(3, row4);

                return combinedMatrix.MultiplyPoint3x4(new Vector3(0, -12, 0));
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.FreeCamera))]
            [HarmonyPrefix]
            public static bool ExitCameraPatch(MapRoomCamera __instance, bool resetPlayerPosition)
            {
                if (__instance.screen != null)
                {
                    return true;
                }
                InputHandlerStack.main.Pop(__instance.inputStackDummy);
                Player player = __instance.controllingPlayer;
                player.ExitLockedMode(false, false);
                __instance.controllingPlayer = null;
                if (resetPlayerPosition)
                {
                    SNCameraRoot.main.transform.localPosition = Vector3.zero;
                    SNCameraRoot.main.transform.localRotation = Quaternion.identity;
                }
                __instance.rigidBody.velocity = Vector3.zero;
                MainCameraControl.main.enabled = true;
                __instance.screen = null;
                __instance.RenderToTexture();
                uGUI_CameraDrone.main.SetCamera(null);
                uGUI_CameraDrone.main.SetScreen(null);
                __instance.engineSound.Stop();
                __instance.screenEffectModel.SetActive(false);
                __instance.droneIdle.Stop();
                __instance.connectingSound.Stop();
                Player.main.SetHeadVisible(false);
                __instance.lightsParent.SetActive(false);

                if (__instance.name == CameraName)
                {
                    GameObject.Destroy(__instance.gameObject);
                }
                return false;
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.GetScreenDistance))]
            [HarmonyPostfix]
            public static void FixDistance(MapRoomCamera __instance, ref float __result)
            {
                if (__instance.name == CameraName && !QMod.Config.InfiniteDistance)
                {
                    __result = (Player.main.transform.position - __instance.transform.position).magnitude;
                }
            }
            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.StabilizeRoll))]
            [HarmonyPostfix]
            public static void FixPlayerMovement(MapRoomCamera __instance)
            {
                if (__instance.name == CameraName)
                {
                    Player.main.ExitLockedMode(false, false);
                    Player.main.EnterLockedMode(null);
                }
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.Update))]
            [HarmonyPostfix]
            public static void GetLookingTarget(MapRoomCamera __instance) //no longer just get's target, now also works keybinds
            {
                if (Input.GetKeyUp(QMod.Config.beaconKey) && __instance.name == CameraName)
                {
                    SubRoot currentSub = Player.main.currentSub;
                    if (currentSub == null) { return; }
                    if (GameModeUtils.IsOptionActive(GameModeOption.NoCost))
                    {
                        CoroutineHost.StartCoroutine(CreateBeacon(__instance.transform));
                        return;
                    }
                    bool hasBeacon = false;
                    ItemsContainer containerWithBeacon = null;

                    foreach (CyclopsLocker locker in currentSub.gameObject.GetComponentsInChildren<CyclopsLocker>())
                    {
                        ItemsContainer container = locker.gameObject.GetComponentInChildren<StorageContainer>().container;

                        if (container != null && container.Contains(TechType.Beacon))
                        {
                            containerWithBeacon = container;
                            hasBeacon = true;
                            break;
                        }
                    }

                    if (!hasBeacon)
                    {
                        if (Inventory.Get().container.Contains(TechType.Beacon))
                        {
                            containerWithBeacon = Inventory.Get().container;
                            hasBeacon = true;
                        }
                    }
                    if (hasBeacon && containerWithBeacon != null)
                    {
                        containerWithBeacon.DestroyItem(TechType.Beacon);
                        CoroutineHost.StartCoroutine(CreateBeacon(__instance.transform));
                    }
                }
                if (!(MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrill.thisTechType) || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType)))
                {
                    return;
                }
                if (GameInput.GetButtonHeld(GameInput.Button.LeftHand) && __instance.name == CameraName)
                {
                    CameraDroneLaser.SetActive(true);
                    SetBeamTarget(__instance);
                    Targeting.GetTarget(__instance.gameObject, MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, out var gameObject4, out _);
                    if (gameObject4 != null)
                    {
                        Drillable drillable = gameObject4.GetComponentInParent<Drillable>();
                        if (drillable != null && (Time.time > nextUse || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType)))
                        {
                            Main.CameraDroneLaser.SetActive(true);
                            __instance.energyMixin.ConsumeEnergy(5);
                            nextUse = Time.time + cooldownTime;
                            if (!MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType))
                            {
                                for (var i = 0; i < 26; i++)
                                {
                                    if (drillable.health.Sum() > 0)
                                    {
                                        drillable.OnDrill(gameObject4.transform.position, null, out var _);
                                    }
                                }
                            }
                            else
                            {
                                while (drillable.health.Sum() > 0)
                                {
                                    drillable.OnDrill(gameObject4.transform.position, null, out var _);
                                }
                            }
                        }
                    }
                }else{ Main.CameraDroneLaser.SetActive(false); }
            }
            public static IEnumerator CreateBeacon(Transform transform)
            {
                var coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.Beacon, false);
                yield return coroutineTask;
                var prefab = coroutineTask.GetResult();

                GameObject.Instantiate(prefab, transform.position + 5f * transform.forward, transform.rotation);
            }
        }

        [HarmonyPatch(typeof(Drillable), nameof(Drillable.ManagedUpdate))]
        [HarmonyPostfix]
        public static void ItemsToCyclops(Drillable __instance)
        {
            SubRoot currentSub = Player.main.currentSub;
            if (__instance.lootPinataObjects.Count > 0 && currentSub != null)
            {
                List<GameObject> list = new List<GameObject>();
                foreach (GameObject gameObject in __instance.lootPinataObjects)
                {
                    if (gameObject == null)
                    {
                        list.Add(gameObject);
                    }
                    else
                    {
                        Vector3 b = currentSub.transform.position;
                        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, b, Time.deltaTime * 5f);
                        if (Vector3.Distance(gameObject.transform.position, b) < 3f)
                        {
                            Pickupable pickupable = gameObject.GetComponentInChildren<Pickupable>();
                            if (pickupable)
                            {
                                ItemsContainer cyclopsContainer = currentSub.gameObject.GetComponentInChildren<CyclopsLocker>().gameObject.GetComponentInChildren<StorageContainer>().container;
                                if (!cyclopsContainer.HasRoomFor(pickupable))
                                {
                                    if (currentSub != null)
                                    {
                                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                                    }
                                }
                                else
                                {
                                    string arg = Language.main.Get(pickupable.GetTechName());
                                    ErrorMessage.AddMessage(Language.main.GetFormat<string>("VehicleAddedToStorage", arg));
                                    uGUI_IconNotifier.main.Play(pickupable.GetTechType(), uGUI_IconNotifier.AnimationType.From, null);
                                    pickupable = pickupable.Initialize();
                                    InventoryItem item = new InventoryItem(pickupable);
                                    cyclopsContainer.UnsafeAdd(item);
                                    pickupable.PlayPickupSound();
                                }
                                list.Add(gameObject);
                            }
                        }
                    }
                }
                if (list.Count > 0)
                {
                    foreach (GameObject item2 in list)
                    {
                        __instance.lootPinataObjects.Remove(item2);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.Start))]
        [HarmonyPostfix]
        public static void CreateLaser(MapRoomCamera __instance)
        {

            GameObject cannon_pylon_left = CraftData.InstantiateFromPrefab(TechType.PowerTransmitter);
            cannon_pylon_left.transform.SetParent(__instance.transform, false);
            Utils.ZeroTransform(cannon_pylon_left.transform);

            GameObject laserBeam = GameObject.Instantiate(cannon_pylon_left.GetComponent<PowerFX>().vfxPrefab, __instance.transform.position - new Vector3(0, 7, 0), __instance.transform.rotation);
            laserBeam.SetActive(false);

            LineRenderer lineRenderer = laserBeam.GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.9f;
            lineRenderer.positionCount = 2;
            Color defaultColour1 = new Color(77, 166, 255);
            Color defaultColour2 = new Color(0, 255, 42);
            Color beamColour;
            if (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType))
            {
                if (!(QMod.Config.drill2RGB1 < 0 || QMod.Config.drill2RGB1 > 255 || QMod.Config.drill2RGB2 < 0 || QMod.Config.drill2RGB2 > 255 || QMod.Config.drill2RGB3 < 0 || QMod.Config.drill2RGB3 > 255))
                {
                    beamColour = new Color(QMod.Config.drill2RGB1, QMod.Config.drill2RGB2, QMod.Config.drill2RGB3);
                }
                else
                {
                    beamColour = defaultColour2;
                }
                if (QMod.Config.drill2RGB1 == 0 && QMod.Config.drill2RGB2 == 0 && QMod.Config.drill2RGB3 == 0) { beamColour = new Color(255, 38, 147); }
            }
            else
            {
                if (!(QMod.Config.drill1RGB1 < 0 || QMod.Config.drill1RGB1 > 255 || QMod.Config.drill1RGB2 < 0 || QMod.Config.drill1RGB2 > 255 || QMod.Config.drill1RGB3 < 0 || QMod.Config.drill1RGB3 > 255))
                {
                    beamColour = new Color(QMod.Config.drill1RGB1, QMod.Config.drill1RGB2, QMod.Config.drill1RGB3);
                }
                else
                {
                    beamColour = defaultColour1;
                }
                if (QMod.Config.drill1RGB1 == 0 && QMod.Config.drill1RGB2 == 0 && QMod.Config.drill1RGB3 == 0) { beamColour = new Color(255, 38, 147); }
            }
            lineRenderer.material.color = beamColour;
            CameraDroneLaser = UnityEngine.Object.Instantiate(laserBeam, position: __instance.transform.position - new Vector3(0,5,0), rotation: __instance.transform.rotation);
            GameObject.DestroyImmediate(laserBeam);
            GameObject.DestroyImmediate(cannon_pylon_left);
        }
        public static void SetBeamTarget(MapRoomCamera __instance)
        {
            if (Targeting.GetTarget(__instance.gameObject, MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, out GameObject targetGameobject, out float targetDist))
            {
                CalculateBeamVectors(targetDist, __instance);
            }
            else
                CalculateBeamVectors(MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, __instance);
        }

        public static void CalculateBeamVectors(float targetDistance, MapRoomCamera __instance)
        {
            Transform aimTransform = __instance.transform;

            Vector3 targetPosition = aimTransform.position + targetDistance * aimTransform.forward;

            CameraDroneLaser.GetComponent<LineRenderer>().SetPosition(0, aimTransform.position - new Vector3(0,5,0));
            CameraDroneLaser.GetComponent<LineRenderer>().SetPosition(1, targetPosition);          
        }
    }
}
