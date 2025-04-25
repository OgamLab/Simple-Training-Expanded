using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SimpleTrainingExpanded
{
    public class JobDriver_StudyShooting : JobDriver_StudyBuilding
    {
        public override void tickSubAction()
        {
            if (pawn.IsHashIntervalTick(compTraining.Props.fleckInterval) && pawn.Position.ShouldSpawnMotesAt(pawn.Map) && compTraining.Props.fleckDef != null)
            {
                Vector3 vector = base.TargetA.Cell.ToVector3Shifted() + Vector3Utility.RandomHorizontalOffset((1f - (float)pawn.skills.GetSkill(SkillDefOf.Shooting).Level / 20f) * 1.8f);
                vector.y = pawn.DrawPos.y;
                if (compTraining.Props.soundCast != null)
                {
                    compTraining.Props.soundCast.PlayOneShot(new TargetInfo(pawn.Position, pawn.MapHeld));
                }
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(pawn.DrawPos, pawn.Map, compTraining.Props.fleckDef);
                dataStatic.rotation = (vector - dataStatic.spawnPosition).AngleFlat();
                dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
                dataStatic.velocitySpeed = compTraining.Props.velocitySpeed;
                dataStatic.airTimeLeft = Mathf.RoundToInt((dataStatic.spawnPosition - vector).MagnitudeHorizontal() / compTraining.Props.velocitySpeed);
                pawn.Map.flecks.CreateFleck(dataStatic);
            }
        }
    }
}
