using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using QModManager.API.ModLoading;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;

namespace CyclopsVehicleBayHUDIcon
{
    internal class MySubStatus : CyclopsStatusIcon
    {
        public MySubStatus(SubRoot cyclops) : base(cyclops)
        {

        }

        private VehicleDockingBay dockingBay;
        private VehicleDockingBay DockingBay => dockingBay ?? (dockingBay = Cyclops.GetComponentInChildren<VehicleDockingBay>());
        private Vehicle VehicleInBay => Cyclops.GetComponentInChildren<VehicleDockingBay>().GetDockedVehicle();
        public override bool ShowStatusIcon => VehicleInBay is SeaMoth || VehicleInBay is Exosuit;

        string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public override Sprite StatusSprite()
        {
            if (VehicleInBay is Exosuit)
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "ExosuitSprite.png"));
            }
            else if (VehicleInBay is SeaMoth)
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "SeamothSprite.png"));
            }
            else
            {
                return null;
            }

        }

        public override string StatusText()
        {
            string statusText = "Vehicle Docked.";
            return statusText;
        }

        public override Color StatusTextColor()
        {
            return Color.white;
        }
    }

    // Your main patching class must have the QModCore attribute (and must be public)
    [QModCore]
    public static class MyInitializerClass
    {
        // Your patching method must have the QModPatch attribute (and must be public)
        [QModPatch]
        public static void MyInitializationMethod()
        {
            MCUServices.Register.CyclopsStatusIcon<MySubStatus>((SubRoot cyclops) => new MySubStatus(cyclops));
        }
    }
}
