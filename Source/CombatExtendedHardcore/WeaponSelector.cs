using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtendedHardcore
{
    public class WeaponSelector : DefModExtension
    {
        public List<WeaponCategoryDef> weaponCategories;
        public List<WeaponSubCategoryDef> subCategories = new List<WeaponSubCategoryDef>();

        private static List<ThingStuffPair> allWeaponPairs;
        private static List<ThingDef> allWeapons = new List<ThingDef>();
        //private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

        public static void Reset()
        {
            // Initialize weapons
            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>();
            allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == thingDef).Sum((ThingStuffPair pa) => pa.Commonality);
                float num2 = thingDef.generateCommonality / num;
                if (num2 != 1f)
                {
                    allWeapons.Add(thingDef);
                    for (int i = 0; i < allWeaponPairs.Count; i++)
                    {
                        ThingStuffPair thingStuffPair = allWeaponPairs[i];
                        if (thingStuffPair.thing == thingDef)
                        {
                            allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
                        }
                    }
                }
            }
        }

        public void TryGenerateWeaponFor(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0 || !pawn.RaceProps.ToolUser || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return;
            }
            var weaponsWithCategories = new List<ThingStuffPair>();
            GenCollection.TryRandomElementByWeight(weaponCategories, (WeaponCategoryDef c) => c.baseChance, out var chosenWeaponCategory);
            Log.Warning($"Category: {chosenWeaponCategory.defName}");
            if(CategoryHasValidSubCategories(chosenWeaponCategory))
            {
                var subCategory = GetValidSubcategory(chosenWeaponCategory);
                Log.Message($"Subcategory: {subCategory.defName}");
                weaponsWithCategories = GetWeaponsWithWeaponCategory(chosenWeaponCategory, subCategory);
                Log.Message($"Weapons added");
            }
            else
            {
                weaponsWithCategories = GetWeaponsWithWeaponCategory(chosenWeaponCategory);
                Log.Message($"Weapons added");
            }
            float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
            var workingWeapons = new List<ThingStuffPair>();
            for (int i = 0; i < weaponsWithCategories.Count; i++)
            {
                ThingStuffPair w2 = weaponsWithCategories[i];
                if (!(w2.Price > randomInRange) && (pawn.kindDef.weaponStuffOverride == null || w2.stuff == pawn.kindDef.weaponStuffOverride) && (!w2.thing.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) && (!(w2.thing.generateAllowChance < 1f) || Rand.ChanceSeeded(w2.thing.generateAllowChance, pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
                {
                    workingWeapons.Add(w2);
                    Log.Warning($"{w2.thing.defName}");
                    break;
                }
            }
            if (workingWeapons.Count == 0)
            {
                return;
            }

            pawn.equipment.DestroyAllEquipment();
            if (workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => GetCommonality(pawn, w), out var result))
            {
                ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
                PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
                CompEquippable compEquippable = thingWithComps.TryGetComp<CompEquippable>();
                if (compEquippable != null)
                {
                    if (pawn.kindDef.weaponStyleDef != null)
                    {
                        compEquippable.parent.StyleDef = pawn.kindDef.weaponStyleDef;
                    }
                    else if (pawn.Ideo != null)
                    {
                        compEquippable.parent.StyleDef = pawn.Ideo.GetStyleFor(thingWithComps.def);
                    }
                }
                float num = ((request.BiocodeWeaponChance > 0f) ? request.BiocodeWeaponChance : pawn.kindDef.biocodeWeaponChance);
                if (Rand.Value < num)
                {
                    thingWithComps.TryGetComp<CompBiocodable>()?.CodeFor(pawn);
                }
                pawn.equipment.AddEquipment(thingWithComps);
            }
        }

        private bool CategoryHasValidSubCategories(WeaponCategoryDef pawnMainCategory)
        {
            if (pawnMainCategory.subCategories.Count() != 0)
            {
                var remainingCategories = new List<WeaponSubCategoryDef>();
                remainingCategories.AddRange(subCategories);
                foreach (var pawnSubcategory in subCategories)
                {
                    GenCollection.TryRandomElement(remainingCategories, out var chosenSubCategory);
                    if (pawnMainCategory.subCategories.Any(sc => sc.defName == chosenSubCategory.defName) && subCategories.Any(sc => sc.defName == chosenSubCategory.defName))
                    {
                        Log.Message($"Valid Subcategory Found: {chosenSubCategory.defName}");
                        return true;
                    }
                    remainingCategories.RemoveWhere(rc => rc.defName == chosenSubCategory.defName);
                    Log.Message($"Remaining subcategories: {remainingCategories.Count}");
                }
                Log.Message("NO Valid Subcategory Found");
                return false;
            }
            else
            {
                Log.Message("NO Valid Subcategory Found");
                return false;
            }
        }

        //Ist noch nicht richtig
        private WeaponSubCategoryDef GetValidSubcategory(WeaponCategoryDef chosenWeaponCategory)
        {
            GenCollection.TryRandomElement(chosenWeaponCategory.subCategories, out var weaponSubCategory);
            foreach(var weapon in allWeaponPairs)
            {
                var weaponSelector = weapon.thing.GetModExtension<WeaponSelector>();
                if (weaponSelector != null && weaponSelector.subCategories.Count != 0)
                {
                    foreach(var pawnSubcategory in subCategories)
                    {
                        if (WeaponHasMainAndSubCategory(weaponSelector, chosenWeaponCategory, weaponSubCategory) && subCategories.Any(sc => sc.defName == pawnSubcategory.defName))
                        {
                            Log.Message($"{weapon.thing.defName} is Valid");
                            Log.Warning($"Valid Sub-Category: {weaponSubCategory.defName}");
                            return weaponSubCategory;
                        }
                    }
                }
            }
            Log.Warning($"NO Valid Sub-Category found. RECURSION!");
            return GetValidSubcategory(chosenWeaponCategory);
        }

        private List<ThingStuffPair> GetWeaponsWithWeaponCategory(WeaponCategoryDef mainCategory, WeaponSubCategoryDef subCategory = null)
        {
            var weaponsWithChosenCategory = new List<ThingStuffPair>();
            foreach (var weapon in allWeaponPairs)
            {
                var weaponSelector = weapon.thing.GetModExtension<WeaponSelector>();
                if(weaponSelector != null)
                {
                    if (WeaponHasMainAndSubCategory(weaponSelector, mainCategory, subCategory) && !weaponsWithChosenCategory.Contains(weapon))
                    {
                        weaponsWithChosenCategory.Add(weapon);
                        Log.Warning($"{weapon.thing.defName} has MAIN and SUB category");
                    }
                    else if (WeaponHasMainCategory(weaponSelector, mainCategory) && !weaponsWithChosenCategory.Contains(weapon) && subCategory == null)
                    {
                        weaponsWithChosenCategory.Add(weapon);
                        Log.Warning($"{weapon.thing.defName} has MAIN category");
                    }
                }
            }
            return weaponsWithChosenCategory;
        }

        private bool WeaponHasMainAndSubCategory(WeaponSelector weapon, WeaponCategoryDef mainCategory, WeaponSubCategoryDef subCategory)
        {
            return weapon.weaponCategories.Any(wc => wc.defName == mainCategory.defName) && weapon.subCategories.Any(sc => sc.defName == subCategory.defName) && subCategories.Any(sc => sc.defName == subCategory.defName);
        }

        private bool WeaponHasMainCategory(WeaponSelector weapon, WeaponCategoryDef mainCategory)
        {
            return weapon.weaponCategories.Any(wc => wc.defName == mainCategory.defName);
        }

        private float GetCommonality(Pawn pawn, ThingStuffPair pair)
        {
            return pair.Commonality * pair.Price * GetWeaponCommonalityFromIdeo(pawn, pair) * GetWeaponCommonalityFromXenotype(pawn, pair);
        }

        private float GetWeaponCommonalityFromIdeo(Pawn pawn, ThingStuffPair pair)
        {
            if (pawn.Ideo == null)
            {
                return 1f;
            }
            return pawn.Ideo.GetDispositionForWeapon(pair.thing) switch
            {
                IdeoWeaponDisposition.Noble => 100f,
                IdeoWeaponDisposition.Despised => 0.001f,
                _ => 1f,
            };
        }

        private float GetWeaponCommonalityFromXenotype(Pawn pawn, ThingStuffPair pair)
        {
            if (pawn.genes?.Xenotype?.forbiddenWeaponClasses != null && !pair.thing.weaponClasses.NullOrEmpty())
            {
                foreach (WeaponClassDef forbiddenWeaponClass in pawn.genes.Xenotype.forbiddenWeaponClasses)
                {
                    if (pair.thing.weaponClasses.Contains(forbiddenWeaponClass))
                    {
                        return 0f;
                    }
                }
            }
            return 1f;
        }
    }
}

