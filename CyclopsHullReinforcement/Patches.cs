using HarmonyLib;
using UnityEngine;
using MoreCyclopsUpgrades.API;

namespace CyclopsHullReinforcement.Patches
{
    [HarmonyPatch(typeof(DamageSystem), nameof(DamageSystem.CalculateDamage))]
    class Postfix
    {
        public static bool didWork;
        [HarmonyPostfix]
        public static void DamagePatch(GameObject target, ref float __result)
        {
            if (target.GetComponentInParent<SubRoot>().isCyclops == true && MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Module.Module.CyclopsHullReinforcement.thisTechType))
            {
                didWork = true;
                __result /= 2;  
            }
            else { didWork = false;  }
            if (QMods.Config.DevStuff) { ErrorMessage.AddMessage($"{target.name}, {didWork}, {__result}"); }
        }
    }
}