using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.PlanTools;
using Mediation.Interfaces;

namespace Mediation.StateSpace
{
    public static class PlanSimulator
    {
        // Given a plan and the current state, verify it can be executed.
        public static bool VerifyPlan (Plan plan, State state, List<IObject> objects)
        {
            // Create a clone of the state, to not affect the given one.
            State stateClone = state.Clone() as State;

            // Apply the entire plan to the current state.
            for (int planStepIndex = 0; planStepIndex < plan.Steps.Count; planStepIndex++)
            {
                Operator step = plan.Steps.ElementAt(planStepIndex) as Operator;
                
                // If the next plan step can be executed...
                if(stateClone.Satisfies(step.Preconditions))
                {
                    // Apply it, and continue.
                    stateClone.UpdateState(step, objects);
                }

                else
                {
                    // Otherwise, this is not a valid plan.
                    return false;
                }
            }

            // If we get to the end without reporting an error, we have a valid plan.
            return true;
        }
    }
}
