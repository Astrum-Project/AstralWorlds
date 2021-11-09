using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Astrum
{
    partial class AstralWorlds
    {
        public static class InstanceHistory
        {
            private const string path = "UserData/AstralIH.txt";

            public static List<(string, string)> instances = new List<(string, string)>();

            public static void Initialize()
            {
                AstralWorlds.OnWorldInstance += OnWorldInstance;

                if (File.Exists(path))
                {
                    instances = File.ReadAllLines(path).Select(f => f.Split('#')).Select(f => (f[0], f[1])).ToList();
                    while (instances.Count > 20)
                        instances.RemoveAt(0);
                }
                else File.Create(path);
            }

            public static void OnWorldInstance(VRC.Core.ApiWorldInstance world)
            {
                (string, string) data = ($"{world.world.name}: {world.name}", world.id);
                if (instances.Count > 0 && instances[instances.Count - 1] == data)
                    return;
                instances.Add(data);
                while (instances.Count > 20)
                    instances.RemoveAt(0);
                File.WriteAllLines(path, instances.Select(f => $"{f.Item1}#{f.Item2}"));
            }
        }
    }
}
