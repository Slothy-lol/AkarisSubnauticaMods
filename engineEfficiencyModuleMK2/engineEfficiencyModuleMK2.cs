using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Assets;
using QModManager.API.ModLoading;
using HarmonyLib;
using RecipeData = SMLHelper.V2.Crafting.TechData;
using SMLHelper.V2.Crafting;
using Logger = QModManager.Utility.Logger;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using QModManager.API;

namespace engineEfficiencyModuleMK2
{
    public class VehiclePowerUpgradeModuleMK2 : Equipable
    {
        public static TechType thisTechType;
        public VehiclePowerUpgradeModuleMK2() : base("VehiclePowerUpgradeModuleMK2", "Engine Efficiency Module MK2", "Boosts engine efficiency by double the mark 1 variant.")
        {
            OnFinishedPatching += () =>
            {
                VehiclePowerUpgradeModuleMK2.thisTechType = this.TechType;
            };
        }
        public override EquipmentType EquipmentType => EquipmentType.VehicleModule;
        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;
        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.SeamothUpgrades;
        public override string[] StepsToFabricatorTab => new string[] { "CommonModules" };
        public override float CraftingTime => 10f;
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;
        protected override Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.VehiclePowerUpgradeModule);
        }
        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.VehiclePowerUpgradeModule, 1),
                        new Ingredient(TechType.AdvancedWiringKit, 1),
                        new Ingredient(TechType.AluminumOxide, 2)

                    }
                )
            };
        }

        public override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(TechType.VehiclePowerUpgradeModule);
            var obj = GameObject.Instantiate(prefab);
            return obj;
        }


    }
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnUpgradeModuleChange))]
    class Patch
    {
        [HarmonyPostfix]
        public static void PostUpgradeModuleChange(Vehicle __instance, TechType techType)
        {

            if (techType == VehiclePowerUpgradeModuleMK2.thisTechType || techType == TechType.VehiclePowerUpgradeModule)
            {
                __instance.enginePowerRating += 1f + (__instance.modules.GetCount(TechType.VehiclePowerUpgradeModule) + (2f * __instance.modules.GetCount(VehiclePowerUpgradeModuleMK2.thisTechType)));
                ErrorMessage.AddMessage(Language.main.GetFormat("PowerRatingNowFormat", __instance.enginePowerRating));
            }
            
        }
    }

    class UPPatchClass0
    {
        [HarmonyPostfix]
        public static int UVPatch0(Vehicle __instance, ref int __result)
        {
            return __result += 2 * __instance.modules.GetCount(VehiclePowerUpgradeModuleMK2.thisTechType);
        }
    }
    class PatchIfExistsClass
    {
        public static void PatchIfExists(Harmony harmony, string assemblyName, string typeName, string methodName, HarmonyMethod prefix, HarmonyMethod postfix, HarmonyMethod transpiler)
        {
            var assembly = FindAssembly(assemblyName);
            if (assembly == null)
            {
                Logger.Log(Logger.Level.Debug, "Could not find assembly " + assemblyName + ", don't worry this probably just means you don't have the mod installed");
                return;
            }

            Type targetType = assembly.GetType(typeName);
            if (targetType == null)
            {
                Logger.Log(Logger.Level.Debug, "Could not find class/type " + typeName + ", the mod/assembly " + assemblyName + " might have changed");
                return;
            }

            Logger.Log(Logger.Level.Debug, "Found targetClass " + typeName);
            var targetMethod = AccessTools.Method(targetType, methodName);
            if (targetMethod != null)
            {
                Logger.Log(Logger.Level.Debug, "Found targetMethod " + typeName + "." + methodName + ", Patching...");
                harmony.Patch(targetMethod, prefix, postfix, transpiler);
                Logger.Log(Logger.Level.Debug, "Patched " + typeName + "." + methodName);
            }
            else
            {
                Logger.Log(Logger.Level.Debug, "Could not find method " + typeName + "." + methodName + ", the mod/assembly " + assemblyName + " might have been changed");
            }
        }


        private static Assembly FindAssembly(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.StartsWith(assemblyName + ","))
                    return assembly;

            return null;
        }


    }

        [QModCore]
        public static class QMod
        {
            [QModPatch]
            public static void Patch()
            {
                var assembly = Assembly.GetExecutingAssembly();
                var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
                Logger.Log(Logger.Level.Info, $"Patching {modName}");
                Harmony harmony = new Harmony(modName);
                harmony.PatchAll(assembly);
                Logger.Log(Logger.Level.Info, "Patched successfully!");
                new VehiclePowerUpgradeModuleMK2().Patch();
            }
        }

    
    public static class UVPatch0
    {
        public static void UpVPatch0()
        {
            Logger.Log(Logger.Level.Debug, "Patching...");

            var myPostfixMethod = new HarmonyMethod(AccessTools.Method(typeof(UPPatchClass0), "UPPatch0"));
            var harmony = new Harmony("Akari.EngineEffModMK2");
            PatchIfExistsClass.PatchIfExists(harmony, "UpgradedVehicles", "UpgradedVehicles.VehicleUpgrader", "GetEfficiencyBonus", null, myPostfixMethod, null);

            Logger.Log(Logger.Level.Debug, "Patching complete");
        }
    }

}