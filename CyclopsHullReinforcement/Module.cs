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

namespace CyclopsHullReinforcement.Module
{
    public class Module
    {
        public class CyclopsHullReinforcement : Equipable
        {
            public static TechType thisTechType;

            public override string AssetsFolder => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

            public CyclopsHullReinforcement() : base("CyclopsArmorPlating", "Cyclops Hull Reincorcement", "Hardens the chassis before collisions, reducing impact damage.")
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
                return SpriteManager.Get(TechType.VehicleArmorPlating);
            }

            protected override RecipeData GetBlueprintRecipe()
            {
                return new RecipeData()
                {
                    craftAmount = 1,
                    Ingredients = new List<Ingredient>(new Ingredient[]
                        {
                            new Ingredient(TechType.VehicleArmorPlating, 1),
                            new Ingredient(TechType.TitaniumIngot, 1),
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
        internal class CyclopsHullReinforcmentUpgradeHandler : UpgradeHandler
        {
            public CyclopsHullReinforcmentUpgradeHandler(SubRoot cyclops) : base(CyclopsHullReinforcement.thisTechType, cyclops)
            {
            }
        }
    }
}
