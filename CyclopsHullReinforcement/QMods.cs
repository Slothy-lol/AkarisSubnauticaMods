using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QModManager.API.ModLoading;
using QModManager.Utility;
using HarmonyLib;
using System.Reflection;

namespace CyclopsHullReinforcement.QMods
{
    [QModCore]
    public class QMods
    {
        [QModPatch]
        public static void QModStuff()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            new Module.Module.CyclopsHullReinforcement().Patch();
        }
    }
}
