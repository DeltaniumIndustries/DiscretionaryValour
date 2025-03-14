using System;
using XRL.World.Capabilities;

namespace XRL.World.Parts
{
    [Serializable]
    public class CowardlyBehaviour : IPart
    {
        private bool beenDaunted = false;

        // Register the event
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CheckingHostilityTowardsPlayer");
            base.Register(Object, Registrar);
        }

        // Evaluate if the parent object feels threatened by a given game object
        public void EvaluateThreat(GameObject GO)
        {
            if (GO == null || this.ParentObject.Brain == null) return;

            // Skip if the brain doesn't perceive the object as a threat
            if (IsFriendlyOrNeutral(GO)) return;

            // Skip if the brain is already targeting the object
            if (IsTargeting(GO)) return;

            // If the parent object has already been daunted, skip
            if (beenDaunted) return;

            AssessAndSetThreat(GO);
        }

        private bool IsFriendlyOrNeutral(GameObject GO)
        {
            return this.ParentObject.Brain.GetFeeling(GO) >= 0;
        }

        private bool IsTargeting(GameObject GO)
        {
            return this.ParentObject.Brain.Target == GO;
        }

        // Assess the threat and take action if necessary
        private void AssessAndSetThreat(GameObject GO)
        {
            int? assessedThreat = DifficultyEvaluation.GetDifficultyRating(GO, this.ParentObject, true);
            if (assessedThreat == null) return;

            // Set faction feeling and mark as daunted if the threat is above the threshold
            if (IsThreatAboveThreshold(assessedThreat))
            {
                this.ParentObject.Brain.SetFactionFeeling(GO.GetPrimaryFaction(), 0);
                beenDaunted = true;
            }
        }

        private bool IsThreatAboveThreshold(int? assessedThreat)
        {
            return assessedThreat >= 15 || (assessedThreat >= 10 && this.ParentObject.Brain.GetFeeling(IPart.ThePlayer) >= -50);
        }

        // Handle event for checking hostility
        public override bool FireEvent(Event E)
        {
            if (E.ID == "CheckingHostilityTowardsPlayer")
            {
                EvaluateThreat(IPart.ThePlayer);
            }
            return true;
        }
    }
}
