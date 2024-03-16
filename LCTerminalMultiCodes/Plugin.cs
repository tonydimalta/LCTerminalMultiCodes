using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace LCTerminalMultiCodes
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    internal class Plugin : BaseUnityPlugin
    {
        private readonly Harmony _harmony = null;

        internal static new ManualLogSource Logger { get; private set; } = null;

        protected Plugin() : base()
        {
            Logger = base.Logger;
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        }

        protected void Awake()
        {
            Logger?.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");

            Logger?.LogDebug("Patching harmony...");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}