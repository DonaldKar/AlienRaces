﻿namespace AlienRace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using HarmonyLib;
    using JetBrains.Annotations;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.AI;

    [DefOf]
    public static class AlienDefOf
    {
        // ReSharper disable InconsistentNaming
        public static TraitDef Xenophobia;
        public static ThoughtDef XenophobiaVsAlien;
        public static ThingCategoryDef alienCorpseCategory;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_AteAlienMeat;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AteNonAlienFood;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_ButcheredAlien;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_Dating;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_BeginRomance;
        [MayRequireIdeology]
        public static HistoryEventDef HAR_AlienDating_SharedBed;

        [MayRequireIdeology]
        public static HistoryEventDef HAR_Alien_SoldSlave;
        // ReSharper restore InconsistentNaming


    }

    public static class Utilities
    {
        public static bool DifferentRace(ThingDef one, ThingDef two) =>
            one != two                                                                                                && one != null && two != null && one.race.Humanlike && two.race.Humanlike &&
            !(one is ThingDef_AlienRace oneAr && oneAr.alienRace.generalSettings.notXenophobistTowards.Contains(two)) &&
            !(two is ThingDef_AlienRace twoAr && twoAr.alienRace.generalSettings.immuneToXenophobia);

        private static List<AlienPartGenerator.BodyAddon> universalBodyAddons;

        public static List<AlienPartGenerator.BodyAddon> UniversalBodyAddons
        {
            get
            {
                if (universalBodyAddons == null)
                {
                    universalBodyAddons = new List<AlienPartGenerator.BodyAddon>();
                    universalBodyAddons.AddRange(DefDatabase<RaceSettings>.AllDefsListForReading.SelectMany(rs => rs.universalBodyAddons));
                }
                return universalBodyAddons;
            }
        }
    }

    [UsedImplicitly]
    public class ThinkNode_ConditionalIsMemberOfRace : ThinkNode_Conditional
    {
        public List<ThingDef> races;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalIsMemberOfRace obj = (ThinkNode_ConditionalIsMemberOfRace)base.DeepCopy(resolve);
            obj.races = new List<ThingDef>(this.races);
            return obj;
        }

        protected override bool Satisfied(Pawn pawn) => 
            this.races.Contains(pawn.def);
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class LoadDefFromField : Attribute
    {
        private string defName;

        public LoadDefFromField(string defName)
        {
            this.defName = defName;
        }

        public Def GetDef(Type defType) => 
            GenDefDatabase.GetDef(defType, this.defName);
    }

    public class Graphic_Multi_RotationFromData : Graphic_Multi
    {
        public override bool ShouldDrawRotated => 
            this.data?.drawRotated ?? false;
    }

    public static class CachedData
    {
        private static Dictionary<RaceProperties, ThingDef> racePropsToRaceDict = new Dictionary<RaceProperties, ThingDef>();

        public static ThingDef GetRaceFromRaceProps(RaceProperties props)
        {
            if (!racePropsToRaceDict.ContainsKey(props))
                racePropsToRaceDict.Add(props,
                                        new List<ThingDef>(DefDatabase<ThingDef>.AllDefsListForReading).Concat(new List<ThingDef_AlienRace>(DefDatabase<ThingDef_AlienRace>.AllDefsListForReading)).First(predicate: td => td.race == props));

            return racePropsToRaceDict[props];
        }

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allApparelPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnApparelGenerator), "allApparelPairs"));

        public static readonly AccessTools.FieldRef<List<ThingStuffPair>> allWeaponPairs =
            AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(AccessTools.Field(typeof(PawnWeaponGenerator), "allWeaponPairs"));

        public delegate Color SwaddleColor(PawnGraphicSet graphicSet);

        public static readonly SwaddleColor swaddleColor =
            AccessTools.MethodDelegate<SwaddleColor>(AccessTools.Method(typeof(PawnGraphicSet), "SwaddleColor"));

        public delegate void PawnGeneratorPawnRelations(Pawn pawn, ref PawnGenerationRequest request);

        public static readonly PawnGeneratorPawnRelations generatePawnsRelations =
            AccessTools.MethodDelegate<PawnGeneratorPawnRelations>(AccessTools.Method(typeof(PawnGenerator), "GeneratePawnRelations"));

        public delegate void FoodUtilityAddThoughtsFromIdeo(HistoryEventDef eventDef, Pawn ingester, ThingDef foodDef, MeatSourceCategory meatSourceCategory);

        public static readonly FoodUtilityAddThoughtsFromIdeo foodUtilityAddThoughtsFromIdeo =
            AccessTools.MethodDelegate<FoodUtilityAddThoughtsFromIdeo>(AccessTools.Method(typeof(FoodUtility), "AddThoughtsFromIdeo"));

        public static readonly AccessTools.FieldRef<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>> pawnTextureAtlasFrameAssignments =
            AccessTools.FieldRefAccess<PawnTextureAtlas, Dictionary<Pawn, PawnTextureAtlasFrameSet>>("frameAssignments");

        public static readonly AccessTools.FieldRef<List<FoodUtility.ThoughtFromIngesting>> ingestThoughts =
            AccessTools.StaticFieldRefAccess<List<FoodUtility.ThoughtFromIngesting>>(AccessTools.Field(typeof(FoodUtility), "ingestThoughts"));

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, Color> hairColor =
            AccessTools.FieldRefAccess<Pawn_StoryTracker, Color>(AccessTools.Field(typeof(Pawn_StoryTracker), "hairColor"));

        public static readonly AccessTools.FieldRef<Pawn_AgeTracker, Pawn> ageTrackerPawn =
            AccessTools.FieldRefAccess<Pawn_AgeTracker, Pawn>(AccessTools.Field(typeof(Pawn_AgeTracker), "pawn"));

        private static List<HeadTypeDef> defaultHeadTypeDefs;

        public static List<HeadTypeDef> DefaultHeadTypeDefs
        {
            get => defaultHeadTypeDefs.NullOrEmpty() ? 
                       DefaultHeadTypeDefs = DefDatabase<HeadTypeDef>.AllDefsListForReading.Where(hd => Regex.IsMatch(hd.defName, @"(?>Male|Female)_(?>Average|Narrow)(?>Normal|Wide|Pointy)")).ToList() : 
                       defaultHeadTypeDefs;
            set => defaultHeadTypeDefs = value;
        }

        public static readonly AccessTools.FieldRef<Dictionary<Type, MethodInfo>> customDataLoadMethodCacheInfo =
            AccessTools.StaticFieldRefAccess<Dictionary<Type, MethodInfo>>(AccessTools.Field(typeof(DirectXmlToObject), "customDataLoadMethodCache"));

        public delegate Graphic_Multi GetGraphic(GraphicRequest req);

        public static GetGraphic getInnerGraphic =
            AccessTools.MethodDelegate<GetGraphic>(AccessTools.Method(typeof(GraphicDatabase), "GetInner", new []{typeof(GraphicRequest)}, new []{typeof(Graphic_Multi)}));

        public delegate void PawnMethod(Pawn pawn);

        public static readonly PawnMethod generateStartingPossessions =
            AccessTools.MethodDelegate<PawnMethod>(AccessTools.Method(typeof(StartingPawnUtility), "GeneratePossessions"));

        public static readonly AccessTools.FieldRef<Pawn_StoryTracker, Color?> skinColorBase =
            AccessTools.FieldRefAccess<Pawn_StoryTracker, Color?>(AccessTools.Field(typeof(Pawn_StoryTracker), "skinColorBase"));

        public static void GeneBodyAddonPatcher()
        {
            List<AlienPartGenerator.BodyAddon> result= new List<AlienPartGenerator.BodyAddon>();
            foreach (GeneDef gene in (DefDatabase<GeneDef>.AllDefsListForReading))
            {

                if (gene.graphicData == null && !gene.HasModExtension<HARgene>())
                {
                    continue;
                }
                AlienPartGenerator.BodyAddon addon = new AlienPartGenerator.BodyAddon();
                addon.geneRequirement = gene;
                HARgene har = gene.GetModExtension<HARgene>();
                if (gene.graphicData != null && (har==null||har.useAutogeneratedAddon))
                {
                    addon.path = gene.graphicData.graphicPath;
                    addon.paths = gene.graphicData.graphicPaths;
                    if (gene.graphicData.graphicPathFemale != null)
                    {
                        AlienPartGenerator.ExtendedGenderGraphic female = new AlienPartGenerator.ExtendedGenderGraphic();
                        female.path = gene.graphicData.graphicPathFemale;
                        female.gender = Gender.Female;
                        addon.genderGraphics.Add(female);
                    }
                    addon.drawSize = new Vector2(gene.graphicData.drawScale, gene.graphicData.drawScale);
                    addon.drawWithoutPart = true;
                    addon.drawnDesiccated = gene.graphicData.drawWhileDessicated;
                    addon.alignWithHead = gene.graphicData.drawLoc != GeneDrawLoc.Tailbone;
                    addon.layerInvert = false;
                    addon.ColorChannel = gene.graphicData.colorType == GeneColorType.Custom ? "base" : gene.graphicData.colorType.ToString().ToLower();//
                    if (gene.graphicData.skinIsHairColor && addon.ColorChannel.Equals("skin"))
                    {
                        addon.ColorChannel = "hair";
                    }
                    if (gene.graphicData.color != null)
                    {
                        addon.colorOverrideOne = gene.graphicData.color;
                    }
                    addon.colorPostFactor = gene.graphicData.colorRGBPostFactor;
                    addon.ShaderType = gene.graphicData.useSkinShader ? ShaderTypeDefOf.CutoutComplex : ShaderTypeDefOf.Transparent;//
                    if (!gene.graphicData.drawIfFaceCovered)
                    {
                        addon.hiddenUnderApparelFor.Add(BodyPartGroupDefOf.FullHead);
                        addon.hiddenUnderApparelFor.Add(BodyPartGroupDefOf.Eyes);
                    }
                    float tempOffset = 0;
                    switch (gene.graphicData.layer)
                    {
                        case GeneDrawLayer.None:
                            tempOffset = 0;
                            break;
                        case GeneDrawLayer.PostSkin:
                            tempOffset = 0.0260617733f;
                            break;
                        case GeneDrawLayer.PostTattoo:
                            tempOffset = 0.0289575271f;
                            break;
                        case GeneDrawLayer.PostHair:
                            tempOffset = 0.03335328f;
                            break;
                        case GeneDrawLayer.PostHeadgear:
                            tempOffset = 0.03335328f;
                            break;
                        default:
                            tempOffset = 0;
                            break;
                    }
                    if (gene.graphicData.fur != null)
                    {
                        addon.alignWithHead = false;
                        tempOffset += 0.009187258f;
                        foreach (FurCoveredGraphicData fur in gene.graphicData.fur.bodyTypeGraphicPaths)
                        {
                            AlienPartGenerator.ExtendedBodytypeGraphic furbody = new AlienPartGenerator.ExtendedBodytypeGraphic();
                            furbody.bodytype = fur.bodyType;
                            furbody.path = fur.texturePath;
                            addon.bodytypeGraphics.Add(furbody);
                        }
                    }

                    addon.offsets.north.layerOffset = gene.graphicData.DrawOffsetAt(Rot4.North).y + tempOffset;
                    if (gene.graphicData.drawNorthAfterHair)
                    {
                        addon.offsets.north.layerOffset += 0.3f;
                    }
                    if (!gene.graphicData.visibleNorth || gene.graphicData.drawOnEyes)
                    {
                        addon.offsets.north.layerOffset -= 0.3f;
                    }
                    addon.offsets.east.layerOffset = gene.graphicData.DrawOffsetAt(Rot4.East).y + tempOffset;
                    addon.offsets.south.layerOffset = gene.graphicData.DrawOffsetAt(Rot4.South).y + tempOffset;
                    addon.offsets.west.layerOffset = gene.graphicData.DrawOffsetAt(Rot4.West).y + tempOffset;

                    addon.offsets.north.offset = gene.graphicData.DrawOffsetAt(Rot4.North);
                    addon.offsets.east.offset = gene.graphicData.DrawOffsetAt(Rot4.East);
                    addon.offsets.south.offset = gene.graphicData.DrawOffsetAt(Rot4.South);
                    addon.offsets.west.offset = gene.graphicData.DrawOffsetAt(Rot4.West);
                    if (gene.graphicData.narrowCrownHorizontalOffset != 0)
                    {
                        foreach (HeadTypeDef head in (DefDatabase<HeadTypeDef>.AllDefsListForReading))
                        {
                            if (head.narrow)
                            {
                                AlienPartGenerator.HeadTypeOffsets headoffset = new AlienPartGenerator.HeadTypeOffsets();
                                headoffset.headType = head;

                                float narrowCrownHorizontalOffset = gene.graphicData.narrowCrownHorizontalOffset;
                                Vector3 geneLoc = addon.offsets.east.offset;
                                geneLoc += Vector3.right * (0f - narrowCrownHorizontalOffset);
                                geneLoc += Vector3.forward * (0f - narrowCrownHorizontalOffset);
                                headoffset.offset = geneLoc;
                                addon.offsets.east.headTypes.Add(headoffset);
                                addon.offsets.east.portraitHeadTypes.Add(headoffset);


                                Vector3 geneLoc2 = addon.offsets.west.offset;
                                geneLoc2 += Vector3.right * narrowCrownHorizontalOffset;
                                geneLoc2 += Vector3.forward * (0f - narrowCrownHorizontalOffset);
                                headoffset.offset = geneLoc2;
                                addon.offsets.west.headTypes.Add(headoffset);
                                addon.offsets.west.portraitHeadTypes.Add(headoffset);

                            }
                        }
                    }
                }
                
                if (har == null)
                {
                    Utilities.UniversalBodyAddons.Add(addon);
                    continue;
                }
                if (har.useAutogeneratedAddon)
                {
                    if (har.addon.path == null)//
                    {
                        har.addon.path = addon.path;
                    }
                    if (har.addon.paths.NullOrEmpty())//
                    {
                        har.addon.paths = addon.paths;
                    }
                    if (har.addon.bodytypeGraphics.NullOrEmpty())//
                    {
                        har.addon.bodytypeGraphics = addon.bodytypeGraphics;
                    }
                    if (har.addon.genderGraphics.NullOrEmpty())//
                    {
                        har.addon.genderGraphics = addon.genderGraphics;
                    }



                    if (har.addon.offsets.north.layerOffset == 0)
                    {
                        har.addon.offsets.north.layerOffset = addon.offsets.north.layerOffset;
                    }
                    if (har.addon.offsets.south.layerOffset == 0)
                    {
                        har.addon.offsets.south.layerOffset = addon.offsets.south.layerOffset;
                    }
                    if (har.addon.offsets.east.layerOffset == 0)
                    {
                        har.addon.offsets.east.layerOffset = addon.offsets.east.layerOffset;
                    }
                    if (har.addon.offsets.west.layerOffset == 0)
                    {
                        har.addon.offsets.west.layerOffset = addon.offsets.west.layerOffset;
                    }


                    if (har.addon.offsets.north.offset == null)
                    {
                        har.addon.offsets.north.offset = addon.offsets.north.offset;
                    }
                    if (har.addon.offsets.south.offset == null)
                    {
                        har.addon.offsets.south.offset = addon.offsets.south.offset;
                    }
                    if (har.addon.offsets.east.offset == null)
                    {
                        har.addon.offsets.east.offset = addon.offsets.east.offset;
                    }
                    if (har.addon.offsets.west.offset == null)
                    {
                        har.addon.offsets.west.offset = addon.offsets.west.offset;
                    }

                    if (har.addon.offsets.east.headTypes.NullOrEmpty())
                    {
                        har.addon.offsets.east.headTypes = addon.offsets.east.headTypes;
                    }
                    if (har.addon.offsets.east.portraitHeadTypes.NullOrEmpty())
                    {
                        har.addon.offsets.east.portraitHeadTypes = addon.offsets.east.portraitHeadTypes;
                    }

                    if (har.addon.offsets.west.headTypes.NullOrEmpty())
                    {
                        har.addon.offsets.west.headTypes = addon.offsets.west.headTypes;
                    }
                    if (har.addon.offsets.west.portraitHeadTypes.NullOrEmpty())
                    {
                        har.addon.offsets.west.portraitHeadTypes = addon.offsets.west.portraitHeadTypes;
                    }

                    har.addon.layerInvert = addon.layerInvert;//always inverted due to offsets managing this
                    har.addon.drawnDesiccated = addon.drawnDesiccated;
                    har.addon.alignWithHead = addon.alignWithHead;
                    if (har.addon.ColorChannel == "skin")//
                    {
                        har.addon.ColorChannel = addon.ColorChannel;
                    }
                    if (har.addon.colorOverrideOne == null)
                    {
                        har.addon.colorOverrideOne = addon.colorOverrideOne;
                    }
                    if (har.addon.colorPostFactor == 1)
                    {
                        har.addon.colorPostFactor = addon.colorPostFactor;
                    }

                    if (har.addon.hiddenUnderApparelFor.NullOrEmpty())//
                    {
                        har.addon.hiddenUnderApparelFor = addon.hiddenUnderApparelFor;
                    }

                    har.addon.geneRequirement = addon.geneRequirement;//always needs this gene

                    if (har.addon.ShaderType == ShaderTypeDefOf.Cutout)//
                    {
                        har.addon.ShaderType = addon.ShaderType;
                    }
                    if (har.addon.drawSize == Vector2.one)//
                    {
                        har.addon.drawSize = addon.drawSize;
                    }
                    addon.drawWithoutPart = har.addon.drawWithoutPart;// part should link to head or torso if it is a gene, if override is wanted, disable autogene writing
                }

                Utilities.UniversalBodyAddons.Add(har.addon);
                if(!har.addons.NullOrEmpty())
                {
                    Utilities.UniversalBodyAddons.AddRange(har.addons);

                }
            }
            return;
        }

        public class HARgene: DefModExtension
        {
            public bool useAutogeneratedAddon = true;
            public AlienPartGenerator.BodyAddon addon;
            public List<AlienPartGenerator.BodyAddon> addons;
        }
    }
}