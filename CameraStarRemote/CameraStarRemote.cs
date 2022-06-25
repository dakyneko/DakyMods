using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using Daky;
using PickupLib;

[assembly:MelonInfo(typeof(CameraStarRemote.CameraStarRemoteMod), "CameraStarRemote", "0.0.1", "daky", "https://github.com/dakyneko/DakyMods")]
[assembly:MelonGame("VRChat", "VRChat")]

namespace CameraStarRemote
{
    using static Dakytils;
    internal partial class CameraStarRemoteMod : MelonMod
    {
        private VRCPickup myCameraRemote = null;

        public override void OnApplicationStart()
        {
            ExpansionKitApi.OnUiManagerInit += OnUiManagerInit;
        }

        private void AttachCameraToWorld(bool v)
        {
            WithPhotoCameraTransform(t => WithCameraController(cc =>
            {
                var toUnparent = t.parent == cc.transform;
                toUnparent = v; // TODO: forced
                cc.enabled = !toUnparent;
                t.SetParent(toUnparent ? cc.transform.root : cc.transform);
            }));
        }

        private void OnUiManagerInit()
        {
            var qm = ExpansionKitApi.GetExpandedMenu(ExpandedMenu.CameraQuickMenu);
            qm.AddToggleButton("Remote", (v) => WithCameraViewFinder(vf =>
            {
                AttachCameraToWorld(v);
                if (myCameraRemote?.gameObject != null)
                {
                    myCameraRemote.gameObject.active = v;
                    if (!v)
                    {
                        WithPhotoCameraTransform(t => t.transform.position = vf.position);
                    }
                    return;
                }

                var parent = new GameObject("CameraRemoteParent");
                {
                    var t = parent.transform;
                    t.parent = vf;
                    t.localScale = 0.05f * Vector3.one;
                    t.localPosition = new Vector3(-0.35f, 0f, 0f);
                    t.localRotation = new Quaternion(0f, 0.7f, -0.7f, 0f);
                }

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                {
                    var t = cube.transform;
                    t.parent = parent.transform;
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                    t.localScale = Vector3.one;
                }
                cube.name = "CameraRemote";
                cube.layer = LayerMask.NameToLayer("InternalUI");
                cube.GetComponent<Collider>().isTrigger = true;

                var body = cube.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = true;
                var pickup = cube.AddComponent<VRCPickup>();
                pickup.orientation = VRC_Pickup.PickupOrientation.Grip;
                pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
                pickup.allowManipulationWhenEquipped = true;
                pickup.InteractionText = "Magics";
                pickup.proximity = 1;
                pickup.ThrowVelocityBoostMinSpeed = 0;
                pickup.ThrowVelocityBoostScale = 0;

                var listener = cube.AddComponent<PickupListener>();
                listener.OnDrop += (_) =>
                {
                    var t = cube.transform;
                    t.localPosition = Vector3.zero;
                    t.localRotation = Quaternion.identity;
                };
                listener.OnPickupUseDown += (_) => TakeScreenshot();

                myCameraRemote = pickup;
            }), () => myCameraRemote != null);
        }

        public override void OnFixedUpdate()
        {
            UpdateCameraRemote();
        }

        private void UpdateCameraRemote()
        {
            if (myCameraRemote?.gameObject.active != true || GetPhotoCameraTransform() == null) return;
            if (!myCameraRemote.IsHeld) return;

            // follow but at slower scale
            var t = myCameraRemote.transform;
            myUserCamera.localPosition += myUserCamera.localRotation * (0.005f * t.localPosition);
            myUserCamera.localRotation *= Quaternion.Slerp(Quaternion.identity, t.localRotation, 0.005f);
        }
    }
}