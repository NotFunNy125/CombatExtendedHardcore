using HarmonyLib;
using Verse;

namespace CombatExtendedHardcore
{
    public class CombatExtendedHardcore : Mod
    {
        private Harmony harmony;

        public CombatExtendedHardcore(ModContentPack content) : base(content)
        {
            harmony = new Harmony("CombatExtendedHardcore");

            harmony.PatchAll();

            LongEventHandler.QueueLongEvent(PawnWeaponCategories.Reset, "CEH_LongEvent_PawnWeaponCategories", false, null);
        }
    }
}
