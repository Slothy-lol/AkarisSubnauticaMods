using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CyclopsCameraDroneMod
{
    public class CyclopsDroneInstance : MonoBehaviour
    {
        private static FMODAsset deployBeaconSound = Helpers.GetFmodAsset("event:/sub/cyclops/load_decoy");
        private static FMODAsset drillLoopSound = Helpers.GetFmodAsset("event:/tools/gravsphere/loop_actual");
        private static FMODAsset mineEmitterLoopSound = Helpers.GetFmodAsset("event:/sub/exo/drill_hit_loop");

        private FMOD_CustomLoopingEmitter drillEmitter;
        private FMOD_CustomLoopingEmitter mineEmitter;

        private void Start()
        {
            drillEmitter = gameObject.AddComponent<FMOD_CustomLoopingEmitter>();
            drillEmitter.SetAsset(drillLoopSound);
            drillEmitter.followParent = true;
            drillEmitter.restartOnPlay = false;

            mineEmitter = gameObject.AddComponent<FMOD_CustomLoopingEmitter>();
            mineEmitter.SetAsset(mineEmitterLoopSound);
            mineEmitter.followParent = true;
            mineEmitter.restartOnPlay = false;
        }

        public void StartDrillSound()
        {
            drillEmitter.Play();
        }

        public void StopDrillSound()
        {
            drillEmitter.Stop();
        }

        public void StartMineSound()
        {
            mineEmitter.Play();
        }

        public void StopMineSound()
        {
            mineEmitter.Stop();
        }

        public void PlayBeaconSound()
        {
            Utils.PlayFMODAsset(deployBeaconSound, transform.position - transform.forward);
        }
    }
}
