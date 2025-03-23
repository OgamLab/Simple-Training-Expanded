using UnityEngine;
using Verse;

namespace SimpleTrainingExpanded
{
    public class STEMod : Mod
    {
        public static STESettings Settings { get; private set; }

        public STEMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<STESettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            Listing_Standard options = new Listing_Standard();
            options.Begin(inRect);
            options.CheckboxLabeled("SimpleTrainingExpanded.Settings.ShowSkillTrainingProgressBar".Translate().RawText, ref Settings.ShowSkillTrainingProgressBar);
            options.End();
        }

        public override string SettingsCategory()
        {
            return "SimpleTrainingExpanded.Settings.Title".Translate().RawText;
        }
    }
}
