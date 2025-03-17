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
            return pawn.Map.listerThings.GetAllThings((Thing t) => t.def.HasModExtension<STE_SimpleTrainingExtension>());
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
            STE_SimpleTrainingExtension simpleTrainingExtension = t.def.GetModExtension<STE_SimpleTrainingExtension>();
            if (simpleTrainingExtension?.jobDef == null)
            {
                return false;
            }
            SkillRecord skillRecord = pawn?.skills?.GetSkill(simpleTrainingExtension.jobDef.joySkill);
            if (skillRecord == null || skillRecord.TotallyDisabled || skillRecord.GetLevel() >= simpleTrainingExtension.maxSkillLevel || skillRecord.LearningSaturatedToday)
            {
                return false;
            }
            if (!pawn.CanReserve(t, simpleTrainingExtension.jobDef.joyMaxParticipants, ignoreOtherReservations: forced) || (t.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(t.InteractionCell, forced)))
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
            STE_SimpleTrainingExtension simpleTrainingExtension = t.def.GetModExtension<STE_SimpleTrainingExtension>();
            if (simpleTrainingExtension?.jobDef == null)
            {
                return null;
            }
            return JobMaker.MakeJob(simpleTrainingExtension.jobDef, t);
        }

        public override float GetPriority(Pawn pawn, TargetInfo t)
        {
            Thing thing = t.Thing;
            float priority = 1;
            STE_SimpleTrainingExtension simpleTrainingExtension = thing.def.GetModExtension<STE_SimpleTrainingExtension>();
            priority *= thing.GetStatValue(StatDefOfLocal.STE_TrainGainFactor);
            SkillRecord skillRecord = pawn?.skills?.GetSkill(simpleTrainingExtension.jobDef.joySkill);
            priority *= (1f + (int)skillRecord.Level) / 10;
            priority *= (1f + (int)skillRecord.passion) / 2;
            return priority;
        }
    }
}
