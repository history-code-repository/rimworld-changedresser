﻿using ChangeDresser;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace MendingChangeDresserPatch
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => "MendAndRecycle".Equals(m.Name)))
            {
                try
                {
                    var harmony = new Harmony("com.mendingchangedresserpatch.rimworld.mod");

                    harmony.PatchAll(Assembly.GetExecutingAssembly());

                    Log.Message(
                        "MendingChangeDresserPatch Harmony Patches:" + Environment.NewLine +
                        "  Postfix:" + Environment.NewLine +
                        "    WorkGiver_DoBill.TryFindBestBillIngredients - Priority Last");
                }
                catch (Exception e)
                {
                    Log.Error("Failed to patch Mending & Recycling." + Environment.NewLine + e.Message);
                }
            }
            else
            {
                Log.Message("MendingChangeDresserPatch did not find MendAndRecycle. Will not load patch.");
            }
        }
    }

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(WorkGiver_DoBill), "TryFindBestBillIngredients")]
    static class Patch_WorkGiver_DoBill_TryFindBestBillIngredients
    {
        static void Postfix(ref bool __result, WorkGiver_DoBill __instance, Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
        {
            if (__result == false &&
				pawn != null && bill != null && bill.recipe != null && 
                bill.Map == pawn.Map &&
                bill.recipe.defName.IndexOf("Apparel") != -1)
            {
                IEnumerable<Building_Dresser> dressers = WorldComp.GetDressers(bill.Map);
                if (dressers == null)
                {
                    Log.Message("MendingChangeDresserPatch failed to retrieve ChangeDressers");
                    return;
                }

                foreach (Building_Dresser dresser in dressers)
                {
                    if ((float)(dresser.Position - billGiver.Position).LengthHorizontalSquared < bill.ingredientSearchRadius * bill.ingredientSearchRadius)
                    {
						if (dresser.TryGetFilteredApparel(bill, bill.ingredientFilter, out List<Apparel> gotten, true, true))
						{
							Apparel a = gotten[0];
							dresser.Remove(a, false);
							if (a.Spawned == false)
							{
								Log.Error("Failed to spawn apparel-to-mend [" + a.Label + "] from dresser [" + dresser.Label + "].");
								__result = false;
								chosen = null;
							}
							else
							{
								__result = true;
								chosen.Add(new ThingCount(a, 1));
							}
							return;
						}
					}
                }
            }
        }
    }
}