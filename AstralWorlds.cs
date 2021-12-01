using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(Astrum.AstralWorlds), "AstralWorlds", "0.1.1", downloadLink: "github.com/Astrum-Project/AstralWorlds")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("AstralCore")]

namespace Astrum
{
    public partial class AstralWorlds : MelonMod
    {
        public static PropertyInfo m_CurrentWorld = null;
        public static VRC.Core.ApiWorldInstance CurrentInstance;
        public static Action<VRC.Core.ApiWorldInstance> OnWorldInstance = new Action<VRC.Core.ApiWorldInstance>(_ => { });

        private static bool hasCore = false;

        public override void OnApplicationStart()
        {
            m_CurrentWorld = AppDomain.CurrentDomain.GetAssemblies()
                .First(f => f.GetName().Name == "Assembly-CSharp")
                .GetExportedTypes()
                .Where(x => x.BaseType == typeof(MonoBehaviour))
                .Where(x => x.GetMethod("OnConnectedToMaster") != null)
                .SelectMany(x => x.GetProperties(BindingFlags.Public | BindingFlags.Static))
                .FirstOrDefault(x => x.PropertyType == typeof(VRC.Core.ApiWorldInstance));

            if (m_CurrentWorld is null)
                MelonLogger.Warning("Failed to find the RoomManager");
            else MelonLogger.Msg($"RoomManager is {m_CurrentWorld.DeclaringType.Name}");

            hasCore = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == "AstralCore");

            InstanceHistory.Initialize();
            WorldMods.Initialize();
        }

        public override void OnSceneWasLoaded(int index, string name)
        {
            if (index != -1) return;

            MelonCoroutines.Start(GetWorldInstance());
            MelonCoroutines.Start(WorldMods.WaitForLocalLoad(name));
        }

        private System.Collections.IEnumerator GetWorldInstance()
        {
            CurrentInstance = null;

            while (CurrentInstance is null)
            {
                CurrentInstance = (VRC.Core.ApiWorldInstance)m_CurrentWorld.GetValue(null);
                yield return null;
            }

            OnWorldInstance(CurrentInstance);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string _)
        {
            if (buildIndex != -1) return;

            WorldMods.mods = new object[0] { };

            if (hasCore)
                WorldMods.API.Module.commands.Clear();
        }

        public override void OnGUI()
        {
            if (!Input.GetKey(KeyCode.Tab)) return;

            for (int i = 0; i < InstanceHistory.instances.Count; i++)
                if (GUI.Button(new Rect(151, 1 + 22 * (InstanceHistory.instances.Count - 1 - i), 150, 20), InstanceHistory.instances[i].Item1))
                    VRC.SDKBase.Networking.GoToRoom(InstanceHistory.instances[i].Item2);
        }

        public static string SHA256(byte[] bytes, bool caps = false)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(bytes);
            foreach (byte theByte in crypto)
                hash.Append(theByte.ToString($"x2"));
            if (caps) return hash.ToString().ToUpper();
            else return hash.ToString();
        }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AstralWorldTargetAttribute : Attribute
    {
        public Type Type;
        public string WorldID;
        public string SceneName;
    }
}
