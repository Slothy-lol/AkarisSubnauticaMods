﻿using MoreCyclopsUpgrades.API;
using MoreCyclopsUpgrades.API.StatusIcons;
using QModManager.API.ModLoading;
using System.Reflection;
using Sprite = Atlas.Sprite;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;
using CyclopsCameraDroneMod.Main;

namespace cyclopsVehiclebayHUDIcon
{
    internal class MySubStatus : CyclopsStatusIcon
    {
        public MySubStatus(SubRoot cyclops) : base(cyclops)
        {

        }

        public override bool ShowStatusIcon => true;
        string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        public override Sprite StatusSprite()
        {
            return ImageUtils.LoadSpriteFromFile(Path.Combine(AssetsFolder, "CyclopsCameraDrone0.png"));
        }

        public override string StatusText()
        {
            if (Time.time < Main.timeNextUseDrone)
            {
                return $"Time until next use: {Main.timeNextUseDrone - Time.time}.";
            }
            else
                return "Drone Ready.";
        }

        public override Color StatusTextColor()
        {
            if (Time.time < Main.timeNextUseDrone)
            {
                return Color.red;
            }
            else
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