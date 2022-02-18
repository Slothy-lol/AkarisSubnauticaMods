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
    
private static readonly Type VehicleUpgraderType = Type.GetType("UpgradedVehicles.VehicleUpgrader, UpgradedVehicles", false, false);
private static readonly MethodInfo VehicleUpgraderAddEfficiencyBonus = VehicleUpgraderType?.GetMethod("AddEfficiencyBonus", BindingFlags.Public | BindingFlags.Static);

public static bool AddUVEfficiencyBonus(TechType module = VehiclePowerUpgradeModuleMK2.thisTechType, float efficiencybonus = 2f, bool bForce = false)
{
    if (VehicleUpgraderAddEfficiencyBonus == null)
        return false;

    VehicleUpgraderAddEfficiencyBonus.Invoke(null, new object[] {module, efficiencybonus, bForce});
    return true;
}  
       
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnUpgradeModuleChange))]
    class Patch
    {
        [HarmonyPostfix]
        public static void PostUpgradeModuleChange(Vehicle __instance, TechType techType)
        {
            if (!QModServices.Main.ModPresent("UpgradedVehicles"))
            {
                if (techType == VehiclePowerUpgradeModuleMK2.thisTechType || techType == TechType.VehiclePowerUpgradeModule)
                {
                    __instance.enginePowerRating += 1f + (__instance.modules.GetCount(TechType.VehiclePowerUpgradeModule) + (2f * __instance.modules.GetCount(VehiclePowerUpgradeModuleMK2.thisTechType)));
                    ErrorMessage.AddMessage(Language.main.GetFormat("PowerRatingNowFormat", __instance.enginePowerRating));
                }
            }
        }
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
}
