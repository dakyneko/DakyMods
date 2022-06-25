using MelonLoader;
using System;
using UIExpansionKit.API;
using UnityEngine;
using VRC.SDKBase;
using System.Collections.Generic;
using VRC.Core;
using VRC;
using System.Linq;
using HarmonyLib;
using System.IO;
using VRC.SDK3.Components;
using VRC.UserCamera;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;
using Daky;
using PickupLib;

[assembly:MelonInfo(typeof(CameraStarInstants.CameraStarInstantsMod), "CameraStarInstants", "0.0.1", "daky", "https://github.com/dakyneko/DakyMods")]
[assembly:MelonGame("VRChat", "VRChat")]

namespace CameraStarInstants
{
    using static Dakytils;
    internal partial class CameraStarInstantsMod : MelonMod
    {
        private static CameraStarInstantsMod _modInstance;
        private MelonPreferences_Entry<bool> myInstantsEnabled;

        public override void OnApplicationStart()
        {
            _modInstance = this;

            var category = MelonPreferences.CreateCategory("CameraStarInstants", "Camera★Instants");
            myInstantsEnabled = category.CreateEntry("InstantsEnabled", true, "Spawn picture instants when taking photo with camera");

            // TODO: FIXME: waiting on LagFreeScreenshot PR merge upstream to use LagFreeScreenshots.API.LfsApi.OnScreenshotTexture
            HarmonyInstance.Patch(
                typeof(RenderTexture).GetMethods().Single(it => it.Name == "ResolveAntiAliasedSurface" && it.GetParameters().Length == 0),
                new HarmonyMethod(AccessTools.Method(typeof(CameraStarInstantsMod), nameof(ResolveAntiAliasedSurface))));
        }

        private static void ResolveAntiAliasedSurface(RenderTexture __instance)
        {
            WithCameraViewFinder(vf => _modInstance.OnCameraCapture(__instance, vf));
        }

        private void OnCameraCapture(RenderTexture rtex, Transform vf)
        {

            if (!myInstantsEnabled.Value) return;
            var tex = new Texture2D(rtex.width, rtex.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(rtex, tex);
            var aspectRatio = (float)tex.height / (float)tex.width;

            // TODO: gotta downscale to avoid huge memory usage!
            //var resizedWidth = Math.Min(tex.width, 1920);
            //tex.Resize(resizedWidth, (int)Mathf.Floor(aspectRatio * resizedWidth));
            //tex.Apply();

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            var r = plane.GetComponent<Renderer>();
            var m = new Material(Shader.Find("Unlit/Texture"));
            r.material = m;
            r.material.mainTexture = tex;

            var t = plane.transform;
            t.SetParent(vf, false);
            t.localPosition = 0.35f * Vector3.right;
            t.rotation = Quaternion.LookRotation(Vector3.up);
            t.localScale = new Vector3(0.02f, 0.01f, 0.02f * aspectRatio);

            plane.name = "CameraInstants";
            plane.layer = LayerMask.NameToLayer("InternalUI");
            plane.GetComponent<Collider>().isTrigger = true;

            var body = plane.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            var pickup = plane.AddComponent<VRCPickup>();
            pickup.orientation = VRC_Pickup.PickupOrientation.Grip;
            pickup.AutoHold = VRC_Pickup.AutoHoldMode.Yes;
            pickup.allowManipulationWhenEquipped = true;
            pickup.proximity = 0.1f;

            var listener = plane.AddComponent<PickupListener>();
            listener.OnPickup += (_) => t.SetParent(t.root, true);
            listener.OnPickupUseDown += (_) => GameObject.Destroy(plane);
        }
    }
}