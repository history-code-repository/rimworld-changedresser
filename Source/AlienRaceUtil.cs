﻿using System;
using System.Collections.Generic;
using System.Reflection;
using AlienRace;
using Verse;

namespace ChangeDresser
{
    class AlienRaceUtil
    {
        private static bool alienRaceEnabled = false;
        private static bool initialized = false;
        private static List<ThingDef> alienRaces = new List<ThingDef>(0);

        #region Get Field/Metho Info

        private static FieldInfo raceSettingsFieldInfo = null;
        private static FieldInfo hairSettingsFieldInfo = null;
        private static FieldInfo generalSettingsFieldInfo = null;

        public static object GetAlienRaceSettings(Pawn pawn)
        {
            if (raceSettingsFieldInfo == null)
            {
                raceSettingsFieldInfo = pawn.def.GetType().GetField("alienRace");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("raceSettingsFieldInfo found: " + (string)((raceSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }

            if (raceSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get raceSettingsFieldInfo", "raceSettingsFieldInfo".GetHashCode());
                return null;
            }

            return raceSettingsFieldInfo.GetValue(pawn.def);
        }

        public static object GetHairSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (raceSettings == null)
            {
                return null;
            }

            if (hairSettingsFieldInfo == null)
            {
                hairSettingsFieldInfo = raceSettings.GetType().GetField("styleSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("hairSettingsFieldInfo found: " + (string)((hairSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }

            if (hairSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get hairSettingsFieldInfo", "hairSettingsFieldInfo".GetHashCode());
                return null;
            }

            return hairSettingsFieldInfo.GetValue(raceSettings);
        }

        public static bool HasHair(Pawn pawn)
        {
            // object styleSettings = GetHairSettings(pawn);
            // if (styleSettings == null)
            // {
            //     return false;
            // }
            // foreach (var enumValue in styleSettings.GetType().GetEnumValues())
            // {
            //     Log.Message(enumValue.ToString());
            // }
            // var fi = styleSettings.GetType().GetField("hasStyle");
            // Log.Message(styleSettings.ToString());
            // if (fi == null)
            // {
            //     Log.ErrorOnce("Unable to get hasHair", "hasStyle".GetHashCode());
            //     return false;
            // }
            // return (bool)fi.GetValue(styleSettings);
            return true;
        }

        public static object GetGeneralSettings(Pawn pawn)
        {
            object raceSettings = GetAlienRaceSettings(pawn);
            if (raceSettings == null)
            {
                return null;
            }

            if (generalSettingsFieldInfo == null)
            {
                generalSettingsFieldInfo = raceSettings.GetType().GetField("generalSettings");
#if ALIEN_DEBUG || DEBUG || REFLECTION_DEBUG
                Log.Warning("generalSettingsFieldInfo found: " + (string)((generalSettingsFieldInfo != null) ? "True" : "False"));
#endif
            }

            if (generalSettingsFieldInfo == null)
            {
                Log.ErrorOnce("Unable to get generalSettings", "generalSettings".GetHashCode());
                return null;
            }

            return generalSettingsFieldInfo.GetValue(raceSettings);
        }

        public static List<string> GetHairTags(Pawn pawn)
        {
            object hairSettings = GetHairSettings(pawn);
            var fi = hairSettings.GetType().GetField("hairTags");
            if (fi == null)
            {
                Log.ErrorOnce("Unable to get hairTags", "hairTags".GetHashCode());
                return null;
            }

            return (List<string>)fi.GetValue(hairSettings);
        }

        private static FieldInfo GetMaleGenderProbabilityFieldInfo(Pawn pawn)
        {
            object generalSettings = GetGeneralSettings(pawn);
            if (generalSettings == null)
                return null;

            return generalSettings.GetType().GetField("maleGenderProbability");
        }

        public static bool HasMaleGenderProbability(Pawn pawn)
        {
            return GetMaleGenderProbabilityFieldInfo(pawn) != null;
        }

        public static float GetMaleGenderProbability(Pawn pawn)
        {
            FieldInfo fi = GetMaleGenderProbabilityFieldInfo(pawn);
            if (fi == null)
            {
                Log.ErrorOnce("Unable to get male gender probability. Setting to 0.5f",
                    "maleGenderProbability".GetHashCode());
                return .5f;
            }

            return (float)fi.GetValue(GetGeneralSettings(pawn));
        }

        #endregion

        public static bool Initiated
        {
            get
            {
                return initialized;
            }
        }

        public static bool Exists
        {
            get
            {
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Aliens Exists?");
#endif
                if (!initialized)
                {
#if ALIEN_DEBUG && DEBUG
                    Log.Warning("Aliens Not Initialized");
#endif
                    foreach (ModContentPack pack in LoadedModManager.RunningMods)
                    {
                        foreach (Assembly assembly in pack.assemblies.loadedAssemblies)
                        {
                            if (assembly.GetName().Name.Equals("AlienRace"))
                            {
                                if (assembly.GetType("AlienRace.ThingDef_AlienRace") != null)
                                {
                                    initialized = true;
                                    alienRaceEnabled = true;
                                    break;
                                }
                            }
                        }

                        if (initialized)
                        {
                            break;
                        }
                    }

                    initialized = true;
                }
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Aliens Exists: " + ((bool)(alienRaceAssembly != null)).ToString());
#endif
                return alienRaceEnabled;
            }
        }

        public static List<ThingDef> AlienRaces
        {
            get
            {
#if ALIEN_DEBUG && DEBUG
                Log.Warning("Begin AlienRaces GET");
#endif
                if (Exists && alienRaces.Count == 0)
                {
                    foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
                    {
                        if (def.GetType().GetField("alienRace") != null)
                        {
                            for (Type type = def.GetType(); type != null; type = type.BaseType)
                            {
#if ALIEN_DEBUG && DEBUG
                                Log.Message("    Alien Race Found: " + def.label);
#endif
                                if (type.Name.EqualsIgnoreCase("ThingDef_AlienRace"))
                                {
                                    alienRaces.Add(def);
                                    break;
                                }
                            }
                        }
                    }
                }
#if ALIEN_DEBUG && DEBUG
                Log.Warning("End AlienRaces GET -- " + ((alienRaces == null) ? "<null>" : alienRaces.Count.ToString()));
#endif
                return alienRaces;
            }
        }

        public static bool IsAlien(Pawn pawn)
        {
            // return false;
            // Log.Message("10");
            if (Exists)
            {
                // Log.Message("11");
                return AlienRaces.Contains(pawn.def);
            }
            return false;
        }
    }
}