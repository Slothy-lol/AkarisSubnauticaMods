using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QModManager;
using SMLHelper.V2.Options;
using System.Threading.Tasks;
using SMLHelper.V2.Json;
using SMLHelper.V2.Utility;
using UnityEngine;
using SMLHelper.V2.Options.Attributes;
using System.Reflection;
using Logger = QModManager.Utility.Logger;
using QModManager.API.ModLoading;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using CameraDroneUpgrades.API;

namespace Camera_Drone_Sonar.QMods
{
    [QModCore]
    public static class QMods
    {
        internal static Config Config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public static void Patch()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
            var item = new Item.CameraDroneSonarModule();
            item.Patch();
            var sonar = new SonarFunctionality.SonarFunctionality();
            sonar.upgrade = Registrations.RegisterDroneUpgrade("CameraDroneSonar", item, sonar.SetUp);
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            Logger.Log(Logger.Level.Info, "Patched successfully!");
        }
    }

    [Menu("Camera Drone Sonar")]
    public class Config : ConfigFile
    {
        [Keybind("Sonar Key", Tooltip = "When using the Camera Drone (with sonar module equipped), press this key to activate sonar.")]
        public KeyCode sonarKey = KeyCode.Z;
    }
}