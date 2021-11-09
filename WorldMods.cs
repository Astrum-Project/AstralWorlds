using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace Astrum
{
    partial class AstralWorlds
    {
        public static class WorldMods
        {
            public static AstralWorldTargetAttribute[] worlds = new AstralWorldTargetAttribute[0] { };
            public static object[] mods;

            public static void Initialize()
            {
                if (Directory.Exists(nameof(AstralWorlds)))
                {
                    worlds = Directory.EnumerateFiles(nameof(AstralWorlds))
                        .Select(f => (f, File.ReadAllBytes(f)))
                        .Select(f =>
                        {
                            try
                            {
                                Assembly asm = Assembly.Load(f.Item2);
                                MelonLogger.Msg($"Loaded {f.f.Substring(13):20} ({SHA256(f.Item2)})");
                                return asm;
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Msg($"Failed to load {f.f}: {ex}");
                                return null;
                            }
                        })
                        .Where(f => f != null)
                        .Select(f => f.GetCustomAttributes<AstralWorldTargetAttribute>())
                        .Aggregate(new AstralWorldTargetAttribute[0], (a, f) => a.Concat(f).ToArray())
                        .ToArray();
                }
                else MelonLogger.Msg("AstralWorlds folder does not exist. No world mods will be loaded.");
            }

            public static System.Collections.IEnumerator WaitForLocalLoad(string name)
            {
                Scene scene = SceneManager.GetSceneByName(name);
                while (!scene.GetRootGameObjects().Any(f => f.name.StartsWith("VRCPlayer[Local]"))) 
                    yield return null;


                mods = worlds.Where(f => f.SceneName == name)
                    .Select(f =>
                    {
                        try
                        {
                            return Activator.CreateInstance(f.Type);
                        }
                        catch (Exception ex)
                        {
                            MelonLogger.Msg($"Failed to load world mod: {ex}");
                            return null;
                        }
                    })
                    .Where(f => f != null)
                    .ToArray();
                MelonLogger.Msg($"Loaded into {name} with {mods.Length} World Mods");
            }
        }
    }
}
