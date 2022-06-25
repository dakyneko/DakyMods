using System;
using UnityEngine;
using VRC;
using System.Linq;
using MelonLoader;
using VRC.UserCamera;
using System.Collections.Generic;
using Random = System.Random;
using System.Reflection;
using System.IO;
using UnhollowerRuntimeLib;
using UnhollowerRuntimeLib.XrefScans;

[assembly:MelonGame("VRChat", "VRChat")]
[assembly:MelonInfo(typeof(Daky.DakytilsMod), "Dakytils", "1.0.0", "daky", "https://github.com/dakyneko/DakyMods")]

namespace Daky
{
    internal partial class DakytilsMod : MelonMod
    { }

    public static class Dakytils
    {
        private static readonly LazyNotNull<VRC.UI.PageUserInfo> userInfoInstance = new(() => GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<VRC.UI.PageUserInfo>());
        // TODO: replace those by LazyNotNull
        public static Transform myUserCamera = null;
        public static Transform myUserCameraFinder = null;
        public static Transform myUserCameraParent = null;
        public static MelonLogger.Instance logger = new("Dakytils");

        public static void Msg(string msg) => logger.Msg(msg);

        // not null, useful for GameObjects
        public static T NN<T>(this T v) where T : UnityEngine.Object => v == null ? null : v;

        public static Player GetSelectedUser()
        {
            return GetLaserSelectedUser().NN() ?? GetActiveUserInMenu().NN();
        }

        public static Player GetSelectedUserOrMe()
        {
            return GetSelectedUser() ?? PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_Player_0;
        }
        public static Player GetLaserSelectedUser()
        {
            // TDOO: selection can be invalid, how to check??
            //if (userInfoInstance.Value?.field_Private_APIUser_0?.id != userInfoInstance.Value?.field_Public_MenuController_0?.activeUserId)
            //    return null; // selection isn't valid active anymore
            return userInfoInstance.Value?.field_Public_MenuController_0.activePlayer?._player;
        }
        public static Player GetActiveUserInMenu()
        {
            var pid = userInfoInstance.Value.field_Private_APIUser_0?.id;
            Msg($"GetActiveUserInMenu pid={pid}");
            if (pid == null) return null;
            foreach (var p in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                Msg($"GetActiveUserInMenu scan {p} {p.name} {p?.prop_String_0}");
                if (p?.prop_String_0 == pid)
                    return p;
            }
            return null;
        }

        public static String PlayerToName(VRC.Player p)
            => p?.field_Private_APIUser_0?.displayName;

        public static String PlayerToId(VRC.Player p)
            => p?.field_Private_APIUser_0?.id;

        public static String PlayerToAvatarId(VRC.Player p)
            => p?.prop_ApiAvatar_0?.id;

        public static GameObject PlayerToObject(Player p)
            => p?._vrcplayer?.gameObject;

        public static T[] PlayerComponents<T>(Player p, bool withInactive = false)
        {
            if (p == null) return null;
            return p.transform.Find("ForwardDirection/Avatar").GetComponentsInChildren<T>(withInactive);
        }

        public static Animator PlayerToAnimator(Transform t)
        {
            return t?.Find("ForwardDirection/Avatar")?.GetComponent<Animator>();
        }

        public static Renderer[] PlayerToRenderers(Player p, bool withInactive = false)
        {
            var xs = PlayerComponents<Renderer>(p, withInactive);
            if (!withInactive) // only visible and enabled
                return xs.Where(x => x.gameObject.activeInHierarchy && x.isVisible).ToArray();
            return xs;
        }

        public static List<(string name, float weight)> MeshToBlendshapes(SkinnedMeshRenderer r)
        {
            var m = r?.sharedMesh;
            if (m == null) return new();

            var cnt = (m?.blendShapeCount) ?? 0;
            if (cnt == 0) return new();

            return Enumerable.Range(0, cnt).Select(i => (m.GetBlendShapeName(i), r.GetBlendShapeWeight(i))).ToList();
        }

        public static String[] AvatarParamsInternalVRC = {
            "Viseme", "Voice", "GestureLeft", "GestureLeftWeight", "GestureRight", "GestureRightWeight",
            "TrackingType", "VRMode", "MuteSelf", "Grounded", "AngularY", "Upright", "AFK", "Seated", "InStation",
            "VelocityX", "VelocityY", "VelocityZ", "IsLocal", "AvatarVersion", //"VRCEmote",
            "VRCFaceBlendH", "VRCFaceBlendV",
        };

        public static List<VRC.Playables.AvatarParameter> PlayerToAvatarParams(Player p, bool internalVRC = false) {
            List<VRC.Playables.AvatarParameter> xs = new();
            var apc = p?.transform.Find("AnimationController/PlayableController")?.GetComponent<AvatarPlayableController>();
            if (apc == null) return null;
            foreach (var v in apc.field_Private_Dictionary_2_Int32_AvatarParameter_0?.Values)
                if (AvatarParamsInternalVRC.Contains(v?.prop_String_0) == internalVRC)
                    xs.Add(v);
            return xs;
        }

        public static float V01_to_11(float v) // map (0,1) to (-1, +1)
        {
            return (v * 2) - 1;
        }
        public static Vector3 SetVectorX(Vector3 vec, float v) { vec.x = v; return vec; }
        public static Vector3 SetVectorY(Vector3 vec, float v) { vec.y = v; return vec; }
        public static Vector3 SetVectorZ(Vector3 vec, float v) { vec.z = v; return vec; }
        public static Vector3 VectorX(float v) { return new Vector3(v, 0, 0); }
        public static Vector3 VectorY(float v) { return new Vector3(0, v, 0); }
        public static Vector3 VectorZ(float v) { return new Vector3(0, 0, v); }
        public static void ReparentDelegateTransform(Transform newParent, Transform target)
        {
            var rot = target.rotation;
            var pos = target.position;
            if (target.parent != newParent)
                newParent.parent = target.parent;
            target.parent = newParent;
            target.localRotation = Quaternion.identity;
            target.localPosition = Vector3.zero;
            newParent.rotation = rot;
            newParent.position = pos;
        }

        public static void DeparentRelegateTransform(Transform parent, Transform target)
        {
            target.parent = parent.parent;
            target.localRotation = parent.localRotation;
            target.localPosition = parent.localPosition;
        }

        public static Transform FindInParents(Transform t, string name)
        {
            while (t != null)
            {
                if (t.name == name) return t;
                t = t.parent;
            }
            return null;
        }

        public static UserCameraController GetUserCameraController()
        {
            return UserCameraController.field_Internal_Static_UserCameraController_0;
        }

        public static void WithCameraController(Action<UserCameraController> f)
        {
            var cc = GetUserCameraController();
            if (cc == null) return;
            f(cc);
        }
        public static Transform GetCameraViewFinder()
        {
            if (myUserCameraFinder == null)
            {
                var cc = GetUserCameraController();
                var vf = cc?.transform.Find("ViewFinder");
                myUserCameraFinder = vf;
            }
            return myUserCameraFinder;
        }

        public static void WithCameraViewFinder(Action<Transform> f)
        {
            var vf = GetCameraViewFinder();
            if (vf == null) return;
            f(vf);
        }

        public static Transform GetPhotoCameraTransform()
        {
            if (myUserCamera == null)
                myUserCamera = GameObject.Find("PhotoCamera")?.transform;
            return myUserCamera;
        }

        public static void WithPhotoCameraTransform(Action<Transform> f)
        {
            var c = GetPhotoCameraTransform();
            if (c == null) return;
            f(c);
        }

        public static Camera GetPhotoCamera()
        {
            return GetPhotoCameraTransform()?.GetComponent<Camera>();
        }

        public static void WithPhotoCamera(Action<Camera> f)
        {
            WithPhotoCameraTransform(t =>
            {
                var c = t.GetComponent<Camera>();
                if (c != null)
                    f(c);
            });
        }
        public static void CreateUserCameraParent()
        {
            if (myUserCameraParent == null)
            {
                myUserCameraParent = (new GameObject("UserCameraParent")).transform;
                myUserCameraParent.parent = GetUserCameraController().transform.root;
            }
        }

        private static MethodInfo takeScreenshotMethod = null;
        public static void TakeScreenshot()
        {
            takeScreenshotMethod ??= typeof(UserCameraController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Single(
                    it => it.Name.StartsWith("Method_Private_Void_")
                        && it.GetParameters().Length == 0 && it.ReturnType == typeof(void)
                        && XrefScanner.XrefScan(it).Any(r => r.Type == XrefType.Global && r.ReadAsObject()?.ToString() == "PhotoCapture"));

            WithCameraController(cc => takeScreenshotMethod.Invoke(cc, new object[0]));
        }

        public static Il2CppSystem.Collections.Generic.List<Player> PlayerList()
        {
            var playerManager = PlayerManager.field_Private_Static_PlayerManager_0;
            if (playerManager == null) return null;

            return playerManager.field_Private_List_1_Player_0;
        }

        public static List<T> Il2toList<T>(Il2CppSystem.Collections.Generic.List<T> xs) {
            var ys = new List<T>();
            foreach (var x in xs)
            {
                ys.Add(x);
            }
            return ys;
        }

        public static Lazy<bool> IsInDesktopCache = new(() => !UnityEngine.XR.XRDevice.isPresent || Environment.CommandLine.Contains("--no-vr"));
        public static bool IsInDesktop() => IsInDesktopCache.Value;

        public static Random random = new();
        public static double SampleGaussian(float mean = 0, float stdDev = 1) {
            // thanks https://stackoverflow.com/a/218600
            double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        }

        public static AssetBundle ResourceToBundle(Assembly assembly, string path)
        {
            Msg($"ResourceToBundle {path}");
            using var stream = assembly.GetManifestResourceStream(path);
            using var memStream = new MemoryStream((int)stream.Length);
            stream.CopyTo(memStream);
            var bundle = AssetBundle.LoadFromMemory_Internal(memStream.ToArray(), 0);
            bundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return bundle;
        }

        public static T BundleToObject<T>(AssetBundle bundle, string path, Transform parent) where T : UnityEngine.Object {
            Msg($"BundleToObject {path} <- {bundle} contains?={bundle.Contains(path)}");
            if (!bundle.Contains(path))
            {
                Msg($"BundleToObject said doesn't contain {path}");
                return null;
            }
            var objectFromBundle = bundle.LoadAsset(path, Il2CppType.Of<T>()).Cast<T>();
            Msg($"BundleToObject {path} <- {bundle} got {objectFromBundle} (type {objectFromBundle?.GetType()})");
            if (objectFromBundle == null)
            {
                Msg($"BundleToObject failed null");
                return null;
            }
            var newObject = UnityEngine.Object.Instantiate(objectFromBundle, parent);
            //newObject.SetActive(true);
            newObject.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return newObject;
        }
        public static float[] Vec3Array(Vector3 v) => new[] { v.x, v.y, v.z };
        public static float[] Vec3Array(Vector3? v) => v != null ? Vec3Array(v.Value) : null;
        public static float[] Quat4Array(Quaternion v) => new[] { v.x, v.y, v.z, v.w };
    }

    public class LazyNotNull<T> where T : class // Lazy but for value that can become null, we retry if it does
    {
        public T _value = null;
        public Func<T> getter;
        public List<Action<T>> listeners = new();

        public LazyNotNull(Func<T> f)
        {
            getter = f;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                    _value = getter();
                return _value;
            }
        }

        public bool HasValue {
            get => _value != null;
        }
        public void CacheValue() {
            var _ = Value;
        }
        public T RefreshValue()
        {
            return _value = getter();
        }

        public U WithValue<U> (Func<T,U> f) where U : class
        {
            var v = Value;
            if (v != null)
                return f(v);
            return null;
        }
        public void WithValue (Action<T> f)
        {
            WithValue<object>(new Func<T, object>(v => { f(v); return null; }));
        }

        public void SetValue(T v) => _value = v; // will not call listeners below
        // and it won't work with null-ish value either

        public event Action<T> OnValue
        {
            add
            {
                if (listeners.Count == 0)
                    MelonCoroutines.Start(WaitValue());
                listeners.Add(value);
            }
            remove
            {
                listeners.Remove(value);
            }
        }
        private System.Collections.IEnumerator WaitValue()
        {
            while (Value == null)
                yield return null;

            var v = Value;
            foreach (var l in listeners)
                l(v);
        }
    }
}