using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Mediation.Interfaces;
using Mediation.PlanTools;
using System.Collections;

namespace Mediation.KnowledgeTools
{
	public static class RobertsonMicrotheory
	{
		public static List<IPredicate> PredicateBinder(Hashtable objectTypes, IPredicate predicate, int termLocation)
		{
			List<IPredicate> predicateList = new List<IPredicate>();

			if (termLocation >= predicate.Terms.Count)
				return new List<IPredicate> { predicate };

			if (!objectTypes.ContainsKey(predicate.TermAt(termLocation).Type))
				return new List<IPredicate>();

			foreach (string obj in objectTypes[predicate.TermAt(termLocation).Type] as List<string>)
			{
				Predicate newPredicate = predicate.Clone() as Predicate;
				newPredicate.BindTerm(obj, termLocation);
				predicateList.AddRange(PredicateBinder(objectTypes, newPredicate, termLocation + 1));
			}

			return predicateList;
		}

		// Returns the predicates that the character is currently observing.
		public static List<IPredicate> FullKnowledgeState(List<IPredicate> predicates, Hashtable objectTypes, List<IPredicate> state, string character)
		{
			List<IPredicate> possiblePredicates = new List<IPredicate>();
			foreach (IPredicate pred in predicates)
				possiblePredicates.AddRange(PredicateBinder(objectTypes, pred, 0));
			List<IPredicate> observedPossibles = KnowledgeState(possiblePredicates, state, character);
			Hashtable stateChecker = new Hashtable();
			foreach (IPredicate pred in state)
				stateChecker[pred.ToString()] = true;
			foreach (IPredicate obs in observedPossibles)
				if (!stateChecker.ContainsKey(obs.ToString()))
					obs.Sign = false;
			return observedPossibles;
		}

		// Returns the predicates that the character is currently observing.
		public static List<IPredicate> KnowledgeState(List<IPredicate> state, string character)
		{
			return KnowledgeState(state, state, character);
		}

		// Returns the predicates that the character could possibly observe of those in the check list given the state.
		public static List<IPredicate> KnowledgeState(List<IPredicate> check, List<IPredicate> state, string character)
		{
			// Create a new list.
			List<IPredicate> newState = new List<IPredicate>();

			// Foreach predicate, check to see if it is observed by the user.
			foreach (IPredicate pred in check)
			{
				if (Observes(character, pred, state))
					newState.Add((Predicate) pred.Clone());
			}
				

			// Return the filtered predicates.
			return newState;
		}

		// Marks the predicates that the character is currently observing.
		public static List<IPredicate> Annotate(List<IPredicate> state, string character)
		{
			// Create a new list.
			List<IPredicate> newState = new List<IPredicate>();

			// Populate the list with clones.
			foreach (IPredicate pred in state)
				newState.Add((Predicate) pred.Clone());

			// Foreach predicate, check to see if it is observed by the user.
			foreach (IPredicate pred in newState)
				if (Observes(character, pred, state))
					pred.Observes(character, true);
				else
					pred.Observes(character, false);

			// Return the annotated predicates.
			return newState;
		}

		// Checks to see if a predicate is currently observed.
		public static bool Observes(string character, IPredicate pred, List<IPredicate> state)
		{
			if (pred.Name.Equals("prefab"))
				return false; // can't ever observe this

			// Get the character's location.
			string characterLocation = GetLocation(character, state);

			// The first and second term of the predicate are distinguished ones.
			string predFirstTerm = pred.TermAt(0).Constant;
			string predSecondTerm = pred.TermAt(1).Constant;
			string predicateLocation = GetLocation(predFirstTerm, state);

			if (characterLocation.Equals(predicateLocation) ||
			    characterLocation.Equals(predFirstTerm) ||
			    (pred.Name.Equals("at") && characterLocation.Equals(predSecondTerm)))
				return true;

			// This predicate is a one special case.  A door is technically not at any location, but
			// rather exists between rooms.  Thus, if the character is at any of the rooms the
			// door connects, the character can observe it.
			if (pred.Name.Equals("doorbetween"))
			{
				// (doorbetween ?x ?y ?z) - ?x is a door between ?y and ?z
				string y = pred.TermAt(1).Constant;
				string z = pred.TermAt(2).Constant;

				if (characterLocation.Equals(y) || characterLocation.Equals(z))
					return true;
			}
				
			// This is another special case - unary predicates that apply to doors.
			// i.e. (door ?x) and (locked ?x)
			if (pred.Name.Equals("door") || pred.Name.Equals("locked"))
			{
				// We can only observe this predicate if we are in a room that has this door.
				// First, find all predicates in the state whose name is "doorbetween"
				List<IPredicate> doorbetweenLiterals = state.FindAll(x => x.Name.Equals("doorbetween"));

				// For each of these literals,
				foreach (IPredicate literal in doorbetweenLiterals)
				{
					// (doorbetween ?x ?y ?z) - ?x is a door between ?y and ?z
					string x = literal.TermAt(0).Constant;

					// If x is the door of the predicate,
					string predicateDoor = pred.TermAt(0).Constant;
					if (x.Equals(predicateDoor))
					{
						string y = literal.TermAt(1).Constant;
						string z = literal.TermAt(2).Constant;

						// And the character is at one of the two locations the door is between,
						if (characterLocation.Equals(y) || characterLocation.Equals(z))
							return true; // the door is observed!
					}
				}

			}
				
			// Otherwise, return false.
			return false;
		}

		// Checks to see if a object is currently observed by the given character.
		public static bool Observes(string character, IObject obj, List<IPredicate> state)
		{
			// Get the character's and the object's location.
			string characterLocation = GetLocation(character, state);
			string objectLocation = GetLocation(obj.Name, state);

			// If the character is at the same location as the object, or
			// If the object is the location the character is at, return true.
			if (characterLocation.Equals(objectLocation) || characterLocation.Equals(obj.Name))
				return true;

			// We need to treat doors as a special case.  Find all predicates in the state whose name is "doorbetween".
			List<IPredicate> doorbetweenLiterals = state.FindAll(x => x.Name.Equals("doorbetween"));

			// Find which of these mention the object.
			foreach (IPredicate literal in doorbetweenLiterals)
			{
				// (doorbetween ?x ?y ?z) - ?x is a door between ?y and ?z
				string x = literal.TermAt(0).Constant;

				// If the object is the door in this literal,
				if (x.Equals(obj.Name))
				{
					string y = literal.TermAt(1).Constant;
					string z = literal.TermAt(2).Constant;

					// And the character is at one of the two locations the door is between,
					if (characterLocation.Equals(y) || characterLocation.Equals(z))
						return true; // the door is observed!
				}
			}
				
			// Otherwise, return false.
			return false;
		}

		// Checks to see if an action is observed.
		public static bool Observes(string character, IOperator action, List<IPredicate> state)
		{
			// Get the character and the action location.
			string characterLocation = GetLocation(character, state);
			string actionLocation = GetLocation(character, action, state);

			// If the character and the action's actor are co-located, return true.
			if (characterLocation.Equals(actionLocation))
				return true;

			// Otherwise, return false.
			return false;
		}

		// Returns the location of an action given a state.
		public static string GetLocation(string character, IOperator action, List<IPredicate> state)
		{
			// If the action has a designated actor, the location is the location of that actor.
			if (!string.IsNullOrEmpty(action.Actor))
				return GetLocation(action.Actor, state);

			// Otherwise,
			else
			{
				// Find all the (location ?x) predicates in the action's preconditions.
				List<IPredicate> locations = new List<IPredicate>();

				foreach (IPredicate precondition in action.Preconditions)
				{
					if (precondition.Name.Equals("location"))
						locations.Add(precondition);
				}

				// If there is exactly one predicate, the location is the term of that predicate.
				if (locations.Count == 1)
					return locations.ToArray()[0].TermAt(0).Constant;

				// Otherwise, we need to get the term from the location predicate where the character is at.
				else
				{
					// Find the character's location:
					string characterLocation = GetLocation(character, state);

					foreach (IPredicate location in locations)
					{
						// We have a match!
						if (location.TermAt(0).Equals(characterLocation))
							return characterLocation;
					}
				}
			}

			// If it all fails, return the empty string.
			return "";
		}

		// Returns the location of an object given a state.
		public static string GetLocation(string obj, List<IPredicate> state)
		{
			// Loop through the predicates in the state.
			foreach (Predicate pred in state)
			{
				// Depending on the predicate symbol, return the correct location.

				// Predicate: (at ?x ?y)
				// Read as "?x is at ?y"
				if (pred.Name.Equals("at") && pred.TermAtEquals(0, obj))
				{
					return pred.TermAt(1).Constant;
				}

				// Predicate: (in ?x ?y)
				// Read as "?x is in ?y"
				else if (pred.Name.Equals("in") && pred.TermAtEquals(0, obj))
				{
					foreach (Predicate p in state)
						if (p.Name.Equals("open") && p.TermAtEquals(0, pred.TermAt(1).Constant))
							return GetLocation(pred.TermAt(1).Constant, state);
				}

				// Predicate: (location ?x)
				// Read as "?x is a location" (by convention, all locations are within themself).
				else if (pred.Name.Equals("location") && pred.TermAtEquals(0, obj))
				{
					return pred.TermAt(0).Constant;
				}

				// Predicate: (room ?x)
				// Read as "?x is a room" (by convention, all rooms are within themself).
				else if (pred.Name.Equals("room") && pred.TermAtEquals(0, obj))
				{
					return pred.TermAt(0).Constant;
				}

				// Predicate: (has ?x ?y)
				// Read as "?x has ?y"
				else if (pred.Name.Equals("has") && pred.TermAtEquals(1, obj))
				{
					return GetLocation(pred.TermAt(0).Constant, state);
				}

				// Predicate: (on ?x ?y)
				// Read as "?x is on ?y"
				else if (pred.Name.Equals("on") && pred.TermAtEquals(0, obj))
				{
					return GetLocation(pred.TermAt(1).Constant, state);
				}
			}

			// If nothing is found, return a null location.
			return "";
		}
	}
}
