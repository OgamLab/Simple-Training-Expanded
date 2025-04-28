using RimWorld;
using Verse;

namespace SimpleTrainingExpanded
{
    public class JoyGiver_WatchPerfomance : JoyGiver_WatchBuilding
    {
        protected override bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
        {
            Log.Message($"{base.CanInteractWith(pawn, t, inBed)}");
            if (base.CanInteractWith(pawn, t, inBed))
            {
                CompSTETraining compTraining = t.TryGetComp<CompSTETraining>();
                Log.Message($"{compTraining != null} && ({compTraining.Props.minPawnsForJoy < 0} || {compTraining.usingPawns.Count > compTraining.Props.minPawnsForJoy} {compTraining.usingPawns.Count} >= {compTraining.Props.minPawnsForJoy})");
                if (compTraining != null && (compTraining.Props.minPawnsForJoy < 0 || compTraining.usingPawns.Count >= compTraining.Props.minPawnsForJoy))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
