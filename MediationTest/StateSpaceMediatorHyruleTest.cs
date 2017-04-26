using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mediation.Interfaces;
using Mediation.PlanTools;
using Mediation.FileIO;
using Mediation.Planners;
using Mediation.Enums;
using Mediation.StateSpace;

namespace MediationTest
{
    [TestClass]
    public class StateSpaceMediatorHyruleTest
    {
        string testDomainName;
        string testDomainDirectory;
        Domain testDomain;
        Problem testProblem;
        Plan testPlan;

        public StateSpaceMediatorHyruleTest()
        {
            testDomainName = "hyrule";
            testDomainDirectory = Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl";
            testDomain = Parser.GetDomain(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\domain.pddl", PlanType.StateSpace);
            testProblem = Parser.GetProblem(Parser.GetTopDirectory() + @"Benchmarks\" + testDomainName + @"\prob01.pddl");
            testPlan = FastDownward.Plan(testDomain, testProblem);
        }

        [TestMethod]
        public void HyruleParserTest()
        {
            // There five high level types.
            Assert.AreEqual(5, testDomain.ObjectTypes.Count);

            foreach (IPredicate pred in testProblem.Initial)
                if (pred.Name.Equals("connected") && pred.TermAt(0).Constant.Equals("junkyard"))
                    Assert.AreEqual(pred.Terms.Count, 2);
        }

        [TestMethod]
        public void HyruleActionTest()
        {
            StateSpaceNode root = StateSpaceMediator.BuildTree(Planner.FastDownward, testDomain, testProblem, testPlan, testPlan.Initial as State, 3);
            Assert.AreEqual(1, root.outgoing.Count);
        }

        [TestMethod]
        public void HyrulePlanTest()
        {
            Assert.AreEqual(5, testPlan.Steps.Count);
        }

        [TestMethod]
        public void HyruleTypesTest()
        {
            Hashtable objsByType = testProblem.ObjectsByType;
            List<string> locations = objsByType["location"] as List<string>;
            Assert.AreEqual(12, locations.Count);
        }
    }
}
