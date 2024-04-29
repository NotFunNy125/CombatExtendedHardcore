using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtendedHardcore
{
    public static class CEH_TryGenerateAmmoFor_Patch
    {
        public static void TryGenerateAmmoFor(LoadoutPropertiesExtension instance, ThingWithComps gun, CompInventory inventory, int ammoCount)
        {
            if (ammoCount <= 0)
            {
                return;
            }
            ThingDef thingToAdd;
            int unitCount = 1;  // How many ammo things to add per ammoCount
            int minAmmoSpawned = instance.minAmmoCount; //how many ammo things should the pawn have no matter the magsize
            var compAmmo = gun.TryGetComp<CompAmmoUser>();
            if (compAmmo == null || !compAmmo.UseAmmo)
            {
                if (gun.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.verbClass == typeof(Verb_ShootCEOneUse))
                {
                    thingToAdd = gun.def;   // For one-time use weapons such as grenades, add duplicates instead of ammo
                }
                else
                {
                    return;
                }
            }
            else
            {
                // Generate currently loaded ammo
                thingToAdd = compAmmo.CurrentAmmo;
                // Check if we should use a different magazine ammo count for ammo generation.
                unitCount = GetAmmoInMagazine(compAmmo);
                if (instance.forcedAmmoCategory != null)
                {
                    IEnumerable<AmmoDef> availableAmmo = compAmmo.Props.ammoSet.ammoTypes.Where(a => a.ammo.alwaysHaulable && !a.ammo.menuHidden && (a.ammo.generateAllowChance > 0f || a.ammo.ammoClass == instance.forcedAmmoCategory)).Select(a => a.ammo);
                    if (availableAmmo.Any(x => x.ammoClass == instance.forcedAmmoCategory))
                    {
                        thingToAdd = availableAmmo.Where(x => x.ammoClass == instance.forcedAmmoCategory).FirstOrFallback();
                    }
                }
            }

            var ammoThing = thingToAdd.MadeFromStuff ? ThingMaker.MakeThing(thingToAdd, gun.Stuff) : ThingMaker.MakeThing(thingToAdd);
            //check if total ammo required to load all magazines is less than minimum amount for pawnkind
            ammoThing.stackCount = Math.Max(ammoCount * unitCount, minAmmoSpawned);
            int maxCount;
            if (inventory.CanFitInInventory(ammoThing, out maxCount))
            {
                if (maxCount < ammoThing.stackCount)
                {
                    ammoThing.stackCount = maxCount - (maxCount % unitCount);
                }
                inventory.container.TryAdd(ammoThing);
            }
        }

        private static int GetAmmoInMagazine(CompAmmoUser compAmmo)
        {
            return Rand.Range(5, compAmmo.MagSize + 1);
        }
    }
}
