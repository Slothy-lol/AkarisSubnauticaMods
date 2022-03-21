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

namespace CyclopsCameraDroneMod.Main
{
    [HarmonyPatch]
    public class Main
    {
        public static string CameraName = "CyclopsDroneCamera";
        [HarmonyPatch]
        public class Postfixes
        {
            [HarmonyPatch(typeof(CyclopsExternalCams), nameof(CyclopsExternalCams.HandleInput))]
            [HarmonyPostfix]
            public static void HandleInputPatch(CyclopsExternalCams __instance, ref bool __result)
            {
                /*if (__instance.GetUsingCameras())
                {
                    Logger.Log(Logger.Level.Info, "Not using cameras");
                    return;
                }*/
                KeyCode droneButton = QMod.Config.droneKey;
                if(Input.GetKeyUp(droneButton))
                {
                    __instance.ExitCamera();

                    CoroutineHost.StartCoroutine(createAndControl(__instance));

                    __result = true;
                }
            }
            private static IEnumerator createAndControl(CyclopsExternalCams __instance)
            {
                var coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.MapRoomCamera, false);
                yield return coroutineTask;
                var prefab = coroutineTask.GetResult();

                coroutineTask = CraftData.GetPrefabForTechTypeAsync(TechType.Battery, false);
                yield return coroutineTask;
                var batteryPrefab = coroutineTask.GetResult();

                Vector3 position = GetSpawnPosition(__instance.transform.parent.gameObject, Player.main.gameObject);
                MapRoomCamera cyclopsCameraDrone = GameObject.Instantiate(prefab, position, Player.main.transform.rotation).GetComponent<MapRoomCamera>();
                cyclopsCameraDrone.gameObject.name = CameraName;

                cyclopsCameraDrone.energyMixin.battery = GameObject.Instantiate(batteryPrefab).GetComponent<Battery>();

                yield return new WaitUntil(() => cyclopsCameraDrone.inputStackDummy != null);
                if (Player.main.currChair != null) { Player.main.ExitPilotingMode(); }
                cyclopsCameraDrone.ControlCamera(Player.main, null);
                Player.main.ExitLockedMode(false, false);
                Player.main.EnterLockedMode(null);
            }
            static Vector3 GetSpawnPosition(GameObject cyclopsObject, GameObject playerObject)
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

                return combinedMatrix.MultiplyPoint3x4(new Vector3(0, -12, 0)); // set the position relative to the Cyclops' Rotation and scale and the Player's Position
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.FreeCamera))]
            [HarmonyPrefix]
            public static bool ExitCameraPatch(MapRoomCamera __instance, bool resetPlayerPosition)
            {
                if(__instance.screen != null)
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

                if(__instance.name == CameraName)
                {
                    GameObject.Destroy(__instance.gameObject);
                }
                return false;
            }
            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.GetScreenDistance))]
            [HarmonyPostfix]
            public static void fixDistance(MapRoomCamera __instance, ref float __result)
            {
                if(__instance.name == CameraName/* && !QMods.Config.InfiniteDistance*/) //make config for infinite distance eventually
                {
                    __result = (Player.main.transform.position - __instance.transform.position).magnitude/* - QMods.Config.ExtraDistance*/; //make config for extra distance eventually
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
        }
    }
}
