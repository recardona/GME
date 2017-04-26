using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mediation.Interfaces;
using Mediation.PlanTools;

namespace Mediation.KnowledgeTools
{
	/// <summary>
	/// An environment model is a representation of what an agent knows about
	/// regarding a planning task environment that it is in.
	/// </summary>
	public class EnvironmentModel
	{
		#region Properties

		// The name of the agent for whom we are building this environment model.
		private string agentName;

		// The operators the agent is aware of.
		private List<IOperator> knownOperators;

		// The predicates the agent is aware of.
		private List<IPredicate> knownPredicates;

		// The objects the agent is aware of.
		private List<IObject> knownObjects;

		// The current state of the world known to the agent.
		private IState knownCurrentState;

		/// <summary>
		/// Gets the name of the agent for whom we are building this environment model
		/// </summary>
		/// <value>A string that identifies the agent.</value>
		public string AgentName {
			get { return this.agentName; }	
		}

		/// <summary>
		/// Gets the operators known to the model.
		/// </summary>
		/// <value>A list of known operators.</value>
		public List<IOperator> KnownOperators {
			get { return this.knownOperators; }
		}

		/// <summary>
		/// Gets the predicates known to the model.
		/// </summary>
		/// <value>A list known predicates.</value>
		public List<IPredicate> KnownPredicates {
			get { return this.knownPredicates; }
		}

		/// <summary>
		/// Gets the objects known to the model.
		/// </summary>
		/// <value>A list of known objects.</value>
		public List<IObject> KnownObjects {
			get { return this.knownObjects; }
		}

		/// <summary>
		/// Gets the state known to the model.
		/// </summary>
		/// <value>The current state of the task environment known to the agent.</value>
		public IState KnownCurrentState {
			get { return this.knownCurrentState; }
		}

		#endregion

		// A reference to the domain.
		private Domain planningDomain;

		// A reference to the problem.
		private Problem planningProblem;


		/// <summary>
		/// Creates an empty environment model for the given agent in the given domain and problem.
		/// </summary>
		/// <param name="agentName">Agent name.</param>
		/// <param name="planningDomain">The planning domain to create an agent model out of.</param>
		/// <param name="planningProblem">The planning problem to create an agent model out of.</param>
		public EnvironmentModel(string agentName, Domain planningDomain, Problem planningProblem)
		{
			// Create new lists for the properties.
			this.knownOperators = new List<IOperator>();
			this.knownPredicates = new List<IPredicate>();
			this.knownObjects = new List<IObject>();
			this.knownCurrentState = new State();

			// Store properties.
			this.agentName = agentName;
			this.planningDomain = planningDomain;
			this.planningProblem = planningProblem;

			// Compute the static predicates of the domain.
			this.planningDomain.ComputeStaticPredicates();

			// Update the model with the initial state.
			this.UpdateModel(planningProblem.Initial);
		}

		/// <summary>
		/// Updates the model on the basis of a new state.
		/// </summary>
		/// <param name="newState">The new state that is known to the model.</param>
		public void UpdateModel(List<IPredicate> newState)
		{
			// For every object in the domain,
			foreach (IObject domainObject in this.planningProblem.Objects)
			{
				// If it is observed and unknown, add the object to the model.
				if (RobertsonMicrotheory.Observes(this.agentName, domainObject, newState) && !this.KnowsObject(domainObject))
					this.knownObjects.Add(domainObject);
			}

			// For every observed literal of the given state,
			List<IPredicate> newStateKnowledge = RobertsonMicrotheory.KnowledgeState(newState, this.agentName);

			foreach (IPredicate literal in newStateKnowledge)
			{
				// Add the literal to the state if it isn't already there.
				if (!this.KnowsLiteral(literal))
				{
					List<IPredicate> literals = this.knownCurrentState.Predicates;
					literals.Add(literal);
					this.knownCurrentState.Predicates = literals;
				}

				// Add the predicate to the known predicates if it isn't already there.
				if (!this.KnowsPredicate(literal.Name))
				{
					IPredicate predicate = this.planningDomain.Predicates.Find(t => t.Name.Equals(literal.Name));
					this.knownPredicates.Add(predicate);
				}
			}
		}


		/// <summary>
		/// Updates the model on the basis of a new operator and a new state.
		/// </summary>
		/// <param name="newAction">Last action.</param>
		/// <param name="newState">Resulting state.</param>
		public void UpdateModel(IOperator newAction, List<IPredicate> newState)
		{
			// If the action is observed,
			if (RobertsonMicrotheory.Observes(this.agentName, newAction, newState))
			{
				// Add its operator to the model if it isn't already there.
				if (!this.KnowsOperator(newAction))
				{
					this.knownOperators.Add(newAction.Template() as Operator);
				}

				// If the model has an active knownCurrentState 
				if (this.knownCurrentState.Predicates.Count != 0)
				{
					// Apply the action to the state with the current known objects (the latter is for the case of axioms)
					this.knownCurrentState = (this.knownCurrentState as State).NewState((newAction as Operator), this.KnownObjects);
				}
			}
				
			// Delegate the rest to the other method.
			UpdateModel(newState);
		}


		/// <summary>
		/// Compiles this environment to a planning domain and problem.  The two are created in reference to what this
		/// EnvironmentModel knows about. 
		/// </summary>
		/// <returns>The to domain and problem.</returns>
		//		public KeyValuePair<IDomain, IProblem> CompileToDomainAndProblem()
		//		{
		//
		//
		//			return null;
		//
		//
		//
		//			
		//		}

		/// <summary>
		/// Compiles the necessary elements of this model to create a domain.
		/// </summary>
		/// <returns>The domain that is known to this model.</returns>
		public IDomain CompileDomain()
		{
			// Create the domain, which is a clone of the original one with 
			// an updated name, set of operators, and predicates.
			Domain environmentDomain = this.planningDomain.Clone() as Domain;

			// Domain name is prepended with the name of the agent who owns this EnvironmentModel.
			string domainName = this.agentName + "_" + this.planningDomain.Name;
			environmentDomain.Name = domainName;

			// Predicates and Operators are the ones known to this model.
			environmentDomain.Predicates = this.knownPredicates;
			environmentDomain.Operators = this.knownOperators;

			return environmentDomain;
		}

		/// <summary>
		//// Compiles the necessary elements of this model to create a problem.
		/// </summary>
		/// <returns>The problem that is known to this model.</returns>
		public IProblem CompileProblem()
		{
			// Create the problem, which is a clone of the original one with
			// an updated name, set of objects, and initial state.
			Problem environmentProblem = this.planningProblem.Clone() as Problem;

			// Domain name is prepended with the name of the agent who owns this EnvironmentModel.
			string domainName = this.agentName + "_" + this.planningDomain.Name;
			string problemName = this.agentName + "_" + this.planningProblem.Name;

			environmentProblem.Domain = domainName;
			environmentProblem.Name = problemName;

			// Objects and Initial state are the ones known to this model.
			environmentProblem.Objects = this.knownObjects;
			environmentProblem.Initial = this.knownCurrentState.Predicates;

			return environmentProblem;
		}


		/// <summary>
		/// Checks to see if the model is aware of the given predicate literal.  This method checks to see if its model
		/// of the world satisfies the literal.
		/// </summary>
		/// <returns><c>true</c>, if literal was known, <c>false</c> otherwise.</returns>
		/// <param name="pred">Pred.</param>
		private bool KnowsLiteral(IPredicate pred)
		{
			return (this.knownCurrentState.InState(pred));
		}

		/// <summary>
		/// Checks to see if the model is aware of the given predicate.  This method checks to see if it is aware of the
		/// given predicate relation as something that exists in the environment.
		/// </summary>
		/// <returns><c>true</c>, if predicate was known, <c>false</c> otherwise.</returns>
		/// <param name="pred">Pred.</param>
		private bool KnowsPredicate(string predicateSymbol)
		{
			// find the predicate in the domain
			IPredicate domainPredicate = null;
			foreach (IPredicate pred in this.planningDomain.Predicates)
			{
				// if we find it, save it and break
				if (pred.Name.Equals(predicateSymbol))
				{
					domainPredicate = pred;
					break;
				}
			}

			// if we couldn't find it, then we can't know it. 
			if (domainPredicate == null)
				throw new ArgumentException("Undefined predicate symbol: " + predicateSymbol);

			// otherwise, check if that predicate is already known
			else
			{
				return (this.knownPredicates.Contains(domainPredicate));
			}
		}

		/// <summary>
		/// Checks to see if the model is aware of the given operator.
		/// </summary>
		/// <returns><c>true</c>, if operator was known, <c>false</c> otherwise.</returns>
		/// <param name="op">Op.</param>
		private bool KnowsOperator(IOperator op)
		{
			return (this.knownOperators.Contains(op.Template() as Operator));
		}

		/// <summary>
		/// Checks to see if the model is aware of the given object.
		/// </summary>
		/// <returns><c>true</c>, if object was known, <c>false</c> otherwise.</returns>
		/// <param name="obj">Object.</param>
		private bool KnowsObject(IObject obj)
		{
			return (this.knownObjects.Contains(obj));
		}
	}
}

