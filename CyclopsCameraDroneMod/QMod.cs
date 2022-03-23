using CyclopsCameraDroneMod.Modules;
using HarmonyLib;
using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.Upgrades;
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
            new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModule().Patch();
            new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrill().Patch();
            new CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrillMK2().Patch();
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneUpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModule.thisTechType, cyclops);
            });
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneDrillUpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrill.thisTechType, cyclops);
            });
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneDrillMK2UpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDroneModuleDrillMK2.thisTechType, cyclops);
            });
        }

    }
    [Menu("Cyclops Camera Drone")]
    public class Config : ConfigFile
    {
        [Keybind("Drone Key", Tooltip = "When on the cyclops cameras, use this key to swap to a drone")]
        public KeyCode droneKey = KeyCode.P;

        [Keybind("Mining Key", Tooltip = "When using the cyclops drone, press this key to mine the drillable being targeted")]
        public KeyCode miningKey = KeyCode.Q;

        [Keybind("Beacon Key", Tooltip = "When using the cyclops drone, press this key to spawn a beacon at the drones location. Uses a beacon from cyclops lockers first, then player inventory if there are no beacons found..")]
        public KeyCode beaconKey = KeyCode.B;

        [Toggle("Infinite Distance", Tooltip = "When enabled, there will be no limit on the Cyclops Camera Drone's range.")]
        public bool InfiniteDistance = false;

        [Slider("Drill Range", Max = 50, Min = 5, DefaultValue = 7.0F, Step = 1.0F, Tooltip = "Range in meters of how far the cyclops drone can drill from.")]
        public int drillRange = 10;

        public int drill1RGB1 = 77;
        public int drill1RGB2 = 166;
        public int drill1RGB3 = 255;

        public int drill2RGB1 = 0;
        public int drill2RGB2 = 255;
        public int drill2RGB3 = 42;
    }
}
