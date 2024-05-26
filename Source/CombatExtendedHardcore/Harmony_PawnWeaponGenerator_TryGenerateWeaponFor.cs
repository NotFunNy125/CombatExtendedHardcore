using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtendedHardcore
{
    [HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
    static class Harmony_PawnWeaponGenerator_TryGenerateWeaponFor
    {
        public static bool Prefix(Pawn pawn, PawnGenerationRequest request)
        {
            var weaponSelector = pawn.kindDef.GetModExtension<PawnWeaponCategories>();
            if (weaponSelector == null)
            {
                return true;
            }
            else
            {
                weaponSelector.TryGenerateWeaponFor(pawn, request);
                return false;
            }
        }
    }
}
