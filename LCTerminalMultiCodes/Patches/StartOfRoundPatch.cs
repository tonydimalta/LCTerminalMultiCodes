using HarmonyLib;

namespace LCTerminalMultiCodes.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {
        [HarmonyPatch(nameof(StartOfRound.OnShipLandedMiscEvents))]
        [HarmonyPostfix]
        private static void OnShipLandedMiscEventsPostfix(StartOfRound __instance)
        {
            if (__instance.currentLevel == null ||
                __instance.currentLevel.name == "CompanyBuildingLevel" ||
                __instance.currentLevel.riskLevel == "Safe")
            {
                return;
            }

            TerminalPatch.Instance?.FindTerminalObjects();
        }

        [HarmonyPatch(nameof(StartOfRound.ShipHasLeft))]
        [HarmonyPostfix]
        private static void ShipHasLeftPostfix()
        {
            TerminalPatch.Instance?.ResetTerminalObjects();
        }
    }
}