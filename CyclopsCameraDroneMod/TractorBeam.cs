using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CyclopsCameraDroneMod
{
    public static class TractorBeam
    {
        public static float maxDistance = 50f;
        public static float radius = 0.8f;
        public static float pickupRange = 2f;
        public static float massLimit = 2500f;
        public static float force = 10f;
        public static float maxForce = 12000f;
        public static float dampeningStrength = 5f;
        public static float lineWidth = 1f;
        public static RaycastHit[] tractorBeamHit = new RaycastHit[32];
        public static List<Rigidbody> hitRigidbodies = new List<Rigidbody>();
        public static LayerMask tractorBeamLayerMask = -1;
        
        public static Type[] whitelistedComponents = new Type[] { typeof(Creature), typeof(BreakableResource), };

        public static void Reset()
        {
            hitRigidbodies.Clear();
        }

        public static void Attract(Transform cameraTransform, Collider collider)
        {
            var rb = collider.attachedRigidbody;
            if (rb == null || hitRigidbodies.Contains(rb))
            {
                return;
            }
            if (rb.mass > massLimit)
            {
                return;
            }
            var pickupable = rb.gameObject.GetComponent<Pickupable>();
            if (rb.isKinematic)
            {
                if (pickupable == null || !pickupable.isPickupable)
                {
                    return;
                }
            }
            if (pickupable != null && !pickupable.isPickupable)
            {
                return;
            }
            bool hasComponentInWhitelist = false;
            for (int i = 0; i < whitelistedComponents.Length; i++)
            {
                if (rb.gameObject.GetComponent(whitelistedComponents[i]) != null)
                {
                    hasComponentInWhitelist = true;
                    break;
                }
            }
            if (pickupable == null && hasComponentInWhitelist == false)
            {
                return;
            }
            hitRigidbodies.Add(rb);
            var beamedObject = rb.gameObject.EnsureComponent<TractorBeamedObject>();
            beamedObject.rb = rb;
            beamedObject.targetTransform = cameraTransform;
            beamedObject.Refresh();
        }
    }
}
