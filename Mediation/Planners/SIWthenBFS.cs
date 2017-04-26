using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


using Mediation.PlanTools;
using Mediation.FileIO;

namespace Mediation.Planners
{
	public static class SIWthenBFS 
	{
		public static Plan Plan(Domain domain, Problem problem)
		{
			// Set up the domain path.
			string domain_path = Parser.GetTopDirectory() + @"Benchmarks/" + domain.Name.ToLower();
			string planner_path = Parser.GetTopDirectory() + @"External/planners/siw-then-bfsf";

			// Create new PDDL problem and domain files.
			Writer.ProblemToPDDL(domain_path + @"/probrob.pddl", domain, problem, problem.Initial);
			Writer.DomainToPDDL(domain_path + @"/domrob.pddl", domain);

			// Start SIWthenBFS's batch file.
			ProcessStartInfo startInfo = new ProcessStartInfo(planner_path + @"/plan");

			// Store the process' arguments.
			startInfo.Arguments = 
				"--domain " + domain_path + @"/domrob.pddl" + " " +
				"--problem " + domain_path + @"/probrob.pddl " + " " +
				"--output " + planner_path + @"/plan.ipc";

			startInfo.WindowStyle = ProcessWindowStyle.Hidden;

			// Erase old data.
			System.IO.File.WriteAllText(planner_path + @"/plan.ipc", string.Empty);

			// Start the process and wait for it to finish.
			using (Process proc = Process.Start(startInfo)) {
				proc.WaitForExit();
			}
				
			// Parse the results into a plan object.
			return Parser.GetPlan(planner_path + @"/plan.ipc", domain, problem);
		}

	}


}

