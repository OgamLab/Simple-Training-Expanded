using System.Collections.Generic;
using Verse;

namespace SimpleTrainingExpanded
{
    public class CompProperties_STETraining : CompProperties
    {
        public List<TrainingType> trainingTypes = new List<TrainingType>();
        public int maxSkillLevel = 20;
        public int interactionMode = 0;

        public CompProperties_STETraining()
        {
            compClass = typeof(CompSTETraining);
        }
    }
}
