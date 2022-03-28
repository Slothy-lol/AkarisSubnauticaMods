using System.Collections.Generic;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Reflection;
using Sprite = Atlas.Sprite;
using System.IO;
using UnityEngine;
using RecipeData = SMLHelper.V2.Crafting.TechData;
using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.Upgrades;

namespace CyclopsCameraDroneMod.Modules
{
    public class CyclopsCameraDroneExplorationModule : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneExplorationModule() : base("CyclopsCameraDroneExplModule", "Cyclops Camera Drone Exploration Module", "Allows the use of a camera drone for the Cyclops, equipped with important exploration features.")
        {
            OnFinishedPatching += () =>
            {
                thisTechType = TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.CyclopsFabricator;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => MCUServices.CrossMod.StepsToCyclopsModulesTabInCyclopsFabricator;
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.None;
        protected override Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.MapRoomCamera);
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.Magnetite, 2),
                        new Ingredient(TechType.MapRoomCamera, 1),
                        new Ingredient(TechType.WiringKit, 1),
                        new Ingredient(TechType.Scanner, 1),
                        new Ingredient(TechType.PrecursorIonCrystal, 1)
                    }
                )
            };
        }

        public override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule);
            var obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }

    public class CyclopsCameraDroneDrillModule : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneDrillModule() : base("CyclopsCameraDroneDrillModule", "Cyclops Camera Drone Drill Module", "Allows the use of a camera drone for the Cyclops, with an attached laser drill.")
        {
            OnFinishedPatching += () =>
            {
                thisTechType = TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.ExosuitDrillArmModule;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => MCUServices.CrossMod.StepsToCyclopsModulesTabInCyclopsFabricator;
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.None;
        protected override Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.MapRoomCamera);
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.MapRoomCamera, 1),
                        new Ingredient(TechType.ExosuitDrillArmModule, 1),
                        new Ingredient(TechType.Kyanite, 4),
                        new Ingredient(TechType.PrecursorIonCrystal, 3)

                    }
                )
            };
        }

        public override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule);
            var obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }

    public class CyclopsCameraDroneMaintenanceModule : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneMaintenanceModule() : base("CyclopsCameraDroneMaintenanceModule", "Cyclops Camera Drone Maintenance Module", "Allows the use of a camera drone for the Cyclops, equipped with necessary tools for build and repair.")
        {
            OnFinishedPatching += () =>
            {
                thisTechType = TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.Kyanite;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => MCUServices.CrossMod.StepsToCyclopsModulesTabInCyclopsFabricator;
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.None;
        protected override Sprite GetItemSprite()
        {
            //return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, ".png"));
            return SpriteManager.Get(TechType.MapRoomCamera);
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.MapRoomCamera, 1),
                        new Ingredient(TechType.Builder, 1),
                        new Ingredient(TechType.Welder, 1)
                    }
                )
            };
        }

        public override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(TechType.CyclopsShieldModule);
            var obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }
    internal class CyclopsDroneExplorationUpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneExplorationUpgradeHandler(TechType DroneModule, SubRoot cyclops) : base(DroneModule, cyclops)
        {

        }
    }
    internal class CyclopsDroneDrillUpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneDrillUpgradeHandler(TechType DrillModule, SubRoot cyclops) : base(DrillModule, cyclops)
        {

        }
    }
    internal class CyclopsDroneMaintenanceUpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneMaintenanceUpgradeHandler(TechType DrillModule2, SubRoot cyclops) : base(DrillModule2, cyclops)
        {

        }
    }
}