using CyclopsCameraDroneMod.Modules;
using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using SMLHelper.V2.Utility;
using System.IO;
using System.Reflection;
using UnityEngine;
using Sprite = Atlas.Sprite;

namespace CyclopsCameraDroneMod.HUDIcon
{
    internal class CyclopsCameraDroneHUDIcon : CyclopsStatusIcon
    {
        private readonly UpgradeHandler droneUltimate;
        private readonly UpgradeHandler droneIndustry;
        private readonly UpgradeHandler droneExploration;
    
        public CyclopsCameraDroneHUDIcon(SubRoot cyclops) : base(cyclops)
        {
            // We're no strangers to love
            // You know the rules
            // And so do I
            droneUltimate = MCUServices.Find.CyclopsUpgradeHandler(cyclops, CyclopsCameraDroneUltimate.thisTechType);
            droneIndustry = MCUServices.Find.CyclopsUpgradeHandler(cyclops, CyclopsCameraDroneIndustry.thisTechType);
            droneExploration = MCUServices.Find.CyclopsUpgradeHandler(cyclops, CyclopsCameraDroneExploration.thisTechType);
        }

        public override bool ShowStatusIcon =>
            droneUltimate.HasUpgrade || 
            droneIndustry.HasUpgrade || 
            droneExploration.HasUpgrade;

        internal static readonly string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");
        internal static readonly Sprite DroneSprite0 = ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule0.png"));
        internal static readonly Sprite DroneSprite1 = ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule1.png"));
        internal static readonly Sprite DroneSprite2 = ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule2.png"));

        public override Sprite StatusSprite()
        {
            if (droneUltimate.HasUpgrade)
            {
                return DroneSprite2;
            }
            else if (droneIndustry.HasUpgrade)
            {
                return DroneSprite1;
            }
            return DroneStatus0;
        }

        public override string StatusText()
        {
            if (Time.time < Main.Main.timeNextUseDrone)
            {
                return $"Time until next use: {(int)(Main.Main.timeNextUseDrone - Time.time)}.";
            }
            else { return "Drone Ready."; }
        }

        public override Color StatusTextColor()
        {
            if (Time.time < Main.Main.timeNextUseDrone)
            {
                return Color.red;
            }
            else { return Color.white; }
        }
    }
}
