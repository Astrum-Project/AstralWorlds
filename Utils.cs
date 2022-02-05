using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.Core;

namespace Astrum
{
    public static class Utils
    {
        public static PropertyInfo m_CurrentWorld;

        public static void Initialize()
        {
            m_CurrentWorld = AppDomain.CurrentDomain.GetAssemblies()
               .First(f => f.GetName().Name == "Assembly-CSharp")
               .GetExportedTypes()
               .Where(x => x.BaseType == typeof(MonoBehaviour))
               .Where(x => x.GetMethod("OnConnectedToMaster") != null)
               .SelectMany(x => x.GetProperties(BindingFlags.Public | BindingFlags.Static))
               .FirstOrDefault(x => x.PropertyType == typeof(ApiWorldInstance));

            if (m_CurrentWorld is null)
                Logger.Warn("Failed to find the RoomManager");
            else Logger.Debug($"RoomManager is {m_CurrentWorld.DeclaringType.Name}");
        }

        public static ApiWorldInstance GetInstance() => (ApiWorldInstance)m_CurrentWorld.GetValue(null);
    }
}
