using System.Collections.Generic;
using Verse;

namespace CombatExtendedHardcore
{
    public class PossibleWeaponCategories : DefModExtension
    {
        private float HighCaliberChance = 0.005f;
        private float MidCaliberChance = 0.02f;
        private float LowCaliberChance = 0.04f;
        private float MedievalRangedChance = 0.1f;
        private float MedievalMeleeChance = 0.305f;
        private float NeolithicMeleeChance = 0.265f;
        private float NeolithicRangedChance = 0.265f;

        public float HighCaliberBonusChance = 0f;
        public float MidCaliberBonusChance = 0f;
        public float LowCaliberBonusChance = 0f;
        public float MedievalRangedBonusChance = 0f;
        public float MedievalMeleeBonusChance = 0f;
        public float NeolithicMeleeBonusChance = 0f;
        public float NeolithicRangedBonusChance = 0f;

        public List<string> weaponCategories;
    }
}
