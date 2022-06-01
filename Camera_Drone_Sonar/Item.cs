using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using Sprite = Atlas.Sprite;
using UnityEngine;

namespace Camera_Drone_Sonar.Item
{
    public class CameraDroneSonarModule : Craftable
    {
        public static TechType thisTechType;

        public CameraDroneSonarModule() : base("CameraDroneSonar", "Camera Drone Sonar", "Allows drones to use a sonar.")
        {
            OnFinishedPatching += () =>
            {
                thisTechType = TechType;
            };
        }

        public override TechType RequiredForUnlock => TechType.BaseMapRoom;
        public override TechGroup GroupForPDA => TechGroup.MapRoomUpgrades;
        public override TechCategory CategoryForPDA => TechCategory.MapRoomUpgrades;
        public override CraftTree.Type FabricatorType => CraftTree.Type.MapRoom;
        public override string[] StepsToFabricatorTab => new string[] { };
        public override float CraftingTime => 3f;
        protected override Sprite GetItemSprite()
        {
            return SpriteManager.Get(TechType.SeamothSonarModule);
        }

        protected override TechData GetBlueprintRecipe()
        {
            return new TechData()
            {
                craftAmount = 1,
                Ingredients = new List<Ingredient>(new Ingredient[]
                    {
                        new Ingredient(TechType.Magnetite, 2),
                        new Ingredient(TechType.Glass, 1),
                        new Ingredient(TechType.WiringKit, 1)
                    }
                )
            };
        }

        public override GameObject GetGameObject()
        {
            var prefab = CraftData.GetPrefabForTechType(TechType.MapRoomUpgradeScanRange);
            var obj = GameObject.Instantiate(prefab);
            return obj;
        }
    }
}
