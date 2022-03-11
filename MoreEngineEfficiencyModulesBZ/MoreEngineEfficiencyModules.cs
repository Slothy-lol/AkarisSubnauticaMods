using HarmonyLib;
using QModManager.API;
using QModManager.API.ModLoading;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Logger = QModManager.Utility.Logger;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Json;
using SMLHelper.V2.Handlers;

namespace MoreEngineEfficiencyModules
{
    public class VehiclePowerUpgradeModuleMK2 : Equipable
    {
        public static TechType thisTechType;
        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public VehiclePowerUpgradeModuleMK2() : base("VehiclePowerUpgradeModuleMK2", "Engine Efficiency Module MK2", "Boosts engine efficiency by 1.75x the mark 1 variant.")
        {
            OnFinishedPatching += () =>
            {
                VehiclePowerUpgradeModuleMK2.thisTechType = this.TechType;
                VehicleUpgraderFix.AddUVEfficiencyBonus(VehiclePowerUpgradeModuleMK2.thisTechType, bForce: false);
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.VehicleModule;
        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;
        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;
        public override string[] StepsToFabricatorTab => new[] { QMod.WorkBenchTab };
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "EngineEfficiencyModuleMK2Sprite.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.VehiclePowerUpgradeModule, 1),
                        new Ingredient(TechType.ComputerChip, 1),
                        new Ingredient(TechType.GenericRibbon, 4),
                        new Ingredient(TechType.AluminumOxide, 2)

                    }
                )
            };
        }

        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = CraftData.GetPrefabForTechTypeAsync(TechType.VehiclePowerUpgradeModule, false);
            yield return task;
            var prefab = GameObject.Instantiate(task.GetResult());
            gameObject.Set(prefab);
        }
    }
    public class VehiclePowerUpgradeModuleMK3 : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public VehiclePowerUpgradeModuleMK3() : base("VehiclePowerUpgradeModuleMK3", "Engine Efficiency Module MK3", "Boosts engine efficiency by 2.5x the mark 1 variant.")
        {
            OnFinishedPatching += () =>
            {
                VehiclePowerUpgradeModuleMK3.thisTechType = this.TechType;
                VehicleUpgraderFix.AddUVEfficiencyBonus(VehiclePowerUpgradeModuleMK3.thisTechType ,bForce: false);
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.VehicleModule;
        public override TechType RequiredForUnlock => TechType.BaseUpgradeConsole;
        public override TechGroup GroupForPDA => TechGroup.VehicleUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.VehicleUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.Workbench;
        public override string[] StepsToFabricatorTab => new[] { QMod.WorkBenchTab };
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.Passive;
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "EngineEfficiencyModuleMK3Sprite.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(VehiclePowerUpgradeModuleMK2.thisTechType, 1),
                        new Ingredient(TechType.AdvancedWiringKit, 1),
                        new Ingredient(TechType.Benzene, 2),
                        new Ingredient(TechType.Kyanite, 2)

                    }
                )
            };
        }
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            var task = CraftData.GetPrefabForTechTypeAsync(TechType.VehiclePowerUpgradeModule, false);
            yield return task;
            var prefab = GameObject.Instantiate(task.GetResult());
            gameObject.Set(prefab);
        }

    }

    class VehicleUpgraderFix
    {
        private static readonly Type VehicleUpgraderType = Type.GetType("UpgradedVehicles.VehicleUpgrader, UpgradedVehicles", false, false);
        private static readonly MethodInfo VehicleUpgraderAddEfficiencyBonus = VehicleUpgraderType?.GetMethod("AddEfficiencyBonus", BindingFlags.Public | BindingFlags.Static);

        public static bool AddUVEfficiencyBonus(TechType module, bool bForce = false)
        {
            float efficiencybonus;
            if (VehicleUpgraderAddEfficiencyBonus == null)
                return false;
            if (module == VehiclePowerUpgradeModuleMK2.thisTechType) {
                efficiencybonus = 1.75f;
                 VehicleUpgraderAddEfficiencyBonus.Invoke(null, new object[] { VehiclePowerUpgradeModuleMK2.thisTechType, efficiencybonus, bForce });
                 return true;
            }
            efficiencybonus = 2.5f;
            VehicleUpgraderAddEfficiencyBonus.Invoke(null, new object[] { VehiclePowerUpgradeModuleMK3.thisTechType, efficiencybonus, bForce });
            return true;
        }
    }

    [HarmonyPatch(typeof(Exosuit), nameof(Exosuit.OnUpgradeModuleChange))]
    class Patch
    {
        [HarmonyPostfix]
        public static void PostUpgradeModuleChange(Exosuit __instance, bool added, TechType techType)
        {
            if (!QModServices.Main.ModPresent("UpgradedVehicles"))
            {
                if (techType == TechType.VehiclePowerUpgradeModule || techType == VehiclePowerUpgradeModuleMK2.thisTechType || techType == VehiclePowerUpgradeModuleMK3.thisTechType)
                {
                    ExtraMethods.EfficiencyHandler(__instance, added, techType);
                    return;
                }
            }
        }
    }

    public class ExtraMethods
    {
        public static void EfficiencyHandler(Exosuit __instance, bool added, TechType techType)
        {
            if (techType == TechType.VehiclePowerUpgradeModule || techType == VehiclePowerUpgradeModuleMK2.thisTechType || techType == VehiclePowerUpgradeModuleMK3.thisTechType)
            {
                __instance.enginePowerRating = 1f + (__instance.modules.GetCount(TechType.VehiclePowerUpgradeModule) + (1.75f * __instance.modules.GetCount(VehiclePowerUpgradeModuleMK2.thisTechType) + (2.5f * __instance.modules.GetCount(VehiclePowerUpgradeModuleMK3.thisTechType))));
                ErrorMessage.AddMessage(Language.main.GetFormat("PowerRatingNowFormat", __instance.enginePowerRating));
                return;
            }
        }
    }
    [HarmonyPatch(typeof(SeaTruckUpgrades), nameof(SeaTruckUpgrades.OnUpgradeModuleChange))]
    class Patch2
    {
        [HarmonyPostfix]
        public static void PostUpgradeModuleChange2(SeaTruckUpgrades __instance, TechType techType, bool added)
        {
            if (!QModServices.Main.ModPresent("UpgradedVehicles"))
            {
                __instance.modules.GetCount(techType);
                switch (techType)
                {
                    case TechType.SeaTruckUpgradeEnergyEfficiency:
                        if (added)
                        {
                            if (QMod.Config.SeatruckEfficiencyBoost.Equals(true))
                            {
                                __instance.motor.powerEfficiencyFactor = 0.8f;
                                break;
                            }
                            else
                            {
                                __instance.motor.powerEfficiencyFactor = 0.9f;
                                break;
                            }
                        }
                        if (QMod.Config.SeatruckEfficiencyBoost.Equals(true))
                        {
                            __instance.motor.powerEfficiencyFactor = 0.9f;
                            break;
                        }
                        else
                        {
                            __instance.motor.powerEfficiencyFactor = 1f;
                            break;
                        }

                    default:
                        return;
                }
                if (QMod.Config.DevMode.Equals(true))
                    {
                    ErrorMessage.AddMessage($"Current Seatruck Energy use Multiplier: {__instance.motor.powerEfficiencyFactor}");
                }
            }
        }
    }

    [Menu("MoreEngineEfficiencyModulesBZ")]
    public class Config : ConfigFile
    {
        [Toggle("Seatruck Base Efficiency Boost", Tooltip = "Makes the Seatruck have a higher base efficiency (for if it feels like it runs out of battery too fast)")]
        public bool SeatruckEfficiencyBoost = false;

        [Toggle("View Seatruck Efficiency Multiplier", Tooltip = "If enabled, shows the multiplier for the Seatruck Energy use.")]
        public bool DevMode = false;
    }

    [QModCore]
    public static class QMod
    {

        internal const string WorkBenchTab = "EngineEfficiencyModUpgrades";
        internal static Config Config { get; } = OptionsPanelHandler.Main.RegisterModOptions<Config>();

        [QModPatch]
        public static void Patch()
        {
            SMLHelper.V2.Handlers.CraftTreeHandler.AddCraftingNode(CraftTree.Type.SeamothUpgrades, TechType.VehiclePowerUpgradeModule, new string[] { "ExosuitUpgrades" });
            SMLHelper.V2.Handlers.CraftTreeHandler.AddCraftingNode(CraftTree.Type.SeamothUpgrades, TechType.SeaTruckUpgradeEnergyEfficiency, new string[] { "SeatruckUpgrades" });
            SMLHelper.V2.Handlers.CraftTreeHandler.AddTabNode(CraftTree.Type.Workbench, WorkBenchTab, "Engine Efficiency Modules", SpriteManager.Get(TechType.VehiclePowerUpgradeModule));
            var assembly = Assembly.GetExecutingAssembly();
            var modName = ($"AkariTheSloth_{assembly.GetName().Name}");
            Logger.Log(Logger.Level.Info, $"Patching {modName}");
            Harmony harmony = new Harmony(modName);
            harmony.PatchAll(assembly);
            Logger.Log(Logger.Level.Info, "Patched successfully!");
            new VehiclePowerUpgradeModuleMK2().Patch();
            new VehiclePowerUpgradeModuleMK3().Patch();
        }
        
    }
}