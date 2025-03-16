using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SimpleTrainingExpanded
{
    public class SimpleTrainingExpandedMod : Mod
    {
        public SimpleTrainingExpandedMod(ModContentPack pack) : base(pack)
        {
			new Harmony("SimpleTrainingExpandedMod").PatchAll();
        }
    }

    public class WorkGiver_Train : WorkGiver_Scanner
    {

    }

    public class JobDriver_Train : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {

        }
    }

    public class CompProperties_TrainSkills : CompProperties
    {
        public List<SkillTrain> skillsToTrainPerTick;
        public float joyPerTick;
        public float learningPerTick;
        public JobDef trainingJob;
        public bool rangedWeaponRequired;
        public bool meleeWeaponRequired;

        public CompProperties_TrainSkills()
        {
            this.compClass = typeof(CompTrainSkills);
        }
    }

    public class CompTrainSkills : ThingComp
    {
        public CompProperties_TrainSkills Props => base.props as CompProperties_TrainSkills;

        public void PawnTrainTick(Pawn pawn)
        {
            foreach (var skillToTrain in Props.skillsToTrainPerTick)
            {
                var skill = pawn.skills.GetSkill(skillToTrain.skill);
                skill.Learn(skillToTrain.xp);
            }
            JoyUtility.JoyTickCheckEnd(pawn);
        }
    }
}
