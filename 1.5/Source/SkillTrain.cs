using RimWorld;
using System.Xml;
using Verse;

namespace SimpleTrainingExpanded
{
    public class SkillTrain
    {
        public SkillDef skill;

        public float xp;

        public SkillTrain()
        {
        }

        public SkillTrain(SkillDef skill, float xp)
        {
            this.skill = skill;
            this.xp = xp;
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured SkillGain: " + xmlRoot.OuterXml);
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
            xp = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
