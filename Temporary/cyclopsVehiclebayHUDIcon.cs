using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using QModManager.API.ModLoading;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;

namespace cyclopsVehiclebayHUDIcon
{
    internal class MySubStatus : CyclopsStatusIcon
    {
        public MySubStatus(SubRoot cyclops) : base(cyclops)
        {

        }
        private VehicleDockingBay dockingBay;
        private VehicleDockingBay DockingBay => dockingBay ?? (dockingBay = Cyclops.GetComponentInChildren<VehicleDockingBay>());

        private SeaMoth seaMoth;
        private Exosuit exosuit;

        private SeaMoth DockedSeamoth => this.DockingBay?.dockedVehicle as SeaMoth;
        private Exosuit DockedExosuit => this.DockingBay?.dockedVehicle as Exosuit;

        public bool SeamothInBay
        {
            get
            {
                seaMoth = this.DockedSeamoth;
                return seaMoth != null;
            }
        }

        public bool ExosuitInBay
        {
            get
            {
                exosuit = this.DockedExosuit;
                return exosuit != null;
            }
        }


        public override bool ShowStatusIcon => ExosuitInBay || SeamothInBay;

        string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public override Sprite StatusSprite()
        {
            if (ExosuitInBay == true)
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "ExosuitIcon.png"));
            }
            if (SeamothInBay == true)
            {
                return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "SeamothIcon.png"));
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
