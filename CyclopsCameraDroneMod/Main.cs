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
            public static void HandleInputPatch(ref bool __result)
            {
                if (Input.GetKeyUp(KeyCode.P))
                {
                    MapRoomScreen CyclopsCameraScreenObject = new MapRoomScreen();
                    Vector3 Vector3Drone = Player.main.transform.position - new Vector3(0,12,0);
                    Vector3 position = Vector3Drone;
                    MapRoomCamera CyclopsCameraDroneObject = new MapRoomCamera();
                    Quaternion rotation = CyclopsCameraDroneObject.transform.rotation;
                    GameObject.Instantiate(CyclopsCameraDroneObject, position: position, rotation);
                    CyclopsCameraScreenObject.transform.position = Player.main.transform.position;
                    CyclopsCameraDroneObject.ControlCamera(Player.main, CyclopsCameraScreenObject);
                }
                __result = true;
            }

            [HarmonyPatch(typeof(CyclopsExternalCams), nameof(CyclopsExternalCams.ExitCamera))]
            [HarmonyPostfix]
            public void ExitCameraPatch(MapRoomCamera CyclopsCameraDroneObject)
            {
                if (CyclopsCameraDroneObject != null)
                {
                    GameObject.Destroy(CyclopsCameraDroneObject);
                }
            }
        }
    }

}
