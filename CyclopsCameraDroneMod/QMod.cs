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

        }
    }
    [Menu("Cyclops Camera Drone")]
    public class Config : ConfigFile
    {
        [Keybind("Drone Key", Tooltip = "When on the cyclops cameras, use this key to swap to a drone")]
        public KeyCode droneKey = KeyCode.P;
    }
}
