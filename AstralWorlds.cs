global using Logger = Astrum.AstralCore.Logger;
using MelonLoader;
using System;
using VRC.Core;

[assembly: MelonInfo(typeof(Astrum.AstralWorlds), "AstralWorlds", "0.2.1", downloadLink: "github.com/Astrum-Project/AstralWorlds")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public partial class AstralWorlds : MelonMod
    {
        public static event Action<ApiWorldInstance> OnWorldLoaded = new(_ => { });

        public override void OnApplicationStart()
        {
            Utils.Initialize();
            InstanceHistory.Initialize();
            WorldMods.Initialize();
        }

        public override void OnSceneWasLoaded(int index, string name)
        {
            if (index != -1) return;

            MelonCoroutines.Start(GetWorldInstance());
            MelonCoroutines.Start(WorldMods.WaitForLocalLoad(name));
        }

        public override void OnSceneWasUnloaded(int index, string _)
        {
            if (index == -1) 
                WorldMods.mods = new object[0] { };
        }

        private static System.Collections.IEnumerator GetWorldInstance()
        {
            ApiWorldInstance instance;
            while ((instance = Utils.GetInstance()) == null) 
                yield return null;
            OnWorldLoaded(instance);
        }
    }
}
