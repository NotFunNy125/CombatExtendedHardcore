using CombatExtended;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CombatExtendedHardcore
{
    [HarmonyPatch(typeof(LoadoutPropertiesExtension), "TryGenerateAmmoFor")]
    static class Harmony_LoadoutPropertiesExtension_TryGenerateAmmoFor
    {
        public static bool Prefix(LoadoutPropertiesExtension __instance, ThingWithComps gun, CompInventory inventory, int ammoCount)
        {
            CEH_TryGenerateAmmoFor_Patch.TryGenerateAmmoFor(__instance, gun, inventory, ammoCount);
            return false;
        }
    }
}
