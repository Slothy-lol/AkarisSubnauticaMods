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
        //add way to defend camera????? Maybe? Done, made laser damage enemy.
            //^May also make stalkers release camera on taking damage, you wouldn't want *your* food to burn your mouth off with a laser
            //maybe you would, idk, I won't judge
        //add way to pick up items
        //add new cool shit, what all exactly this entails is for you to decide and then DM me with.
        //add speed upgrades for all drones somehow, they too fuckin slow
        //more shit I guess, idk
        //command for all modules
        //TELEPORTATION! HELL YEA!
        //No seriously, solves problem of being too slow and is more cool shit. Just do it

        //finish lines 249-275

        public static string CameraName = "CyclopsDroneCamera";
        public static float nextUse;
        public static float cooldownTime = 1f;
        public static LineRenderer CameraDroneLaser;
        public static LineRenderer lineRenderer;

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

                Battery battery = GameObject.Instantiate(batteryPrefab).GetComponent<Battery>();
                cyclopsCameraDrone.energyMixin.battery = battery;
                battery.gameObject.transform.parent = cyclopsCameraDrone.gameObject.transform;

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
                if (GameInput.GetButtonHeld(GameInput.Button.LeftHand) || Input.GetKey(QMod.Config.miningKey) && __instance.name == CameraName)
                {
                    workColors();
                    CameraDroneLaser.enabled = true;
                    SetBeamTarget(__instance);
                    Targeting.GetTarget(__instance.gameObject, MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, out var gameObject4, out _);
                    if (gameObject4 != null)
                    {
                        Drillable drillable = gameObject4.GetComponentInParent<Drillable>();
                        if (drillable != null && (Time.time > nextUse || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType)))
                        {
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
                        LiveMixin liveMixin = gameObject4.GetComponent<LiveMixin>() != null ? gameObject4.GetComponent<LiveMixin>() : gameObject4.GetComponentInParent<LiveMixin>();
                        if (liveMixin != null && Time.time > nextUse) 
                        {
                            __instance.energyMixin.ConsumeEnergy(5);
                            nextUse = Time.time + cooldownTime;
                            liveMixin.TakeDamage(30);
                        }
                        //to make laser also function as tractor beam thing for items
                        //copy and paste the code from between the comments below to right here

                        //in that blank line^
                        //also delete the interact button keybind in QMod.cs
                    }
                }else { CameraDroneLaser.enabled = false; }
                if(Input.GetKeyUp(QMod.Config.interactKey)) //currently works based on hitting a key while looking at item, look at comment above to change
                //if copying, delete this ^ entire if statement as it is no longer going to be used
                {
                    Targeting.GetTarget(__instance.gameObject, 5, out var gameObject4, out _);
                    //copy from here
                    Pickupable pickupable = gameObject4.GetComponent<Pickupable>() != null ? gameObject4.GetComponent<Pickupable>() : gameObject4.GetComponentInParent<Pickupable>();
                    if (pickupable != null)
                    {
                        if(true/*add a config for whether items go to player inventory or cyclops locker when picked up*/)
                        {
                            pickupable.OnHandClick(Player.main.armsController.guiHand);//acts as if the player picked up the item
                        }
                        else//may want to also resort to the other inventory if selected inventory is full. EX; player inventory selected in config but player inventory full, so items instead go to cyclops locker as backup.
                        {
                            //copy inventory.pickup in dnspy
                            //change all references to `this.container` to the cyclops lockers
                                //check drill patch below to find how to get cyclops lockers
                            //remove unnecessary if statement at line 13 
                            //remove return statement

                            //steps above should be all, may need a bit more though
                        }
                    }
                    //to here
                }
            }
            public static IEnumerator CreateBeacon(Transform transform)
            {
                var coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.Beacon, false);
                yield return coroutineTask;
                var prefab = coroutineTask.GetResult();

                GameObject.Instantiate(prefab, transform.position - 0.5f * transform.forward, transform.rotation);
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
            if (CameraDroneLaser != null) return;
            GameObject cannon_pylon_left = CraftData.InstantiateFromPrefab(TechType.PowerTransmitter);
            cannon_pylon_left.transform.SetParent(__instance.transform, false);
            Utils.ZeroTransform(cannon_pylon_left.transform);

            GameObject laserBeam = GameObject.Instantiate(cannon_pylon_left.GetComponent<PowerFX>().vfxPrefab, __instance.transform.position - new Vector3(0, 2, 0), __instance.transform.rotation);
            laserBeam.SetActive(true);

            lineRenderer = laserBeam.GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 2;
            
            CameraDroneLaser = UnityEngine.Object.Instantiate(lineRenderer, position: __instance.transform.position - new Vector3(0, 2, 0), rotation: __instance.transform.rotation);
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

            Vector3 targetPosition = aimTransform.position + (targetDistance + 1) * aimTransform.forward;

            Vector3[] positions = new Vector3[2] { aimTransform.position + (1f * -aimTransform.up), targetPosition };
            CameraDroneLaser.SetPositions(positions);
        }
        public static void workColors()
        {
            Color defaultColour1 = new Color(77f / 255, 166f / 255, 255f / 255);
            Color defaultColour2 = new Color(0f, 255f / 255, 42f / 255);
            Color beamColour;
            if (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType))
            {
                if (!(QMod.Config.drill2RGB1 < 0 || QMod.Config.drill2RGB1 > 255 || QMod.Config.drill2RGB2 < 0 || QMod.Config.drill2RGB2 > 255 || QMod.Config.drill2RGB3 < 0 || QMod.Config.drill2RGB3 > 255))
                {
                    beamColour = new Color(QMod.Config.drill2RGB1 / 255f, QMod.Config.drill2RGB2 / 255f, QMod.Config.drill2RGB3 / 255f);
                }
                else
                {
                    beamColour = defaultColour2;
                }
                if (QMod.Config.drill2RGB1 == 0 && QMod.Config.drill2RGB2 == 0 && QMod.Config.drill2RGB3 == 0) { beamColour = new Color(255f / 255, 38f / 255, 147 / 255f); }
            }
            else
            {
                if (!(QMod.Config.drill1RGB1 < 0 || QMod.Config.drill1RGB1 > 255 || QMod.Config.drill1RGB2 < 0 || QMod.Config.drill1RGB2 > 255 || QMod.Config.drill1RGB3 < 0 || QMod.Config.drill1RGB3 > 255))
                {
                    beamColour = new Color(QMod.Config.drill1RGB1 / 255f, QMod.Config.drill1RGB2 / 255f, QMod.Config.drill1RGB3 / 255f);
                }
                else
                {
                    beamColour = defaultColour1;
                }
                if (QMod.Config.drill1RGB1 == 0 && QMod.Config.drill1RGB2 == 0 && QMod.Config.drill1RGB3 == 0) { beamColour = new Color(255, 38, 147); }
            }
            CameraDroneLaser.material.color = beamColour;
        }
    }
}
