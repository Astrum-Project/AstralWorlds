using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
                if (!Directory.Exists(nameof(AstralWorlds)))
                {
                    Logger.Warn("AstralWorlds folder does not exist. No world mods will be loaded.");
                    return;
                }

                worlds = Directory.EnumerateFiles(nameof(AstralWorlds))
                    .Select(f => (f, File.ReadAllBytes(f)))
                    .Select(f =>
                    {
                        try
                        {
                            Assembly asm = Assembly.Load(f.Item2);
                            Logger.Info($"Loaded {f.f.Substring(13):20} ({SHA256(f.Item2)})");
                            return asm;
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Failed to load {f.f}: {ex}");
                            return null;
                        }
                    })
                    .Where(f => f != null)
                    .Select(f => f.GetCustomAttributes<AstralWorldTargetAttribute>())
                    .Aggregate(new AstralWorldTargetAttribute[0], (a, f) => a.Concat(f).ToArray())
                    .ToArray();
            }

            public static System.Collections.IEnumerator WaitForLocalLoad(string name)
            {
                while (VRC.SDKBase.Networking.LocalPlayer == null) 
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
                            Logger.Error($"Failed to load world mod: {ex}");
                            return null;
                        }
                    })
                    .Where(f => f != null)
                    .ToArray();
                Logger.Info($"Loaded into {name} with {mods.Length} World Mods");
            }

            public static string SHA256(byte[] bytes)
            {
                var hash = new System.Text.StringBuilder();
                foreach (byte theByte in new System.Security.Cryptography.SHA256Managed().ComputeHash(bytes))
                    hash.Append(theByte.ToString($"x2"));
                return hash.ToString();
            }
        }
    }
}
