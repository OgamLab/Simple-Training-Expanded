using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace SimpleTrainingExpanded
{
    public class WorkGiver_Train : WorkGiver_Scanner
    {

        public override bool Prioritized => true;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.GetAllThings((Thing t) => t.HasComp<CompSTETraining>());
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //ResearchProjectDef project = Find.ResearchManager.GetProject();
            //if (project == null)
            //{
            //    return false;
            //}
            //if (!(t is Building_ResearchBench bench))
            //{
            //    return false;
            //}
            //if (!project.CanBeResearchedAt(bench, ignoreResearchBenchPowerStatus: false))
            //{
            //    return false;
            //}
            CompSTETraining compTraining = (t as ThingWithComps).GetComp<CompSTETraining>();
            if (!pawn.CanReserve(t, compTraining.CurrentTrainingType().jobDef.joyMaxParticipants, ignoreOtherReservations: forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
            {
                return false;
            }
            bool isAnySkillToTrain = false;
            foreach (SkillDef skillDef in compTraining.trainingSkillDefs)
            {
                SkillRecord skillRecord = pawn?.skills?.GetSkill(skillDef);
                if (!(skillRecord == null || skillRecord.TotallyDisabled || skillRecord.GetLevel() >= compTraining.Props.maxSkillLevel || skillRecord.LearningSaturatedToday))
                {
                    isAnySkillToTrain = true;
                    break;
                }
            }
            if (!isAnySkillToTrain)
            {
                return false;
            }
            //if (!new HistoryEvent(HistoryEventDefOf.Researching, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
            //{
            //    return false;
            //}
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompSTETraining compTraining = (t as ThingWithComps).GetComp<CompSTETraining>();
            if (compTraining?.CurrentTrainingType().jobDef == null)
            {
                return null;
            }
            return JobMaker.MakeJob(compTraining.CurrentTrainingType().jobDef, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            Thing thing = t.Thing;
            float priority = 1;
            CompSTETraining compTraining = (thing as ThingWithComps).GetComp<CompSTETraining>();
            priority *= thing.GetStatValue(StatDefOfLocal.STE_TrainGainFactor);
            SkillRecord skillRecord = pawn?.skills?.GetSkill(compTraining.CurrentTrainingType().jobDef.joySkill);
            priority *= (1f + (int)skillRecord.Level) / 10;
            priority *= (1f + (int)skillRecord.passion) / 2;
            return priority;
        }
    }
}
