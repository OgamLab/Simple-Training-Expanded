using RimWorld;
using System.Collections.Generic;
using Verse;

namespace SimpleTrainingExpanded
{
    public class RoomRoleWorker_TrainingRoom : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            float num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Thing thing = containedAndAdjacentThings[i];
                if (thing.def.category != ThingCategory.Building)
                {
                    continue;
                }
                CompSTETraining compSTETraining = thing.TryGetComp<CompSTETraining>();
                if (compSTETraining != null)
                {
                    num += thing.GetStatValue(StatDefOfLocal.STE_TrainGainFactor);
                }
            }
            Log.Message($"Score {num} {num*5} {num*7} {num*10} {num*60}");
            return num * 10;
        }
    }
}
