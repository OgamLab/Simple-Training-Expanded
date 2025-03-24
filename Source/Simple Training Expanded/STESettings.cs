using Verse;

namespace SimpleTrainingExpanded
{
    public class STESettings : ModSettings
    {
        public bool ShowSkillTrainingProgressBar = false;
        public bool SkillTrainingAfterSaturation = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShowSkillTrainingProgressBar, "ShowSkillTrainingProgressBar", defaultValue: false);
            Scribe_Values.Look(ref SkillTrainingAfterSaturation, "SkillTrainingAfterSaturation", defaultValue: false);
        }
    }
}
