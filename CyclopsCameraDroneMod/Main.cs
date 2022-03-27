using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
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
        //add scanning functionality
        //add way to pick up items
        //add new cool shit, what all exactly this entails is for you to decide and then DM me with.
        //add speed upgrades for all drones somehow, they too fuckin slow
        //more shit I guess, idk
        //command for all modules
        //TELEPORTATION! HELL YEA!
        //No seriously, solves problem of being too slow and is more cool shit. Just do it
        //sonar for drones

        public static string cameraObjectName = "CyclopsDroneCamera";

        public static float drillCooldownLength = 1f;
        public static float useDroneCooldownLength = 2f;
        public static float defaultBeamWidth = 0.15f;

        public static float timeLastDrill;
        public static float timeLastMineResource;
        public static float timeLastTractorBeam;

        public static float timeNextDrill;
        public static float timeNextUseDrone;

        public static LineRenderer cameraDroneLaser;
        public static LineRenderer lineRenderer;
        public static CyclopsDroneInstance droneInstance; // stores references to components on the camera itself and has most audio-related code

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
                if (Input.GetKeyUp(droneButton) && Time.time >= timeNextUseDrone)
                {
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
                cyclopsCameraDrone.gameObject.name = cameraObjectName;
                droneInstance = cyclopsCameraDrone.gameObject.AddComponent<CyclopsDroneInstance>();
                LargeWorldStreamer.main.cellManager.UnregisterEntity(cyclopsCameraDrone.gameObject);
                cyclopsCameraDrone.GetComponent<Pickupable>().isPickupable = false;

                Battery battery = GameObject.Instantiate(batteryPrefab).GetComponent<Battery>();
                cyclopsCameraDrone.energyMixin.battery = battery;
                battery.gameObject.transform.parent = cyclopsCameraDrone.gameObject.transform;

                yield return new WaitUntil(() => cyclopsCameraDrone.inputStackDummy != null);
                __instance.ExitCamera(); //happy Lee?
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

                if (__instance.name == cameraObjectName)
                {
                    GameObject.Destroy(__instance.gameObject);
                    timeNextUseDrone = Time.time + useDroneCooldownLength;
                }
                return false;
            }
            /*
            LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
			if (liveMixin)
			{
				if (liveMixin.IsWeldable())
				{
					liveMixin.AddHealth(5);
				}
				else
				{
					WeldablePoint weldablePoint = gameObject.FindAncestor<WeldablePoint>();
					if (weldablePoint != null && weldablePoint.transform.IsChildOf(liveMixin.transform))
					{
						liveMixin.AddHealth(5);
					}
				}
			}

            Targeting.GetTarget(mapRoomCamera.gameObject, 20, out var gameObject4, out float distance);
            PDAScanner.scanTarget.gameObject = gameObject4;
            PDAScanner.scanTarget.techType = CraftData.GetTechType(gameObject4);
            PDAScanner.Scan();
            */
            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.StabilizeRoll))]
            [HarmonyPrefix]
            public static bool StabilizeRolePatch(MapRoomCamera __instance)
            {
                return false;
            }
                [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.GetScreenDistance))]
            [HarmonyPostfix]
            public static void FixDistance(MapRoomCamera __instance, ref float __result)
            {
                if (__instance.name == cameraObjectName && !QMod.Config.InfiniteDistance)
                {
                    __result = (Player.main.transform.position - __instance.transform.position).magnitude;
                }
            }
            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.StabilizeRoll))]
            [HarmonyPostfix]
            public static void FixPlayerMovement(MapRoomCamera __instance)
            {
                if (__instance.name == cameraObjectName)
                {
                    Player.main.ExitLockedMode(false, false);
                    Player.main.EnterLockedMode(null);
                }
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.Update))]
            [HarmonyPostfix]
            public static void GetLookingTarget(MapRoomCamera __instance) //no longer just get's target, now also works keybinds
            {
                bool hasDrill2 = MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType);
                bool hasDrill1 = MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrill.thisTechType);
                if (Input.GetKeyUp(QMod.Config.beaconKey) && __instance.name == cameraObjectName) // Beacon placement
                {
                    BeaconFunctionality(__instance);
                }
                if (Input.GetKeyUp(QMod.Config.sonarKey) && MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, TechType.CyclopsSonarModule))
                {
                    __instance.energyMixin.ConsumeEnergy(2);
                    SNCameraRoot.main.SonarPing();
                }
                if(Input.GetKey(QMod.Config.repairKey) && MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, TechType.CyclopsSeamothRepairModule))
                {
                    Targeting.GetTarget(__instance.gameObject, 5, out var gameObject, out float distance);
                    LiveMixin liveMixin = gameObject.FindAncestor<LiveMixin>();
                    if (liveMixin)
                    {
                        if (liveMixin.IsWeldable())
                        {
                            liveMixin.AddHealth(5);
                        }
                        else
                        {
                            WeldablePoint weldablePoint = gameObject.FindAncestor<WeldablePoint>();
                            if (weldablePoint != null && weldablePoint.transform.IsChildOf(liveMixin.transform))
                            {
                                liveMixin.AddHealth(5);
                            }
                        }
                    }
                }
                if (Input.GetKey(QMod.Config.scanKey))
                {
                    Targeting.GetTarget(__instance.gameObject, 20, out var gameObject4, out float distance);
                    PDAScanner.scanTarget.gameObject = gameObject4;
                    PDAScanner.scanTarget.techType = CraftData.GetTechType(gameObject4);
                    PDAScanner.Scan();
                }
                if (!(hasDrill1 || hasDrill2))
                {
                    return;
                }
                if ((GameInput.GetButtonHeld(GameInput.Button.LeftHand) || Input.GetKey(QMod.Config.miningKey)) && __instance.name == cameraObjectName)
                {
                    DrillFunctionality(__instance, hasDrill1, hasDrill2);
                }
                else if(Input.GetKey(QMod.Config.interactKey) && (hasDrill1 || hasDrill2)) 
                {
                    TractorBeamFunctionality(__instance);
                }
                else 
                { 
                    cameraDroneLaser.enabled = false;
                }
                if(Input.GetKeyUp(QMod.Config.teleportKey) && hasDrill2) { __instance.transform.position += 10 * __instance.transform.forward; }
                if (Time.time > timeLastDrill + 0.5f) // stop the drilling sound when not drilling, but NOT immediately after releasing the key
                {
                    droneInstance.StopDrillSound();
                }
                if (Time.time < timeLastMineResource + 1f) // if you recently mined a resource, play the mining sound
                {
                    droneInstance.StartMineSound();
                }
                else // otherwise, make sure it isn't playing
                {
                    droneInstance.StopMineSound();
                }
                if (Time.time < timeLastTractorBeam + 1f) // if you recently fired the tractor beam, play its sound
                {
                    droneInstance.StartTractorBeamSound();
                }
                else // otherwise, make sure it isn't playing
                {
                    droneInstance.StopTractorBeamSound();
                }
            }
        }

        public static void DrillFunctionality(MapRoomCamera mapRoomCamera, bool hasDrill1, bool hasDrill2)
        {
            if (hasDrill2)
            {
                UpdateAppearance(QMod.Config.drill2RGB1, QMod.Config.drill2RGB2, QMod.Config.drill2RGB3, defaultBeamWidth, defaultBeamWidth);
            }
            else
            {
                UpdateAppearance(QMod.Config.drill1RGB1, QMod.Config.drill1RGB2, QMod.Config.drill1RGB3, defaultBeamWidth, defaultBeamWidth);
            }
            cameraDroneLaser.enabled = true;
            timeLastDrill = Time.time;
            droneInstance.StartDrillSound();
            SetBeamTarget(mapRoomCamera);
            float range = hasDrill2 ? QMod.Config.drillRange * 2 : QMod.Config.drillRange;
            Targeting.GetTarget(mapRoomCamera.gameObject, range, out var gameObject4, out float distance);
            if (gameObject4 != null)
            {
                Vector3 hitPoint = MainCameraControl.main.transform.position + MainCameraControl.main.transform.forward * distance;
                Drillable drillable = gameObject4.GetComponentInParent<Drillable>();
                if (drillable != null && (Time.time > timeNextDrill || hasDrill2))
                {
                    mapRoomCamera.energyMixin.ConsumeEnergy(5);
                    timeNextDrill = Time.time + drillCooldownLength;
                    timeLastMineResource = Time.time;
                    if (!hasDrill2)
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
                if (liveMixin != null && Time.time > timeNextDrill)
                {
                    mapRoomCamera.energyMixin.ConsumeEnergy(5);
                    timeNextDrill = Time.time + drillCooldownLength;
                    liveMixin.TakeDamage(30, hitPoint, DamageType.Drill);
                }
                BreakableResource resource = gameObject4.GetComponent<BreakableResource>() != null ? gameObject4.GetComponent<BreakableResource>() : gameObject4.GetComponentInParent<BreakableResource>();
                if (resource)
                {
                    resource.BreakIntoResources();
                }
            }
        }

        public static void BeaconFunctionality(MapRoomCamera mapRoomCamera)
        {
            SubRoot currentSub = Player.main.currentSub;
            if (currentSub == null) { return; }
            if (GameModeUtils.IsOptionActive(GameModeOption.NoCost))
            {
                CoroutineHost.StartCoroutine(CreateBeacon(mapRoomCamera.transform));
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
                CoroutineHost.StartCoroutine(CreateBeacon(mapRoomCamera.transform));
            }
        }

        public static void TractorBeamFunctionality(MapRoomCamera mapRoomCamera, bool hasDrill2 = false)
        {
            UpdateAppearance(QMod.Config.tractorBeamRGB1, QMod.Config.tractorBeamRGB2, QMod.Config.tractorBeamRGB3, TractorBeam.lineWidth, TractorBeam.lineWidth);
            cameraDroneLaser.enabled = true;
            timeLastTractorBeam = Time.time;
            SetBeamTarget(mapRoomCamera, true);

            var camTransform = Camera.current.transform;
            int colliders = Physics.SphereCastNonAlloc(new Ray(camTransform.position, camTransform.forward), TractorBeam.radius, TractorBeam.tractorBeamHit, TractorBeam.maxDistance, TractorBeam.tractorBeamLayerMask, QueryTriggerInteraction.Ignore);

            TractorBeam.Reset();

            if(Targeting.GetTarget(mapRoomCamera.gameObject, 10, out var gameObject2, out float distance))
            {
                StarshipDoor door = gameObject2.GetComponent<StarshipDoor>();
                if (door != null) { door.OnHandClick(Player.main.armsController.guiHand); }
            }

            for (int i = 0; i < colliders; i++)
            {
                if (Vector3.Distance(TractorBeam.tractorBeamHit[i].point, camTransform.position) < TractorBeam.pickupRange)
                {
                    GameObject gameObject = TractorBeam.tractorBeamHit[i].transform.gameObject;
                    Pickupable pickupable = gameObject.GetComponent<Pickupable>() != null ? gameObject.GetComponent<Pickupable>() : gameObject.GetComponentInParent<Pickupable>();
                    if (pickupable != null)
                    {
                        SubRoot currentSub = Player.main.currentSub;
                        if (currentSub != null)
                        {
                            CyclopsLocker[] cyclopsLockers = currentSub.gameObject.GetComponentsInChildren<CyclopsLocker>();
                            ItemsContainer emptyContainer = null;
                            foreach (CyclopsLocker locker in cyclopsLockers)
                            {
                                ItemsContainer cyclopsContainer = locker.gameObject.GetComponent<StorageContainer>().container;
                                if (cyclopsContainer != null && cyclopsContainer.HasRoomFor(pickupable))
                                {
                                    emptyContainer = cyclopsContainer;
                                    break;
                                }
                            }
                            if (emptyContainer != null)
                            {
                                CyclopsLockerPickup(emptyContainer, pickupable);
                            }
                            else
                            {
                                pickupable.OnHandClick(Player.main.armsController.guiHand);
                            }
                        }
                    }
                }
                else
                {
                    TractorBeam.Attract(camTransform, TractorBeam.tractorBeamHit[i].collider);
                }
            }

            return;
            // OLD CODE:
            /*
            Targeting.GetTarget(mapRoomCamera.gameObject, QMod.Config.drillRange, out var gameObject4, out _);
            if(gameObject4 == null)
            {
                return;
            }
            Pickupable pickupable = gameObject4.GetComponent<Pickupable>() != null ? gameObject4.GetComponent<Pickupable>() : gameObject4.GetComponentInParent<Pickupable>();
            if (pickupable != null)
            {
                SubRoot currentSub = Player.main.currentSub;
                if (currentSub != null)
                {
                    CyclopsLocker[] cyclopsLockers = currentSub.gameObject.GetComponentsInChildren<CyclopsLocker>();
                    ItemsContainer emptyContainer = null;
                    foreach (CyclopsLocker locker in cyclopsLockers)
                    {
                        ItemsContainer cyclopsContainer = locker.gameObject.GetComponent<StorageContainer>().container;
                        if (cyclopsContainer != null && cyclopsContainer.HasRoomFor(pickupable))
                        {
                            emptyContainer = cyclopsContainer;
                            break;
                        }
                    }
                    if (emptyContainer != null)
                    {
                        CyclopsLockerPickup(emptyContainer, pickupable);
                    }
                    else
                    {
                        pickupable.OnHandClick(Player.main.armsController.guiHand);
                    }
                }
            }*/
        }
        public static IEnumerator CreateBeacon(Transform transform)
        {
            droneInstance.PlayBeaconSound();

            var coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.Beacon, false);
            yield return coroutineTask;
            var prefab = coroutineTask.GetResult();

            GameObject.Instantiate(prefab, transform.position - 0.5f * transform.forward, transform.rotation);

            ErrorMessage.AddMessage("Beacon deployed!");
        }

        public static bool CyclopsLockerPickup(ItemsContainer container, Pickupable pickupable)
        {
            if (!container.HasRoomFor(pickupable))
            {
                return false;
            }
            Vector3 position = pickupable.gameObject.transform.position;
            TechType techType = pickupable.GetTechType();
            pickupable = pickupable.Pickup(true);
            InventoryItem item = new InventoryItem(pickupable);
            if (!((IItemsContainer)container).AddItem(item)) //I don't know what the fuck this line does, but it was in inventory.pickup so I'm keeping it
            {
                container.UnsafeAdd(item);
            }
            KnownTech.Analyze(pickupable.GetTechType(), true);
            if (Utils.GetSubRoot() != null)
            {
                pickupable.destroyOnDeath = false;
            }
            SkyEnvironmentChanged.Send(pickupable.gameObject, Player.main.GetSkyEnvironment());
            return true;
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
                                ItemsContainer cyclopsContainer = currentSub.gameObject.GetComponentInChildren<CyclopsLocker>().gameObject.GetComponent<StorageContainer>().container;
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
            if (cameraDroneLaser != null) return;
            GameObject cannon_pylon_left = CraftData.InstantiateFromPrefab(TechType.PowerTransmitter);
            cannon_pylon_left.transform.SetParent(__instance.transform, false);
            Utils.ZeroTransform(cannon_pylon_left.transform);

            GameObject laserBeam = GameObject.Instantiate(cannon_pylon_left.GetComponent<PowerFX>().vfxPrefab, __instance.transform.position - new Vector3(0, 2, 0), __instance.transform.rotation);
            laserBeam.SetActive(true);

            lineRenderer = laserBeam.GetComponent<LineRenderer>();
            lineRenderer.startWidth = 0.15f;
            lineRenderer.endWidth = 0.15f;
            lineRenderer.positionCount = 2;
            
            cameraDroneLaser = UnityEngine.Object.Instantiate(lineRenderer, position: __instance.transform.position - new Vector3(0, 2, 0), rotation: __instance.transform.rotation);
            GameObject.DestroyImmediate(laserBeam);
            GameObject.DestroyImmediate(cannon_pylon_left);
        }
        public static void SetBeamTarget(MapRoomCamera __instance, bool inverted = false)
        {
            if (Targeting.GetTarget(__instance.gameObject, MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, out GameObject targetGameobject, out float targetDist))
            {
                CalculateBeamVectors(targetDist, __instance, inverted);
            }
            else
                CalculateBeamVectors(MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType) ? QMod.Config.drillRange * 2 : QMod.Config.drillRange, __instance, inverted);
        }

        public static void CalculateBeamVectors(float targetDistance, MapRoomCamera __instance, bool inverted)
        {

            Transform aimTransform = __instance.transform;

            Vector3 targetPosition = aimTransform.position + (targetDistance + 1) * aimTransform.forward;

            Vector3[] positions;
            if (inverted) { positions = new Vector3[2] { targetPosition, aimTransform.position + (1f * -aimTransform.up) }; }
            else { positions = new Vector3[2] { aimTransform.position + (1f * -aimTransform.up), targetPosition }; }
            cameraDroneLaser.SetPositions(positions);
        }
        public static void UpdateAppearance(float red = 77, float green = 166, float blue = 255, float startWidth = 0.15f, float endWidth = 0.15f)
        {
            Color beamColour = new Color(77 / 255, 166 / 255, 1);
            if (!(red < 0 || red > 255 || green < 0 || green > 255 || blue < 0 || blue > 255))
            {
                beamColour = new Color(red / 255f, green / 255f, blue / 255f);
            }
            if (red == 0 && green == 0 && blue == 0) { beamColour = new Color(1, 38f / 255, 147 / 255f); }
            cameraDroneLaser.material.color = beamColour;
            cameraDroneLaser.startWidth = startWidth;
            cameraDroneLaser.endWidth = endWidth;
        }
    }
}
