using System.Collections.Generic;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using System.Reflection;
using Sprite = Atlas.Sprite;
using System.IO;
using UnityEngine;
using RecipeData = SMLHelper.V2.Crafting.TechData;
using SMLHelper.V2.Utility;
using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.Upgrades;

namespace CyclopsCameraDroneMod.Modules
{
    public class CyclopsCameraDrone : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDrone() : base("CyclopsCameraDroneExploration", "Cyclops Camera Drone: Exploration", "Allows the use of an exploration camera drone for the Cyclops. Can scan and place beacons.")
        {
            OnFinishedPatching += () =>
            {
                thisTechType = TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.Cyclops;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => MCUServices.CrossMod.StepsToCyclopsModulesTabInCyclopsFabricator;
        public override float CraftingTime => 3f;
        public override QuickSlotType QuickSlotType => QuickSlotType.None;
        protected override Sprite GetItemSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule0.png"));
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
                        new Ingredient(TechType.WiringKit, 1)
                        
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

    public class CyclopsCameraDroneIndustry : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneIndustry() : base("CyclopsCameraDroneIndustry", "Cyclops Camera Drone: Industry", "Allows the use of an industrial camera drone for the Cyclops, with an attached laser drill. Can be combined and upgraded with exploration drone.")
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
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule1.png"));
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
                        new Ingredient(TechType.ExosuitDrillArmModule, 1),
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

    public class CyclopsCameraDroneUltimate : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneUltimate() : base("CyclopsCameraDroneUltimate", "Cyclops Camera Drone: Ultimate", "Allows the use of an ultimate(tm) camera drone for the Cyclops, which acts as both an exploration and an upgraded industrial drone. Can also teleport short distances.")
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
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule2.png"));
        }

        protected override RecipeData GetBlueprintRecipe()
        {
            return new RecipeData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(CyclopsCameraDroneIndustry.thisTechType, 1),
                        new Ingredient(CyclopsCameraDrone.thisTechType, 1),
                        new Ingredient(TechType.Kyanite, 5),
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
    internal class CyclopsDroneUpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneUpgradeHandler(TechType DroneModule, SubRoot cyclops) : base(DroneModule, cyclops)
        {

        }
    }
    internal class CyclopsDroneDrillUpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneDrillUpgradeHandler(TechType DrillModule, SubRoot cyclops) : base(DrillModule, cyclops)
        {

        }
    }
    internal class CyclopsDroneDrillMK2UpgradeHandler : UpgradeHandler
    {
        public CyclopsDroneDrillMK2UpgradeHandler(TechType DrillModule2, SubRoot cyclops) : base(DrillModule2, cyclops)
        {

        }
    }
}
