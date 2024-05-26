using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtendedHardcore
{
    public class PawnWeaponCategories : DefModExtension
    {
        public List<CategoryWithSubCategoriesClass> weaponCategories;
 
        private static List<ThingStuffPair> allWeaponPairs;
        private static List<ThingDef> allWeapons = new List<ThingDef>();
        private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

        public static void Reset()
        {
            // Initialize weapons
            Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty<string>() && !td.weaponTags.Any(wt => wt == "TurretGun");
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

        private List<WeaponCategoryDef> GetAllMainCategories()
        {
            var mainCategories = new List<WeaponCategoryDef>();
            foreach(var selector in weaponCategories)
            {
                mainCategories.Add(selector.mainCategory);
            }
            return mainCategories;
        }

        public void TryGenerateWeaponFor(Pawn pawn, PawnGenerationRequest request)
        {
            workingWeapons.Clear();
            if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0 || !pawn.RaceProps.ToolUser || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                return;
            }

            var validWeapons = new List<ThingStuffPair>();
            GenCollection.TryRandomElementByWeight(GetAllMainCategories(), (WeaponCategoryDef c) => c.baseChance, out var chosenMainCategory);
            var countClassOfMainCategory = weaponCategories.FirstOrDefault(wc => wc.mainCategory.defName == chosenMainCategory.defName);
            if (countClassOfMainCategory.subCategories.Count() != 0)
            {
                GenCollection.TryRandomElement(countClassOfMainCategory.subCategories, out var chosenSubCategory);
                validWeapons = GetWeaponsWithWeaponCategory(chosenMainCategory, chosenSubCategory);
            }
            else
            {
                validWeapons = GetWeaponsWithWeaponCategory(chosenMainCategory);
            }
            float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
            
            for (int i = 0; i < validWeapons.Count; i++)
            {
                ThingStuffPair w2 = validWeapons[i];
                if (!(w2.Price > randomInRange) && (pawn.kindDef.weaponStuffOverride == null || w2.stuff == pawn.kindDef.weaponStuffOverride) && (!w2.thing.IsRangedWeapon || !pawn.WorkTagIsDisabled(WorkTags.Shooting)) && (!(w2.thing.generateAllowChance < 1f) || Rand.ChanceSeeded(w2.thing.generateAllowChance, pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
                {
                    workingWeapons.Add(w2);
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

        private List<ThingStuffPair> GetWeaponsWithWeaponCategory(WeaponCategoryDef mainCategory, WeaponSubCategoryDef subCategory = null)
        {
            var validWeapons = new List<ThingStuffPair>();
            foreach (var weapon in allWeaponPairs)
            {
                var weaponSelector = weapon.thing.GetModExtension<WeaponCategories>();
                if (weaponSelector != null)
                {
                    if (subCategory != null)
                    {
                        if (WeaponHasMainAndSubCategory(weaponSelector.categories, mainCategory, subCategory) && !validWeapons.Contains(weapon))
                        {
                            validWeapons.Add(weapon);
                        }
                    }
                    else
                    {
                        if (WeaponHasMainCategory(weaponSelector.categories, mainCategory) && !validWeapons.Contains(weapon))
                        {
                            validWeapons.Add(weapon);
                        }
                    }
                }
                //else
                //{
                //    Log.Message($"{weapon.thing.defName} doesnt have WeaponCategories");
                //}
            }
            return validWeapons;
        }

        private bool WeaponHasMainAndSubCategory(List<CategoryWithSubCategoriesClass> categoriesClassOfWeapon, WeaponCategoryDef mainCategory, WeaponSubCategoryDef subCategory)
        {
            if (categoriesClassOfWeapon.Any(c => c.mainCategory.defName == mainCategory.defName))
            {
                foreach (var categoryClass in categoriesClassOfWeapon)
                {
                    if(categoryClass.subCategories.Any(sc => sc.defName == subCategory.defName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool WeaponHasMainCategory(List<CategoryWithSubCategoriesClass> categoriesClassOfWeapon, WeaponCategoryDef mainCategory)
        {
            return categoriesClassOfWeapon.Any(c => c.mainCategory.defName == mainCategory.defName);
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

