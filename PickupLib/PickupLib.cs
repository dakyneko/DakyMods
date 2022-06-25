using MelonLoader;
using System;
using UnityEngine;
using VRC.SDKBase;
using HarmonyLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;

[assembly:MelonInfo(typeof(PickupLib.PickupLibMod), "PickupLib", "0.0.1", "daky", "https://github.com/dakyneko/DakyMods")]
[assembly:MelonGame("VRChat", "VRChat")]

#nullable enable

namespace PickupLib
{
    internal partial class PickupLibMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PickupListener>();

            // Thanks to DragonPlayer for the help!
            HarmonyInstance.Patch(
                typeof(VRCHandGrasper).GetMethod("Method_Private_Void_TriggerType_0"),
                new HarmonyMethod(AccessTools.Method(typeof(PickupLibMod), nameof(OnPickupTrigger))));
        }

        static private void OnPickupTrigger(VRCHandGrasper __instance, VRC_Trigger.TriggerType param_1)
        {

            var pickup = __instance.Method_Public_VRC_Pickup_0();
            if (pickup == null) return;

            var listener = pickup.GetComponent<PickupListener>();
            if (listener == null) return;

            switch (param_1)
            {
                case VRC_Trigger.TriggerType.OnPickup:  listener.InvokePickup(__instance); break;
                case VRC_Trigger.TriggerType.OnDrop: listener.InvokeDrop(__instance); break;
                case VRC_Trigger.TriggerType.OnPickupUseDown:  listener.InvokePickupUseDown(__instance); break;
                case VRC_Trigger.TriggerType.OnPickupUseUp:  listener.InvokePickupUseUp(__instance); break;

                // DEBUG
                default: MelonLogger.Msg($"OnPickupTrigger unknown event instance={__instance}, type={param_1} pickup={__instance.field_Private_VRC_Pickup_0} player={__instance.field_Private_VRCPlayer_0}"); break;
            };
        }
    }

    // TODO: function to create template pickup

    public class PickupListener : MonoBehaviour
    {

        public PickupListener(IntPtr obj0) : base(obj0) { }

        [method: HideFromIl2Cpp]
        public event Action<VRCHandGrasper>? OnPickup;
        public void InvokePickup(VRCHandGrasper grasper) => OnPickup?.Invoke(grasper);

        [method: HideFromIl2Cpp]
        public event Action<VRCHandGrasper>? OnDrop;
        public void InvokeDrop(VRCHandGrasper grasper) => OnDrop?.Invoke(grasper);

        [method: HideFromIl2Cpp]
        public event Action<VRCHandGrasper>? OnPickupUseDown;
        public void InvokePickupUseDown(VRCHandGrasper grasper) => OnPickupUseDown?.Invoke(grasper);

        [method: HideFromIl2Cpp]
        public event Action<VRCHandGrasper>? OnPickupUseUp;
        public void InvokePickupUseUp(VRCHandGrasper grasper) => OnPickupUseUp?.Invoke(grasper);
    }
}