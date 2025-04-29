using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SimpleTrainingExpanded
{
    public class CompSTETraining : ThingComp
    {
        public int trainingTypeIndex = 0;
        public bool isAutoChangeTrainingType;
        public List<Pawn> usingPawns = new List<Pawn>();
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
                    Graphic graphicSub = graphicData.Graphic.GetColoredVersion(parent.Graphic.Shader, parent.DrawColor, parent.DrawColorTwo);
                    graphicSub.Draw(parent.DrawPos + Vector3.up * 0.05f, parent.Rotation, parent);
                }
            }
        }

        public void PawnStartJob(Pawn pawn)
        {
            usingPawns.Add(pawn);
        }

        public void PawnEndJob(Pawn pawn)
        {
            int id = usingPawns.IndexOf(pawn);
            if (id < 0)
            {
                Log.Warning($"Attempted to remove {pawn.LabelCap} from {parent.LabelCap}, while it not working on it.");
                return;
            }
            usingPawns.Remove(pawn);
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
                                floatMenuOption.Label += "SimpleTrainingExpanded.Training.ChangeSkill.Current".Translate();
                            }
                            floatMenuOptions.Add(floatMenuOption);
                        }
                        Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                    },
                    defaultLabel = "SimpleTrainingExpanded.Training.ChangeSkill.Label".Translate(),
                    defaultDesc = "SimpleTrainingExpanded.Training.ChangeSkill.Desc".Translate(CurrentTrainingType().jobDef.joySkill.LabelCap)
                };
                yield return new Command_Toggle
                {
                    defaultLabel = "SimpleTrainingExpanded.Training.AutoChangeSkill.Label".Translate(),
                    defaultDesc = "SimpleTrainingExpanded.Training.AutoChangeSkill.Desc".Translate(),
                    isActive = () => isAutoChangeTrainingType,
                    toggleAction = delegate
                    {
                        isAutoChangeTrainingType = !isAutoChangeTrainingType;
                    },
                    activateSound = SoundDefOf.Tick_Tiny,
                    hotKey = KeyBindingDefOf.Misc5
                };
            }
        }

        public override string GetDescriptionPart()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("SimpleTrainingExpanded.Training.DescriptionPart".Translate());
            if (Props.trainingSkillDefs.Count > 1)
            {
                stringBuilder.AppendLine();
                for (int i = 0; i < Props.trainingSkillDefs.Count; i++)
                {
                    stringBuilder.AppendLine($"- {Props.trainingSkillDefs[i].LabelCap}");
                }
            }
            else
            {
                stringBuilder.Append($" {Props.trainingSkillDefs.FirstOrDefault()?.LabelCap ?? "---"}");
            }
            return stringBuilder.ToString();
        }

        public override string CompInspectStringExtra()
        {
            return "SimpleTrainingExpanded.Training.CurrentSkill".Translate(CurrentTrainingType().jobDef.joySkill.LabelCap);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref trainingTypeIndex, "trainingTypeIndex", 0);
            Scribe_Values.Look(ref isAutoChangeTrainingType, "isAutoChangeTrainingType", false);
            Scribe_Collections.Look(ref usingPawns, "usingPawns", LookMode.Reference);
        }
    }
}
