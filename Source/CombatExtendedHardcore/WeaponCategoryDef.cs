using System.Collections.Generic;
using Verse;

namespace CombatExtendedHardcore
{
    public class WeaponCategoryDef : Def 
    {
        public int tier;
        public float baseChance = 1f;
        public List<WeaponSubCategoryDef> subCategories = new List<WeaponSubCategoryDef>();
        public float Chance { get; set; }
    }
}
