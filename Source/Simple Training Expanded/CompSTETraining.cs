using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SimpleTrainingExpanded
{
    public class CompSTETraining : ThingComp
    {
        public int trainingTypeIndex = 0;
        public CompProperties_STETraining Props => (CompProperties_STETraining)props;

        public TrainingType CurrentTrainingType()
        {
            if (trainingTypeIndex < 0 && trainingTypeIndex >= Props.trainingTypes.Count)
            {
                trainingTypeIndex = 0;
            }
            return Props.trainingTypes.ElementAtOrDefault(trainingTypeIndex);
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
        }

        public override string CompInspectStringExtra()
        {
            return CurrentTrainingType().jobDef.joySkill.LabelCap;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref trainingTypeIndex, "trainingTypeIndex", 0);
        }
    }
}
