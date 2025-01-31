﻿using ChangeDresser.UI.DTO.SelectionWidgetDTOs;
using ChangeDresser.UI.Enums;
using Verse;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RimWorld;
using System.Linq;
using AlienRace;
using static AlienRace.AlienPartGenerator;
using static AlienRace.ThingDef_AlienRace;

namespace ChangeDresser.UI.DTO
{
    class AlienDresserDTO : DresserDTO
    {
        public AlienDresserDTO(Pawn pawn, CurrentEditorEnum currentEditorEnum, IEnumerable<CurrentEditorEnum> editors) :
            base(pawn, currentEditorEnum, editors)
        {
            base.EditorTypeSelectionDto.SetSelectedEditor(currentEditorEnum);
        }

//
        protected override void Initialize()
        {
            AlienComp ac = base.Pawn.TryGetComp<AlienComp>();
            if (ac == null || !(base.Pawn.def is ThingDef_AlienRace ar))
            {
                Log.Error("Failed to get alien race for " + base.Pawn.Name.ToStringShort);
                return;
            }

            AlienSettings raceSettings = ar.alienRace;
            GeneralSettings generalSettings = raceSettings?.generalSettings;

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserAlienSkinColor))
            {
#if ALIEN_DEBUG
                Log.Warning("AlienDresserDTO.initialize - start");
#endif
                if (raceSettings != null)
                {
                    StyleSettings styleSettings = raceSettings.styleSettings[typeof(RimWorld.HairDef)];
                    var c = ac.GetChannel("skin");
                    if (c != null)
                    {
                        base.AlienSkinColorPrimary = new SelectionColorWidgetDTO(c.first);
                        base.AlienSkinColorPrimary.SelectionChangeListener += this.PrimarySkinColorChange;

                        base.AlienSkinColorSecondary = new SelectionColorWidgetDTO(c.second);
                        base.AlienSkinColorPrimary.SelectionChangeListener += this.SecondarySkinColorChange;
                    }

                    if (styleSettings?.hasStyle == true)
                    {
                        base.AlienHairColorPrimary = new HairColorSelectionDTO(this.Pawn.story.HairColor,
                            IOUtil.LoadColorPresets(ColorPresetType.Hair));
                        base.AlienHairColorPrimary.SelectionChangeListener += this.PrimaryHairColorChange;

                        ColorPresetsDTO hairColorPresets = IOUtil.LoadColorPresets(ColorPresetType.Hair);
                        if (GradientHairColorUtil.IsGradientHairAvailable)
                        {
                            if (!GradientHairColorUtil.GetGradientHair(this.Pawn, out bool enabled, out Color color))
                            {
                                enabled = false;
                                color = Color.white;
                            }

                            base.AlienHairColorSecondary =
                                new HairColorSelectionDTO(color, hairColorPresets, enabled);
                            base.AlienHairColorSecondary.SelectionChangeListener += this.GradientHairColorChange;
                        }
                    }
                }
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
            {
                if (raceSettings != null)
                {
                    StyleSettings styleSettings = raceSettings.styleSettings[typeof(RimWorld.HairDef)];
                    base.HasHair = styleSettings?.hasStyle == true;
#if ALIEN_DEBUG
                    Log.Warning("initialize - got hair settings: HasHair = " + base.HasHair);
#endif
                    if (base.HasHair)
                    {
                        List<string> hairTags = styleSettings.styleTags;
                        if (hairTags != null)
                        {
                            IEnumerable<StyleItemDef> hairDefs = from hair in DefDatabase<StyleItemDef>.AllDefs
                                where hair.styleTags.SharesElementWith(hairTags)
                                select hair;
#if ALIEN_DEBUG
                            System.Text.StringBuilder sb = new System.Text.StringBuilder("Hair Defs: ");
                            foreach (HairDef d in hairDefs)
                            {
                                sb.Append(d.defName);
                                sb.Append(", ");
                            }
                            Log.Warning("initialize - " + sb.ToString());
#endif

                            /*if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserHair))
                            {
                                if (hairSettings != null)
                                {
                                    List<string> filter = (List<string>)hairSettings.GetType().GetField("hairTags")?.GetValue(hairSettings);
                                    base.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender, filter);
                                }
                            }*/
                            base.HairStyleSelectionDto = new HairStyleSelectionDTO(this.Pawn.story.hairDef,
                                this.Pawn.gender, (IEnumerable<HairDef>)hairDefs);
                        }
                        else
                        {
                            base.HairStyleSelectionDto =
                                new HairStyleSelectionDTO(this.Pawn.story.hairDef, this.Pawn.gender);
                        }
                    }
                    else
                    {
#if ALIEN_DEBUG
                        Log.Warning("initialize - remove hair editors");
#endif
                        // base.EditorTypeSelectionDto.Remove(CurrentEditorEnum
                            // .ChangeDresserHair); //, CurrentEditorEnum.ChangeDresserAlienHairColor);
#if ALIEN_DEBUG
                        Log.Warning("initialize - hair editors removed");
#endif
                    }
                }
            }

            if (this.EditorTypeSelectionDto.Contains(CurrentEditorEnum.ChangeDresserBody))
            {
                var apg = generalSettings?.alienPartGenerator;
                if (apg != null)
                {
                    List<string> crownTypes = new List<string> { }; // todo: crowntype
                    // if (ac.crownType != null && ac.crownType != "" &&
                    //     crownTypes?.Count > 1)
                    // {
                    this.HeadTypeSelectionDto = new HeadTypeSelectionDTO(this.Pawn.story.headType, this.Pawn.gender,
                        this.Pawn.genes, crownTypes); //todo: crown
                    // }

                    // List<BodyTypeDef> alienbodytypes = apg.alienbodytypes;
                    // if (alienbodytypes != null && alienbodytypes.Count > 1)
                    // {
                    //     this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender,
                    //         alienbodytypes);
                    // }
                    // else
                    // {
                    Log.Warning("No alien body types found. Defaulting to human.");
                    this.BodyTypeSelectionDto = new BodyTypeSelectionDTO(this.Pawn.story.bodyType, this.Pawn.gender,
                        this.Pawn.ageTracker.CurLifeStage);
                    // }
                }

                if (generalSettings.maleGenderProbability > 0f && generalSettings.maleGenderProbability < 1f)
                {
                    base.GenderSelectionDto = new GenderSelectionDTO(base.Pawn.gender);
                    base.GenderSelectionDto.SelectionChangeListener += GenderChange;
                }
#if ALIEN_DEBUG
                Log.Warning("initialize - done");
#endif
            }
        }

        private void PrimarySkinColorChange(object sender)
        {
            var c = base.Pawn.TryGetComp<AlienComp>()?.GetChannel("skin");
            if (c != null)
                c.first = base.AlienSkinColorPrimary.SelectedColor;
        }

        private void SecondarySkinColorChange(object sender)
        {
            var c = base.Pawn.TryGetComp<AlienComp>()?.GetChannel("skin");
            if (c != null)
                c.second = base.AlienSkinColorPrimary.SelectedColor;
        }

        private void PrimaryHairColorChange(object sender)
        {
            var c = base.Pawn.TryGetComp<AlienComp>()?.GetChannel("hair");
            if (c != null)
                c.first = base.AlienHairColorPrimary.SelectedColor;
            // this.Pawn.story.HairColor =
            //     base.HairColorSelectionDto.SelectedColor; //base.AlienHairColorPrimary.SelectedColor;
        }

        private void GradientHairColorChange(object sender)
        {
            var c = base.Pawn.TryGetComp<AlienComp>()?.GetChannel("hair");
            if (c != null)
                c.second = base.AlienHairColorSecondary.SelectedColor;
        }

        /*private void SecondaryHairColorChange(object sender)
        {
            SecondaryHairColorFieldInfo.SetValue(this.alienComp, base.AlienHairColorSecondary.SelectedColor);
        }*/

        private void GenderChange(object sender)
        {
            if (this.Pawn.story.bodyType == BodyTypeDefOf.Male &&
                this.Pawn.gender == Gender.Male)
            {
                this.Pawn.story.bodyType = BodyTypeDefOf.Female;
            }
            else if (this.Pawn.story.bodyType == BodyTypeDefOf.Female &&
                     this.Pawn.gender == Gender.Female)
            {
                this.Pawn.story.bodyType = BodyTypeDefOf.Male;
            }
        }
        // internal override void SetCrownType(object value)
        // {
        //     //AlienPartGenerator apg = (base.Pawn.def as ThingDef_AlienRace)?.alienRace?.generalSettings?.alienPartGenerator;
        //     AlienComp ac = base.Pawn.TryGetComp<AlienComp>();
        //     if (ac != null)
        //     {
        //         var split = value.ToString().Split('/');
        //         if (split.Count() > 0)
        //         {
        //             ac.crownType = split.Last().Replace("Male_", "").Replace("Female_", "");
        //             typeof(Pawn_StoryTracker).GetField("headGraphicPath", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.Pawn.story, value);
        //         }
        //     }
        // } //todo:Alien
    }
}