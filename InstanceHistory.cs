using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Astrum
{
    partial class AstralWorlds
    {
        public override void OnGUI()
        {
            if (!Input.GetKey(KeyCode.Tab)) return;

            for (int i = 0; i < InstanceHistory.instances.Count; i++)
                if (GUI.Button(new Rect(1, 1 + 22 * (InstanceHistory.instances.Count - 1 - i), 150, 20), InstanceHistory.instances[i].Item1))
                    VRC.SDKBase.Networking.GoToRoom(InstanceHistory.instances[i].Item2);
        }

        public static class InstanceHistory
        {
            private const string path = "UserData/AstralIH.txt";

            public static List<(string, string)> instances = new();

            public static void Initialize()
            {
                AstralWorlds.OnWorldLoaded += OnWorldLoaded;

                if (!File.Exists(path))
                {
                    File.Create(path);
                    return;
                }

                instances = File.ReadAllLines(path).Select(f => f.Split('#')).Select(f => (f[0], f[1])).ToList();
                while (instances.Count > 20)
                    instances.RemoveAt(0);
            }

            public static void OnWorldLoaded(VRC.Core.ApiWorldInstance world)
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
