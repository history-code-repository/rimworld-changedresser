﻿using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ChangeDresser
{
    class BuildingUtil
    {
        public static List<Thing> FindThingsNextTo(Map map, IntVec3 position, int distance)
        {
            int minX = Math.Max(0, position.x - distance);
            int maxX = Math.Min(map.info.Size.x, position.x + distance);
            int minZ = Math.Max(0, position.z - distance);
            int maxZ = Math.Min(map.info.Size.z, position.z + distance);

            List<Thing> list = new List<Thing>();
            for (int x = minX - 1; x <= maxX; ++x)
            {
                for (int z = minZ - 1; z <= maxZ; ++z)
                {
                    foreach (Thing t in map.thingGrid.ThingsAt(new IntVec3(x, position.y, z)))
                    {
                        list.Add(t);
                    }
                }
            }
            return list;
        }

        public static List<T> FindThingsOfTypeNextTo<T>(Map map, IntVec3 position, int distance) where T : Thing
        {
            int minX = Math.Max(0, position.x - distance);
            int maxX = Math.Min(map.info.Size.x, position.x + distance);
            int minZ = Math.Max(0, position.z - distance);
            int maxZ = Math.Min(map.info.Size.z, position.z + distance);

            List<T> list = new List<T>();
            for (int x = minX - 1; x <= maxX; ++x)
            {
                for (int z = minZ - 1; z <= maxZ; ++z)
                {
                    foreach (Thing t in map.thingGrid.ThingsAt(new IntVec3(x, position.y, z)))
                    {
                        if (t.GetType() == typeof(T))
                        {
                            list.Add((T)t);
                        }
                    }
                }
            }
            return list;
        }

        public static bool DropThing(Thing toDrop, Building from, Map map, bool makeForbidden = true)
        {
			try
			{
				return DropThing(toDrop, from.InteractionCell, map, makeForbidden);
			}
			catch (Exception e)
			{
				Log.Warning(
					"ChangeDresser:BuildingUtil.DropApparel\n" +
					e.GetType().Name + " " + e.Message + "\n" +
					e.StackTrace);
			}
			return false;
		}

        public static bool DropThing(Thing toDrop, IntVec3 from, Map map, bool makeForbidden = true)
        {
            try
            {
				if (!toDrop.Spawned)
				{
					GenThing.TryDropAndSetForbidden(toDrop, from, map, ThingPlaceMode.Near, out Thing t, makeForbidden);
					if (!toDrop.Spawned)
					{
						GenPlace.TryPlaceThing(toDrop, from, map, ThingPlaceMode.Near);
					}
				}

				toDrop.Position = from;

                return toDrop.Spawned;
            }
            catch (Exception e)
            {
                Log.Warning(
                    "ChangeDresser:BuildingUtil.DropApparel\n" +
                    e.GetType().Name + " " + e.Message + "\n" +
                    e.StackTrace);
            }
            return false;
        }
    }
}
