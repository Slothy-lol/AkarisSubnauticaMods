using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using MoreCyclopsUpgrades.API;

namespace CyclopsHullReinforcement.Patches
{
    [HarmonyPatch(typeof(DamageSystem), nameof(DamageSystem.CalculateDamage))]
    class Postfix
    {
        [HarmonyPostfix]
        public static void DamagePatch(GameObject target, ref float __result)
        {
            if (target.GetComponent<CyclopsDecoyManager>() != null && MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Module.Module.CyclopsHullReinforcement.thisTechType))
            {
                __result /= 2;   
            }
        }
    }
}
