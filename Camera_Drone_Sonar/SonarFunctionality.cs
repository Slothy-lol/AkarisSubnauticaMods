using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using FMOD;
using CameraDroneUpgrades.API;

namespace Camera_Drone_Sonar.SonarFunctionality
{
    public class Helpers
    {
        public static FMODAsset GetFModAsset(string audioPath)
        {
            FMODAsset asset = ScriptableObject.CreateInstance<FMODAsset>();
            asset.path = audioPath;
            return asset;
        }
    }

    public class SonarFunctionality
    {
        public CameraDroneUpgrade upgrade;

        public void SetUp()
        {
            upgrade.activate += DoShit;
            upgrade.deactivate += DoShit;
            upgrade.key = QMods.QMods.Config.sonarKey;

        }

        private static readonly FMODAsset sonarSound = Helpers.GetFModAsset("event:/sub/seamoth/sonar_loop");
        public bool sonarActive = false;
        public float timeNextPing;

        public void DoShit()
        {
            if (upgrade.camera.energyMixin.charge > 2 && Time.time > timeNextPing)
            {
                upgrade.camera.energyMixin.ConsumeEnergy(2);
                SNCameraRoot.main.SonarPing();
                Utils.PlayFMODAsset(sonarSound, upgrade.camera.transform.position);
                sonarActive = !sonarActive;
                timeNextPing = Time.time + 5; //keep +2, otherwise it would ping twice when you hit the button
            }
            else
            {
                sonarActive = false;
            }
        }
    }
}
