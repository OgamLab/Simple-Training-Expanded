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
        public SkillDef skillDef => job.def.joySkill;

        private Effecter effecter = null;
        public bool isNotForJoy = false;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, job.def.joyMaxParticipants, 0, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(BuildingTargetInd);
            Toil chooseCell = Toils_Misc.FindRandomAdjacentReachableCell(BuildingTargetInd, CellTargetInd);
            yield return chooseCell;
            yield return Toils_Reserve.Reserve(CellTargetInd);
            yield return Toils_Goto.GotoCell(CellTargetInd, PathEndMode.OnCell);
            Toil StudyToil = ToilMaker.MakeToil("StudyToils");
            StudyToil.initAction = delegate
            {
                job.locomotionUrgency = LocomotionUrgency.Walk;
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
                    pawn.skills.Learn(skillDef, 0.1f);
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
