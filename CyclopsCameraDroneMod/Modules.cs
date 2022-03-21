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

namespace CyclopsCameraDroneMod.Modules
{
    public class CyclopsCameraDroneModule : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneModule() : base("CyclopsCameraDroneModule", "Cyclops Camera Drone Module", "Allows the use of a camera drone for the Cyclops. Can be upgraded to include a drill.")
        {
            OnFinishedPatching += () =>
            {
                CyclopsCameraDroneModule.thisTechType = this.TechType;
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

    public class CyclopsCameraDroneModuleDrill : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneModuleDrill() : base("CyclopsCameraDroneModuleDrill", "Cyclops Camera Drone MK2", "Allows the use of a camera drone for the Cyclops, with an attached drill. Can be upgraded to enhance drill speed and range.")
        {
            OnFinishedPatching += () =>
            {
                CyclopsCameraDroneModuleDrill.thisTechType = this.TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.CyclopsFabricator;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => new[] { "Root" };
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
                        new Ingredient(CyclopsCameraDroneModule.thisTechType, 2),
                        new Ingredient(TechType.ExosuitDrillArmFragment, 1),
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

    public class CyclopsCameraDroneModuleDrillMK2 : Equipable
    {
        public static TechType thisTechType;

        public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public CyclopsCameraDroneModuleDrillMK2() : base("CyclopsCameraDroneModuleDrillMK2", "Cyclops Camera Drone MK3", "Allows the use of a camera drone for the Cyclops, with an upgraded drill.")
        {
            OnFinishedPatching += () =>
            {
                CyclopsCameraDroneModuleDrillMK2.thisTechType = this.TechType;
            };
        }

        public override EquipmentType EquipmentType => EquipmentType.CyclopsModule;
        public override TechType RequiredForUnlock => TechType.CyclopsFabricator;
        public override TechGroup GroupForPDA => TechGroup.Cyclops;
        public override TechCategory CategoryForPDA => TechCategory.CyclopsUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.CyclopsFabricator;
        public override string[] StepsToFabricatorTab => new[] { "Root" };
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
                        new Ingredient(CyclopsCameraDroneModuleDrill.thisTechType, 2),
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
}
