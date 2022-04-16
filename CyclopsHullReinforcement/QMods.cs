using QModManager.API.ModLoading;
using System.Reflection;
using MoreCyclopsUpgrades.API;
using Logger = QModManager.Utility.Logger;

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
            new Module.Module.CyclopsHullReinforcement().Patch();
            MCUServices.Register.CyclopsUpgradeHandler((SubRoot cyclops) =>
            {
                return new Module.Module.CyclopsHullReinforcmentUpgradeHandler(Module.Module.CyclopsHullReinforcement.thisTechType, cyclops)
                {
                    OnClearUpgrades = () => { cyclops.gameObject.EnsureComponent<Module.Module.ModuleStuff>().enabled = false; },
                    OnUpgradeCounted = () => { cyclops.gameObject.EnsureComponent<Module.Module.ModuleStuff>().enabled = true; },
                };
            });
        }
    }
}