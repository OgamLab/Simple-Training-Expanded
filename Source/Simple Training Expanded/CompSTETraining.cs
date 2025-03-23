using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace SimpleTrainingExpanded
{
    public class CompSTETraining : ThingComp
    {
        public int trainingTypeIndex = 0;
        public bool isAutoChangeTrainingType;
        public CompProperties_STETraining Props => (CompProperties_STETraining)props;
        public List<SkillDef> trainingSkillDefs
        {
            get
            {
                List<SkillDef> skillDefs = new List<SkillDef>();
                if (isAutoChangeTrainingType)
                {
                    skillDefs = Props.trainingSkillDefs;
                }
                else
                {
                    SkillDef skillDef = CurrentTrainingType().jobDef?.joySkill;
                    if (skillDef != null)
                    {
                        skillDefs.Add(skillDef);
                    }

                }
                return skillDefs;
            }
        }

        public TrainingType CurrentTrainingType()
        {
            if (trainingTypeIndex < 0 && trainingTypeIndex >= Props.trainingTypes.Count)
            {
                trainingTypeIndex = 0;
            }
            return Props.trainingTypes.ElementAtOrDefault(trainingTypeIndex);
        }

        public override void PostDraw()
        {
            if (Props.isTextureChangable)
            {
                GraphicData graphicData = parent.Graphic.data.attachments.ElementAtOrDefault(trainingTypeIndex);
                if (graphicData != null)
                {
                    graphicData.Graphic.Draw(parent.DrawPos + Vector3.up * 0.1f, parent.Rotation, parent);
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Props.trainingTypes.Count > 1)
            {
                yield return new Command_Action
                {
                    action = delegate
                    {
                        List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
                        for (int i = 0; i < Props.trainingTypes.Count; i++)
                        {
                            TrainingType trainingType = Props.trainingTypes[i];
                            FloatMenuOption floatMenuOption = new FloatMenuOption(trainingType.jobDef.joySkill.LabelCap, delegate
                            {
                                trainingTypeIndex = Props.trainingTypes.IndexOf(trainingType);
                            });
                            if (i == trainingTypeIndex)
                            {
                                floatMenuOption.Disabled = true;
                                floatMenuOption.Label += " [Current]";
                            }
                            floatMenuOptions.Add(floatMenuOption);
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "Change Training Skill",
                    defaultDesc = $"Change training facility from {CurrentTrainingType().jobDef.joySkill.LabelCap} to selected skill"
                };
            }
            yield return new Command_Toggle
            {
                defaultLabel = "Auto Change",
                defaultDesc = "Will auto change skill to the fit user preferences",
                isActive = () => isAutoChangeTrainingType,
                toggleAction = delegate
                {
                    isAutoChangeTrainingType = !isAutoChangeTrainingType;
                },
                activateSound = SoundDefOf.Tick_Tiny,
                hotKey = KeyBindingDefOf.Misc5
            };
        }

        public override string CompInspectStringExtra()
        {
            return CurrentTrainingType().jobDef.joySkill.LabelCap;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref trainingTypeIndex, "trainingTypeIndex", 0);
            Scribe_Values.Look(ref isAutoChangeTrainingType, "isAutoChangeTrainingType", false);
        }
    }
}
