using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using QModManager.API.ModLoading;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;

namespace CyclopsCameraDroneMod.HUDIcon
{
    internal class CyclopsCameraDroneHUDIcon : CyclopsStatusIcon
    {
        public CyclopsCameraDroneHUDIcon(SubRoot cyclops) : base(cyclops)
        {

        }

        public override bool ShowStatusIcon => (MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneIndustry.thisTechType) || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneExploration.thisTechType) || MCUServices.CrossMod.HasUpgradeInstalled(Player.main.currentSub, Modules.CyclopsCameraDroneUltimate.thisTechType));
        readonly string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public override Sprite StatusSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CameraDroneModule0.png"));
        }

        public override string StatusText()
        {
            if (Time.time < Main.Main.timeNextUseDrone)
            {
                return $"Time until next use: {Main.Main.timeNextUseDrone - Time.time}.";
            }
            else
                return "Drone Ready.";
        }

        public override Color StatusTextColor()
        {
            if (Time.time < Main.Main.timeNextUseDrone)
            {
                return Color.red;
            }
            else
                return Color.white;
        }
    }
}