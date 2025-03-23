using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace SimpleTrainingExpanded
{
    public class JobDriver_StudyBuilding : JobDriver
    {
        private const TargetIndex BuildingTargetInd = TargetIndex.A;
        private const TargetIndex CellTargetInd = TargetIndex.B;

        public Building building => TargetThingA as Building;
        public SkillDef skillDef => job.def.joySkill;

        private Effecter effecter = null;
        public bool isNotForJoy;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            CompSTETraining compTraining = building.GetComp<CompSTETraining>();
            isNotForJoy = job.workGiverDef?.defName.Contains("STE_") ?? false;
            this.EndOnDespawnedOrNull(BuildingTargetInd);
            Toil chooseCell = FindCell(BuildingTargetInd, CellTargetInd, compTraining.Props.interactionMode);
            yield return chooseCell;
            yield return Toils_Reserve.Reserve(CellTargetInd);
            yield return Toils_Goto.GotoCell(CellTargetInd, PathEndMode.OnCell);
            Toil StudyToil = ToilMaker.MakeToil("StudyToils");
            StudyToil.initAction = delegate
            {
                job.locomotionUrgency = LocomotionUrgency.Walk;
                if (compTraining.isAutoChangeTrainingType)
                {
                    compTraining.trainingTypeIndex = compTraining.Props.trainingTypes.FirstIndexOf((TrainingType tt) => tt.jobDef.joySkill == job.def.joySkill);
                }
            };
            StudyToil.tickAction = delegate
            {
                pawn.rotationTracker.FaceCell(building.OccupiedRect().ClosestCellTo(pawn.Position));
                //if (ticksLeftThisToil == 300)
                //{
                //    SoundDefOf.PlayBilliards.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                //}
                if (isNotForJoy)
                {
                    pawn.skills.Learn(skillDef, building.GetStatValue(StatDefOfLocal.STE_TrainGainFactor) / 10);
                    pawn.GainComfortFromCellIfPossible(chairsOnly: true);
                    if (pawn?.skills?.GetSkill(skillDef)?.LearningSaturatedToday ?? true)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
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
                    if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    else
                    {
                        JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, building);
                    }
                }
            };
            StudyToil.handlingFacing = true;
            StudyToil.socialMode = RandomSocialMode.SuperActive;
            StudyToil.defaultCompleteMode = ToilCompleteMode.Delay;
            StudyToil.defaultDuration = isNotForJoy ? 4000 : 600;
            StudyToil.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
                if (effecter != null)
                {
                    effecter.Cleanup();
                    effecter = null;
                }
            });
            if (isNotForJoy)
            {
                StudyToil.activeSkill = () => skillDef;
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

        private Toil FindCell(TargetIndex adjacentToInd, TargetIndex cellInd, int mode = 0)
        {
            Toil findCell = ToilMaker.MakeToil("STEFindCell");
            findCell.initAction = delegate
            {
                Pawn actor = findCell.actor;
                Job curJob = actor.CurJob;
                LocalTargetInfo target = curJob.GetTarget(adjacentToInd);
                if (target.HasThing && (!target.Thing.Spawned || target.Thing.Map != actor.Map))
                {
                    Log.Error(string.Concat(actor, " could not find standable cell adjacent to ", target, " because this thing is either unspawned or spawned somewhere else."));
                    actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
                }
                else
                {
                    int num = 0;
                    IntVec3 intVec;
                    do
                    {
                        num++;
                        if (num > 100)
                        {
                            Log.Error(string.Concat(actor, " could not find standable cell adjacent to ", target));
                            actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
                            return;
                        }
                        switch (mode)
                        {
                            case 1:
                                {
                                    intVec = target.Thing.InteractionCells.RandomElement();
                                    break;
                                }
                            case 2:
                                {
                                    intVec = ((!target.HasThing) ? target.Cell.RandomAdjacentCellCardinal() : target.Thing.RandomAdjacentCellCardinal());
                                    break;
                                }
                            case 3:
                                {
                                    intVec = ((!target.HasThing) ? target.Cell.RandomAdjacentCell8Way() : target.Thing.RandomAdjacentCell8Way());
                                    break;
                                }
                            case 4:
                                {
                                    WatchBuildingUtility.TryFindBestWatchCell(target.Thing, pawn, false, out IntVec3 result, out var chair);
                                    intVec = result;
                                    break;
                                }
                            default:
                                {
                                    intVec = target.Cell;
                                    break;
                                }
                        }
                    }
                    while (!intVec.Standable(actor.Map) || !actor.CanReserve(intVec) || !actor.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly));
                    curJob.SetTarget(cellInd, intVec);
                }
            };
            return findCell;
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
