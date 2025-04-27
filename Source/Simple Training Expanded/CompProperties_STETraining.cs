using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SimpleTrainingExpanded
{
    public class CompProperties_STETraining : CompProperties
    {
        public List<TrainingType> trainingTypes = new List<TrainingType>();
        public int maxSkillLevel = 20;
        public int interactionMode = 0;
        public bool isTextureChangable;
        public int fleckInterval = 350;
        public FleckDef fleckDef;
        public SoundDef soundCast;
        public float velocitySpeed = 3.8f;
        public int walkInterval = 60;
        public bool isWalkRandomly;
        public List<SkillDef> trainingSkillDefs
        {
            get
            {
                List<SkillDef> skillDefs = new List<SkillDef>();
                foreach (TrainingType trainingType in trainingTypes)
                {
                    SkillDef skillDef = trainingType.jobDef?.joySkill;
                    if (skillDef != null)
                    {
                        skillDefs.Add(skillDef);
                    }
                }
                return skillDefs;
            }
        }

        public CompProperties_STETraining()
        {
            compClass = typeof(CompSTETraining);
        }
    }
}
