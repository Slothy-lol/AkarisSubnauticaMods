using HarmonyLib;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Reflection;
using UnityEngine;
using Logger = QModManager.Utility.Logger;

namespace CyclopsCameraDroneMod.QMods
{
    [QModCore]
    public static class QMod
    {
        internal static Config Config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();
        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            Logger.Log(Logger.Level.Info, "Patched successfully!");
            /*new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModule().Patch();
            new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrill().Patch();
            new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrillMK2().Patch();*/
        }

    }
    [Menu("Cyclops Camera Drone")]
    public class Config : ConfigFile
    {
        [Keybind("Drone Key", Tooltip = "When on the cyclops cameras, use this key to swap to a drone")]
        public KeyCode droneKey = KeyCode.P;

        [Keybind("Mining Key", Tooltip = "When using the cyclops drone, press this key to mine the drillable being targeted")]
        public KeyCode miningKey = KeyCode.Q;

        [Keybind("Beacon Key", Tooltip = "When using the cyclops drone, press this key to spawn a beacon at the drones location")]
        public KeyCode beaconKey = KeyCode.E;

        [Toggle("Infinite Distance", Tooltip = "When enabled, there will be no limit on the Cyclops Camera Drone's range")]
        public bool InfiniteDistance = false;

        [Slider("Infinite Distance", Max = 50, Min = 5, Tooltip = "When enabled, there will be no limit on the Cyclops Camera Drone's range")]
        public int drillRange = 10;
        //Make a keybind for spawning a beacon
    }
}
