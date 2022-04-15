using CyclopsCameraDroneMod.Modules;
using cyclopsVehiclebayHUDIcon;
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
            new CyclopsCameraDrone().Patch();
            new CyclopsCameraDroneIndustry().Patch();
            new CyclopsCameraDroneUltimate().Patch();
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneUpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDrone.thisTechType, cyclops);
            });
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneDrillUpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDroneIndustry.thisTechType, cyclops);
            });
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new CyclopsDroneDrillMK2UpgradeHandler(CyclopsCameraDroneMod.Modules.CyclopsCameraDroneUltimate.thisTechType, cyclops);
            });
            MCUServices.Register.CyclopsStatusIcon<HUDIcon.MySubStatus>((SubRoot cyclops) => new HUDIcon.MySubStatus(cyclops));
        }
    }
    [Menu("Cyclops Camera Drone")]
    public class Config : ConfigFile
    {
        [Keybind("Drone Key", Tooltip = "When on the cyclops cameras, use this key to swap to a drone.")]
        public KeyCode droneKey = KeyCode.P;

        [Keybind("Mining Key", Tooltip = "When using the cyclops drone, press this key to mine the drillable being targeted.")]
        public KeyCode miningKey = KeyCode.R;

        [Keybind("Beacon Key", Tooltip = "When using the cyclops drone, press this key to spawn a beacon at the drones location. Uses a beacon from cyclops lockers first, then player inventory if there are no beacons found.")]
        public KeyCode beaconKey = KeyCode.B;

        [Keybind("Interact Key", Tooltip = "When using the cyclops drone, press this key to pick up the object the drone is looking at.")]
        public KeyCode interactKey = KeyCode.Q;

        [Keybind("Sonar Key", Tooltip = "When using the cyclops drone, press this key to activate sonar. Only fuctions if the cyclops sonar module is present.")]
        public KeyCode sonarKey = KeyCode.Z;

        [Keybind("Shield Key", Tooltip = "When using the ion cyclops drone, press this key to activate A shield. Only fuctions if the cyclops shield module is present.")]
        public KeyCode shieldKey = KeyCode.K;

        /*Too lazy to make this. Shouldn't be hard, just more work than I care to put in for something I don't care about.
        [Toggle("Prioritize Player Inventory", Tooltip = "If checked, will priotize putting items into the players inventory first and then the cyclops lockers second. if unchecked, uses cyclops lockers first.")]
        public bool useInventory = false;
        */
        [Slider("Drill Range", Max = 50, Min = 5, DefaultValue = 15, Step = 1.0F, Tooltip = "Range in meters of how far the cyclops drone can drill from.")]
        public int drillRange = 15;

        [Choice("Drone Energy usage", new[] { "All", "Some", "None" }, Tooltip = "All means all energy drains from drone, none from cyclops. Some means that only the base energy drain from moving will drain from drone. None means that all drain comes from cyclops")]
        public string energyUsageType = "Some"; //make more descriptive
                                                //All means all energy drains from drone, none from cyclops
                                                //Some means that only the base energy drain from moving will drain from drone, laser and tractor beam and shit come from cyclops
                                                //None means that the cyclops constantly tops up the drone, effectively meaning that all of the energy is being drained from the cyclops instead

        public int drill1RGB1 = 77;
        public int drill1RGB2 = 166;
        public int drill1RGB3 = 255;

        public int drill2RGB1 = 0;
        public int drill2RGB2 = 255;
        public int drill2RGB3 = 42;

        public int tractorBeamRGB1 = 179;
        public int tractorBeamRGB2 = 0;
        public int tractorBeamRGB3 = 179;

        public KeyCode teleportKey = KeyCode.P;
        public KeyCode scanKey = KeyCode.F;
        public KeyCode repairKey = KeyCode.X;
        public bool autoSonar = false;
        public bool fuckAutoStabilization = false;
        public KeyCode drone2Key = KeyCode.L;
        public bool infiniteDistance = false;
        public bool variableEnergyDrain = true;
    }
}
