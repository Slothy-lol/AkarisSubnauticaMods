using CyclopsCameraDroneMod.QMods;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyclopsCameraDroneMod.droneInstance
{
    public class CyclopsDroneInstance : MonoBehaviour
    {
        private static readonly FMODAsset deployBeaconSound = Helpers.GetFmodAsset("event:/sub/cyclops/load_decoy");
        private static readonly FMODAsset drillLoopSound = Helpers.GetFmodAsset("event:/tools/gravsphere/loop_actual");
        private static readonly FMODAsset mineEmitterLoopSound = Helpers.GetFmodAsset("event:/sub/exo/drill_hit_loop");
        private static readonly FMODAsset tractorBeamLoopSound = Helpers.GetFmodAsset("event:/sub/rocket/call_lift_loop");
        private static readonly FMODAsset sonarSound = Helpers.GetFmodAsset("event:/sub/seamoth/sonar_loop");

        private static readonly FMODAsset repairEndSound = Helpers.GetFmodAsset("event:/tools/welder/weld_end");
        private static readonly FMODAsset repairLoop = Helpers.GetFmodAsset("event:/tools/welder/weld_loop");

        private static readonly FMODAsset scanEndSound = Helpers.GetFmodAsset("event:/tools/scanner/scan_complete");
        private static readonly FMODAsset scanLoop = Helpers.GetFmodAsset("event:/tools/scanner/scan_loop");


        private static readonly FMODAsset hoverSoundLoop = Helpers.GetFmodAsset("event:/sub/seamoth/seamoth_loop");//  event:/sub/exo/jets_loop event:/tools/constructor/bot_1_hover

        //read only because I was annoyed at the messages appearing telling me to. Shouldn't change anything, if it breaks blame Lee not me. Why Lee? Because

        public enum CyclopsDroneType
        {
            Exploration,
            Mining,
            Combo
        }
        public CyclopsDroneType droneType = CyclopsDroneType.Exploration;

        private FMOD_CustomLoopingEmitter drillEmitter;
        private FMOD_CustomLoopingEmitter mineEmitter;
        private FMOD_CustomLoopingEmitter tractorBeamEmitter;

        private FMOD_CustomLoopingEmitter repairEmitter;
        private FMOD_CustomLoopingEmitter scanEmitter;

        private FMOD_CustomLoopingEmitter hoverEmitter;

        //shield shit
        private LiveMixin liveMixin;
        private EnergyMixin energyMixin;

        private MeshRenderer shieldFX;
        private float shieldIntensity;
        private float shieldImpactIntensity;
        private float shieldGoToIntensity;
        private readonly float shieldPowerCost = 10f;
        private readonly float shieldRunningIteration = 5f;

        public bool shieldActive;

        private FMODAsset shield_on_loop;
        private FMOD_CustomEmitter sfx;

        public bool drillSoundPlaying = false;
        public bool mineSoundPlaying = false;
        public bool tractorSoundPlaying = false;
        public bool repairSoundPlaying = false;
        public bool scanSoundPlaying = false;

        public Color vanillaColor;

        private void Start()
        {
            hoverEmitter = AddLoopingEmitter(hoverSoundLoop);

            drillEmitter = AddLoopingEmitter(drillLoopSound);
            mineEmitter = AddLoopingEmitter(mineEmitterLoopSound);
            tractorBeamEmitter = AddLoopingEmitter(tractorBeamLoopSound);

            repairEmitter = AddLoopingEmitter(repairLoop);
            scanEmitter = AddLoopingEmitter(scanLoop);

            liveMixin = GetComponent<LiveMixin>();
            energyMixin = GetComponent<EnergyMixin>();

            shield_on_loop = ScriptableObject.CreateInstance<FMODAsset>();
            shield_on_loop.name = "shield_on_loop";
            shield_on_loop.path = "event:/sub/cyclops/shield_on_loop";
            sfx = gameObject.AddComponent<FMOD_CustomEmitter>();
            sfx.asset = shield_on_loop;
            sfx.followParent = true;

            GameObject CyclopsPrefab = GetRootGameObject("Cyclops", "Cyclops-MainPrefab");

            SubRoot sub = CyclopsPrefab.GetComponent<SubRoot>();

            shieldFX = Instantiate(sub.shieldFX, transform);

            shieldFX.gameObject.SetActive(false);

            Utils.ZeroTransform(shieldFX.transform);
            shieldFX.gameObject.transform.parent = transform;

            shieldFX.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            vanillaColor = uGUI_ScannerIcon.main.icon.backgroundColorNormal;
        }
        private FMOD_CustomLoopingEmitter AddLoopingEmitter(FMODAsset asset)
        {
            var emitter = gameObject.AddComponent<FMOD_CustomLoopingEmitter>();
            emitter.SetAsset(asset);
            emitter.followParent = true;
            emitter.restartOnPlay = false;
            return emitter;
        }

        public void StartDrillSound()
        {
            drillEmitter.Play();
            drillSoundPlaying = true;
        }

        public void StopDrillSound()
        {
            drillEmitter.Stop();
            drillSoundPlaying = false;
        }

        public void StartMineSound()
        {
            mineEmitter.Play();
            mineSoundPlaying = true;
        }

        public void StopMineSound()
        {
            mineEmitter.Stop();
            mineSoundPlaying = false;
        }

        public void StartTractorBeamSound()
        {
            tractorBeamEmitter.Play();
            tractorSoundPlaying = true;
        }

        public void StopTractorBeamSound()
        {
            tractorBeamEmitter.Stop();
            tractorSoundPlaying = false;
        }

        public void PlaySonarSound()
        {
            Utils.PlayFMODAsset(sonarSound, transform.position);
        }
        public IEnumerator DashVFXCoroutine(float duration = 0.25f)
        {
            TeleportScreenFXController fxController = MainCamera.camera.GetComponent<TeleportScreenFXController>();
            //fxController.StartTeleport();
            yield return new WaitForSeconds(duration);
            fxController.StopTeleport();
        }
        public void PlayBeaconSound()
        {
            Utils.PlayFMODAsset(deployBeaconSound, transform.position - transform.forward);
        }
        public void StartRepairSound()
        {
            repairEmitter.Play();
            repairSoundPlaying = true;
        }

        public void StopRepairSound()
        {
            repairEmitter.Stop();
            repairSoundPlaying = false;
        }

        public void PlayRepairEnd()
        {
            Utils.PlayFMODAsset(repairEndSound, transform.position);
        }
        public void StartScanSound()
        {
            scanEmitter.Play();
            scanSoundPlaying = true;
        }

        public void StopScanSound()
        {
            scanEmitter.Stop();
            scanSoundPlaying = false;
        }

        public void PlayScanEndSound()
        {
            Utils.PlayFMODAsset(scanEndSound, transform.position);
        }

        //shield shit
        public GameObject GetRootGameObject(string sceneName, string startsWith)
        {
            Scene scene;

            GameObject[] rootObjects;

            try
            {
                scene = SceneManager.GetSceneByName(sceneName);
            }
            catch
            {
                return null;
            }

            rootObjects = scene.GetRootGameObjects();

            foreach (GameObject gameObject in rootObjects)
            {
                if (gameObject.name.StartsWith(startsWith))
                {
                    return gameObject;
                }
            }
            return null;
        }

        public void ActivateShield()
        {
            liveMixin.shielded = true;
            shieldFX.gameObject.SetActive(true);
            shieldGoToIntensity = 1f;
        }

        public void DeactivateShield()
        {
            liveMixin.shielded = false;
            shieldGoToIntensity = 0f;
        }

        private void Update()
        {
            if (shieldFX != null && shieldFX.gameObject.activeSelf)
            {
                shieldImpactIntensity = Mathf.MoveTowards(shieldImpactIntensity, 0f, Time.deltaTime / 4f);
                shieldIntensity = Mathf.MoveTowards(shieldIntensity, shieldGoToIntensity, Time.deltaTime / 2f);
                shieldFX.material.SetFloat(ShaderPropertyID._Intensity, shieldIntensity);
                shieldFX.material.SetFloat(ShaderPropertyID._ImpactIntensity, shieldImpactIntensity);

                if (Mathf.Approximately(shieldIntensity, 0f) && shieldGoToIntensity == 0f)
                {
                    shieldFX.gameObject.SetActive(false);
                }
            }
        }

        private void ShieldIteration()
        {
            if (energyMixin.charge < energyMixin.capacity * 0.2f)
            {
                ErrorMessage.AddDebug("Warning!\nLow Power!\nEnergy Shield Disabled!");

                StopShield();
            }
            else if (!HandleEnergyDrain(gameObject.GetComponent<MapRoomCamera>(), shieldPowerCost))
            {
                StopShield();
            }
        }

        public static bool HandleEnergyDrain(MapRoomCamera camera, float amount) //here because I couldn't access the one in main
        {
            EnergyMixin mixin = camera.GetComponent<EnergyMixin>();
            if (QMod.Config.energyUsageType.Equals("All"))
            {
                return mixin.ConsumeEnergy(amount);
            }
            else if (QMod.Config.energyUsageType.Equals("Some") || QMod.Config.energyUsageType.Equals("None"))
            {
                return Player.main.currentSub.powerRelay.ConsumeEnergy(amount, out float _);
            }
            return false;
        }

        private void StartShield()
        {
            sfx.Play();

            if (GameModeUtils.RequiresPower())
            {
                InvokeRepeating("ShieldIteration", 0f, shieldRunningIteration);
            }

            ActivateShield();
            shieldActive = true;
        }

        private void StopShield()
        {
            sfx.Stop();
            CancelInvoke("ShieldIteration");
            DeactivateShield();
            shieldActive = false;
        }

        public void ToggleShield()
        {
            if (shieldActive)
            {
                StopShield();
            }
            else
            {
                StartShield();
            }
        }
        public void ScannerIconFunction(float alpha, Color color)
        {
            uGUI_ScannerIcon icon = uGUI_ScannerIcon.main;
            icon.Show();
            //icon.SetAlpha(alpha);
            icon.icon.SetBackgroundColors(color, color, color);
        }
        public void RepairIconFunction(float alpha, Color color)
        {
            uGUI_RepairToolIcon icon = uGUI_RepairToolIcon.main;
            icon.Show();
            icon.SetAlpha(alpha);
            icon.icon.SetBackgroundColors(color, color, color);
        }
        public void StartHoverSound()
        {
            if(!hoverEmitter.playing) hoverEmitter.Play();
        }
        public void StopHoverSound()
        {
            if (hoverEmitter.playing) hoverEmitter.Stop();
        }
    }
}
