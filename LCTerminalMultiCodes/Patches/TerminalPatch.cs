using HarmonyLib;
using System.Linq;
using System;
using UnityEngine;

namespace LCTerminalMultiCodes.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch : MonoBehaviour
    {
        private ILookup<string, TerminalAccessibleObject> _wordToTerminalObjects = null;

        public static TerminalPatch Instance { get; private set; }

        public void ResetTerminalObjects()
        {
            Plugin.Logger?.LogDebug($"\"{nameof(TerminalPatch)}\" Resetting {_wordToTerminalObjects?.Count} terminal objects");
            _wordToTerminalObjects = null;
        }

        public void FindTerminalObjects()
        {
            _wordToTerminalObjects = FindObjectsOfType<TerminalAccessibleObject>().ToLookup(x => x.objectCode);
            Plugin.Logger?.LogDebug($"\"{nameof(TerminalPatch)}\" Found {_wordToTerminalObjects?.Count} terminal objects");
        }

        [HarmonyPatch(nameof(Terminal.Start))]
        [HarmonyPostfix]
        private static void StartPostfix(Terminal __instance)
        {
            Instance = __instance.gameObject.AddComponent<TerminalPatch>();
        }

        [HarmonyPatch(nameof(Terminal.ParsePlayerSentence))]
        [HarmonyPostfix]
        private static void ParsePlayerSentencePostfix(ref TerminalNode __result, Terminal __instance)
        {
            string textInput = __instance.screenText.text[^__instance.textAdded..];
            textInput = __instance.RemovePunctuation(textInput);
            string[] words = textInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int firstIndex = __instance.broadcastedCodeThisFrame ? 1 : 0;

            for (int i = firstIndex; i < words.Length; ++i)
            {
                __instance.CallFunctionInAccessibleTerminalObject(words[i]);
            }

            if (__instance.broadcastedCodeThisFrame && firstIndex == 0)
            {
                __instance.PlayBroadcastCodeEffect();
                __result ??= __instance.terminalNodes?.specialNodes?[19];
            }
        }

        [HarmonyPatch(nameof(Terminal.CallFunctionInAccessibleTerminalObject))]
        [HarmonyPrefix]
        private static bool CallFunctionInAccessibleTerminalObjectPrefix(Terminal __instance, string word)
        {
            var objects = Instance?._wordToTerminalObjects?[word];
            if (objects == null)
            {
                Plugin.Logger?.LogDebug($"\"{nameof(TerminalPatch)}\" No valid objects for word \"{word}\", fallback to vanilla method");
                return true;
            }

            bool bFoundValidObject = false;
            foreach (var terminalObject in objects)
            {
                if (terminalObject == null)
                {
                    Plugin.Logger?.LogDebug($"\"{nameof(TerminalPatch)}\" Invalid object for word \"{word}\"");
                    continue;
                }

                __instance.broadcastedCodeThisFrame = true;
                terminalObject.CallFunctionFromTerminal();
                bFoundValidObject = true;
            }

            // If we found a valid object, skip vanilla method.
            return !bFoundValidObject;
        }
    }
}