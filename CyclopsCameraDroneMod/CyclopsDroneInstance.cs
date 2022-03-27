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
        private static FMODAsset tractorBeamLoopSound = Helpers.GetFmodAsset("event:/sub/rocket/call_lift_loop");

        private FMOD_CustomLoopingEmitter drillEmitter;
        private FMOD_CustomLoopingEmitter mineEmitter;
        private FMOD_CustomLoopingEmitter tractorBeamEmitter;

        private void Start()
        {
            drillEmitter = AddLoopingEmitter(drillLoopSound);
            mineEmitter = AddLoopingEmitter(mineEmitterLoopSound);
            tractorBeamEmitter = AddLoopingEmitter(tractorBeamLoopSound);
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

        public void StartTractorBeamSound()
        {
            tractorBeamEmitter.Play();
        }

        public void StopTractorBeamSound()
        {
            tractorBeamEmitter.Stop();
        }

        public void PlayBeaconSound()
        {
            Utils.PlayFMODAsset(deployBeaconSound, transform.position - transform.forward);
        }
    }
}
