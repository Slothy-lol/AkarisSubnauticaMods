using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;
using CyclopsCameraDroneMod.Modules;

namespace CyclopsCameraDroneMod.HUDIcon
{
    internal class CyclopsCameraDroneHUDIcon : CyclopsStatusIcon
    {
        public CyclopsCameraDroneHUDIcon(SubRoot cyclops) : base(cyclops)
        {
            // We're no strangers to love
            // You know the rules
            // And so do I
        }

        public override bool ShowStatusIcon => 
            (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, CyclopsCameraDroneIndustry.thisTechType) 
            || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, CyclopsCameraDroneExploration.thisTechType) 
            || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, CyclopsCameraDroneUltimate.thisTechType));

        readonly string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public override Sprite StatusSprite()
        {
            if (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, CyclopsCameraDroneUltimate.thisTechType))
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule2.png"));
            }
            else if (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, CyclopsCameraDroneIndustry.thisTechType))
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule1.png"));
            }
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule0.png"));
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