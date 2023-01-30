﻿using SEE.GO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.XR;

namespace SEE.Game.Avatars
{
    /// <summary>
    /// Manages a laser beamer for simple pointing gestures for a VR avatar.
    /// </summary>
    /// <remarks>This component is expected to be added to an avatar that
    /// represents a local player in VR, that is, <see cref="gameObject"/>
    /// is referring to a game object that is the root of an avatar.</remarks>
    internal class VRAvatarAimingSystem : MonoBehaviour
    {
        [Tooltip("The laser beam for pointing. If null, one will be created at run-time.")]
        public LaserPointer laser;

        [Tooltip("If true, local interactions control where the avatar is pointing to.")]
        public bool IsLocallyControlled = true;

        /// <summary>
        /// The source from which to start the laser beam. Assigning
        /// a value to this Source will always turn the laser on.
        /// </summary>
        [ShowInInspector]
        public Transform Source
        {
            set
            {
                if (laser == null)
                {
                    laser = gameObject.AddOrGetComponent<LaserPointer>();
                }

                laser.Source = value;
                laser.On = true;
            }
            get { return laser.Source; }
        }

        /// <summary>
        /// The target (end) of the laser beam. This target is assumed to be the AimTarget
        /// of the avatar. This transform will be moved by this component during
        /// <see cref="Update"/>. It is assumed that <see cref="Target"/>
        /// has a <see cref="ClientNetworkTransform"/> attached to it, which will then
        /// automatically broadcast the positions of all corresponding aim targets of all remote
        /// representations of this avatar.
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Adds a <see cref="LaserPointer"/> if necessary and turns it on.
        /// </summary>
        private void Awake()
        {
            laser = gameObject.AddOrGetComponent<LaserPointer>();
            laser.On = true;
        }

        /// <summary>
        /// Retrieves the direction from the pointing device and aims the laser beam
        /// towards this direction. The position of <see cref="Target"/> is set to
        /// the end of the laser beam.
        /// Also distinguishs between local controlled player and remote player.
        /// If it's the remote player, draw method is directly called.
        /// </summary>
        private void Update()
        {
            if (IsLocallyControlled)
            {
                // Draw a line from the AimTransform of the avatar into the direction
                // where the pointing device is pointing to.
                InputDevice handR = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                if (handR.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotR))
                {
                    Vector3 direction = rotR * Vector3.forward;
                    // Move the aim target to the tip of the laser beam.
                    Target.position = laser.PointTowards(direction);
                }
            }
            else
            {
                laser.Draw(Target.position);
            }
        }
    }
}