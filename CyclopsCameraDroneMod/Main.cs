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

namespace CyclopsCameraDroneMod.Main
{
    [HarmonyPatch]
    public class Main
    {

        [HarmonyPatch]
        public class Postfixes
        {
            [HarmonyPatch(typeof(CyclopsExternalCams), nameof(CyclopsExternalCams.HandleInput))]
            [HarmonyPostfix]
            public static void HandleInputPatch()
            {
                if (Input.GetKeyUp(KeyCode.P))
                {
                    GameObject CyclopsCameraScreenObject = new GameObject("Cyclops Camera Screen");
                    MapRoomScreen CyclopsMapRoomScreen = CyclopsCameraScreenObject.AddComponent<MapRoomScreen>();

                    Vector3 position = Player.main.transform.position - new Vector3(0, 12, 0);

                    GameObject CyclopsCameraDroneObject = GameObject.Instantiate(CraftData.GetPrefabForTechType(TechType.MapRoomCamera, false), position, Player.main.transform.rotation);
                    MapRoomCamera CyclopsCameraDrone = CyclopsCameraDroneObject.GetComponent<MapRoomCamera>();

                    CyclopsCameraScreenObject.transform.position = Player.main.transform.position;
                    CyclopsCameraDrone.ControlCamera(Player.main, CyclopsMapRoomScreen);
                    CyclopsCameraDroneObject.gameObject.name = "CyclopsDroneCamera";
                }
            }

            [HarmonyPatch(typeof(MapRoomCamera), nameof(MapRoomCamera.FreeCamera))]
            [HarmonyPostfix]
            public static void ExitCameraPatch(MapRoomCamera __instance)
            {
                if (__instance.name == "CyclopsDroneCamera")
                {
                    GameObject.Destroy(__instance);
                }
            }
        }
    }

}
