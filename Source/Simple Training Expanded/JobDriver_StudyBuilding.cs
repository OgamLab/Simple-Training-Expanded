using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SimpleTrainingExpanded
{
    public class JobDriver_StudyBuilding : JobDriver
    {
        private const TargetIndex BuildingTargetInd = TargetIndex.A;
        private const TargetIndex CellTargetInd = TargetIndex.B;

        public Building building => TargetThingA as Building;

        public CompSTETraining compTraining => compTrainingCached ?? (compTrainingCached = building.GetComp<CompSTETraining>());
        protected CompSTETraining compTrainingCached;

        private Effecter effecter = null;
        public bool isNotForJoy;
        public int interactionMode => interactionModeCached == -1 ? ((!isNotForJoy && compTraining.Props.interactionModeJoy > -1 && job.def.joyMaxParticipants >= compTraining.Props.overrideMaxParticipants) ? compTraining.Props.interactionModeJoy : compTraining.Props.interactionMode) : interactionModeCached;
        public int interactionModeCached = -1;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, Mathf.Max(job.def.joyMaxParticipants, compTraining.Props.overrideMaxParticipants), 0, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            isNotForJoy = job.workGiverDef?.defName.Contains("STE_") ?? false;
            this.EndOnDespawnedOrNull(BuildingTargetInd);
            Toil chooseCell = FindCell(interactionMode);
            yield return chooseCell;
            yield return Toils_Reserve.Reserve(CellTargetInd);
            yield return Toils_Goto.GotoCell(CellTargetInd, PathEndMode.OnCell);
            Toil StudyToil = ToilMaker.MakeToil("StudyToils");
            StudyToil.initAction = delegate
            {
                job.locomotionUrgency = LocomotionUrgency.Walk;
                if (!isNotForJoy && compTraining.isAutoChangeTrainingType)
                {
                    JobDef jobDef = compTraining.CurrentTrainingType().jobDef;
                    float max = 0;
                    foreach (TrainingType trainingType in compTraining.Props.trainingTypes)
                    {
                        SkillRecord skillRecord = pawn?.skills?.GetSkill(trainingType.jobDef.joySkill);
                        if (skillRecord == null)
                        {
                            continue;
                        }
                        float priority = 1 * ((1f + (int)skillRecord.Level) / 10) * ((1f + (int)skillRecord.passion) / 2) * trainingType.XPmult;
                        if (skillRecord.LearningSaturatedToday)
                        {
                            if (!STEMod.Settings.SkillTrainingAfterSaturation)
                            {
                                continue;
                            }
                            priority *= SkillRecord.SaturatedLearningFactor;
                        }
                        if (priority > max)
                        {
                            jobDef = trainingType.jobDef;
                            max = priority;
                        }
                    }
                    compTraining.trainingTypeIndex = compTraining.Props.trainingTypes.FirstIndexOf((TrainingType tt) => tt.jobDef == jobDef);
                }
                compTraining.PawnStartJob(pawn);
            };
            StudyToil.tickAction = delegate
            {
                if (interactionMode <= 0)
                {
                    pawn.rotationTracker.FaceCell(building.OccupiedRect().ClosestCellTo(pawn.Position) + building.Rotation.FacingCell);
                }
                else
                {
                    pawn.rotationTracker.FaceCell(building.OccupiedRect().ClosestCellTo(pawn.Position));
                }
                tickSubAction();
                //if (ticksLeftThisToil == 300)
                //{
                //    SoundDefOf.PlayBilliards.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                //}
                if (isNotForJoy)
                {
                    SkillDef skillDef = compTraining.CurrentTrainingType().jobDef.joySkill;
                    pawn.skills.Learn(skillDef, building.GetStatValue(StatDefOfLocal.STE_TrainGainFactor) * compTraining.CurrentTrainingType().XPmult / 10);
                    pawn.GainComfortFromCellIfPossible(chairsOnly: true);
                    if (pawn?.skills?.GetSkill(skillDef)?.LearningSaturatedToday ?? true)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    if (STEMod.Settings.ShowSkillTrainingProgressBar)
                    {
                        if (effecter == null)
                        {
                            EffecterDef progressBar = EffecterDefOf.ProgressBar;
                            effecter = progressBar.Spawn();
                        }
                        else
                        {
                            effecter.EffectTick(pawn, TargetInfo.Invalid);
                            MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                            if (mote != null)
                            {
                                mote.progress = Mathf.Clamp01(pawn?.skills?.GetSkill(skillDef)?.XpProgressPercent ?? 0f);
                                mote.offsetZ = -0.5f;
                            }
                        }
                    }
                    else
                    {
                        if (effecter != null)
                        {
                            effecter.Cleanup();
                            effecter = null;
                        }
                    }
                }
                else
                {
                    if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    else
                    {
                        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, building);
                    }
                }
                if (compTraining.Props.isWalkRandomly && pawn.IsHashIntervalTick(compTraining.Props.walkInterval) && Rand.Chance(0.25f))
                {
                    IntVec3 intVec = CellToStand(interactionMode);
                    if (intVec != IntVec3.Invalid)
                    {
                        pawn.pather.StartPath(intVec, PathEndMode.OnCell);
                    }
                }
            };
            StudyToil.handlingFacing = true;
            StudyToil.socialMode = RandomSocialMode.SuperActive;
            StudyToil.defaultCompleteMode = ToilCompleteMode.Delay;
            StudyToil.defaultDuration = isNotForJoy ? 4000 : 600;
            StudyToil.AddFinishAction(delegate
            {
                compTraining.PawnEndJob(pawn);
                JoyUtility.TryGainRecRoomThought(pawn);
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            });
            if (isNotForJoy)
            {
                StudyToil.activeSkill = () => compTraining.CurrentTrainingType().jobDef.joySkill;
            }
            yield return StudyToil;
            yield return Toils_Reserve.Release(CellTargetInd);
            if (isNotForJoy)
            {
                yield return Toils_General.Wait(2);
            }
            else
            {
                yield return Toils_Jump.Jump(chooseCell);
            }
        }

        public virtual void tickSubAction()
        {

        }

        private Toil FindCell(int mode = 0)
        {
            Toil findCell = ToilMaker.MakeToil("STEFindCell");
            findCell.initAction = delegate
            {
                Job curJob = findCell.actor.CurJob;
                if (building == null || (!building.Spawned || building.Map != pawn.Map))
                {
                    Log.Error(string.Concat(pawn, " could not find standable cell adjacent to ", building, " because this thing is either unspawned or spawned somewhere else."));
                    pawn.jobs.curDriver.EndJobWith(JobCondition.Errored);
                }
                else
                {
                    IntVec3 intVec = CellToStand(mode);
                    if (intVec == IntVec3.Invalid)
                    {
                        Log.Error(string.Concat(pawn, " could not find standable cell adjacent to ", building));
                        pawn.jobs.curDriver.EndJobWith(JobCondition.Errored);
                        return;
                    }
                    curJob.SetTarget(CellTargetInd, intVec);
                }
            };
            return findCell;
        }

        public IntVec3 CellToStand(int mode = 0)
        {
            int num = 0;
            IntVec3 intVec;
            do
            {
                num++;
                if (num > 100)
                {
                    return IntVec3.Invalid;
                }
                switch (mode)
                {
                    case 1:
                        {
                            intVec = building.InteractionCells.RandomElement();
                            break;
                        }
                    case 2:
                        {
                            intVec = ((building == null) ? TargetA.Cell.RandomAdjacentCellCardinal() : building.RandomAdjacentCellCardinal());
                            break;
                        }
                    case 3:
                        {
                            intVec = ((building == null) ? TargetA.Cell.RandomAdjacentCell8Way() : building.RandomAdjacentCell8Way());
                            break;
                        }
                    case 4:
                        {
                            WatchBuildingUtility.TryFindBestWatchCell(building, base.pawn, false, out IntVec3 result, out var chair);
                            intVec = result;
                            break;
                        }
                    default:
                        {
                            intVec = building.OccupiedRect().RandomCell;
                            break;
                        }
                }
            }
            while (!intVec.Standable(pawn.Map) || !pawn.CanReserve(intVec) || !pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly));
            return intVec;
        }

        public override string GetReport()
        {
            return base.GetReport() + $" {(isNotForJoy ? "SimpleTrainingExpanded.TrainingJon.Train".Translate(building.Label) : "SimpleTrainingExpanded.TrainingJon.Joy".Translate(building.Label))}";
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isNotForJoy, "isNotForJoy", false);
        }

        public override object[] TaleParameters()
        {
            return new object[2]
            {
                pawn,
                building.def
            };
        }
    }
}
