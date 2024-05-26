using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtendedHardcore
{
    [HarmonyPatch(typeof(StockGenerator_MarketValue), "SelectionWeight")]
    public static class Harmony_StockGenerator_MarketValue_SelectionWeight
    {
        public static bool Prefix(ref float __result, ThingDef thingDef)
        {
            if(thingDef.IsWeapon && thingDef.equipmentType == EquipmentType.Primary && !thingDef.weaponTags.NullOrEmpty<string>())
            {
                var countClass = thingDef.GetModExtension<WeaponCategories>();
                if(countClass != null)
                {
                    var selectionWeight = countClass.categories[0].mainCategory.baseChance;
                    __result = selectionWeight;
                    return false;
                }
            } 
            return true;
        }
    }
}
