using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QModManager.API.ModLoading;
using QModManager.Utility;
using HarmonyLib;
using System.Reflection;
using MoreCyclopsUpgrades.API;
using SMLHelper.V2.Options.Attributes;
using Logger = QModManager.Utility.Logger;
using SMLHelper.V2.Json;
using SMLHelper.V2.Handlers;

namespace CyclopsHullReinforcement.QMods
{
    [Menu("CyclopsHullReinforcment")]
    public class Config : ConfigFile
    {
        public static bool DevStuff = false;
    }

    [QModCore]
    public class QMods
    {
        internal static Config Config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public static void QModStuff()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            new Module.Module.CyclopsHullReinforcement().Patch();
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new Module.Module.CyclopsHullReinforcmentUpgradeHandler(Module.Module.CyclopsHullReinforcement.thisTechType, cyclops);
            });
        }
    }
}
