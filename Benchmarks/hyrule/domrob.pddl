(define
	(domain HYRULE)
	(:requirements :adl :typing :universal-preconditions)
	(:types 
		actant portal - entity
		key - item
		character item - actant
		location entity prefab - object
		door entrance - portal
	)
	(:constants )
	(:predicates
		(at ?x - entity ?y - location)
		(has ?x - character ?y - item)
		(leadsto ?x - entrance ?y - location)
		(connected ?x - location ?y - location)
		(closed ?x - entrance)
		(locked ?x - portal)
		(unlocks ?x - key ?y - door)
		(doorway ?x - location ?y - location)
		(doorbetween ?x - door ?y - location ?z - location)
		(player ?x - character)
		(wants-item ?x - character ?y - item)
		(prefab ?x - object ?y - prefab)
	)

	(:action open
		:parameters (?character - character ?entrance - entrance ?location - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?location)
				(at ?entrance ?location)
				(closed ?entrance)
				(not (locked ?entrance))
			)
		:effect
				(not (closed ?entrance))
	)

	(:action close
		:parameters (?character - character ?entrance - entrance ?location - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?location)
				(at ?entrance ?location)
				(not (closed ?entrance))
				(not (locked ?entrance))
			)
		:effect
				(closed ?entrance)
	)

	(:action pickup
		:parameters (?character - character ?item - item ?location - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?location)
				(at ?item ?location)
			)
		:effect
			(and
				(not (at ?item ?location))
				(has ?character ?item)
			)
	)

	(:action give
		:parameters (?sender - character ?item - item ?receiver - character ?location - location)
		:precondition
			(and
				(player ?sender)
				(at ?sender ?location)
				(has ?sender ?item)
				(at ?receiver ?location)
				(not (has ?receiver ?item))
				(wants-item ?receiver ?item)
			)
		:effect
			(and
				(not (has ?sender ?item))
				(has ?receiver ?item)
			)
	)

	(:action move-through-doorway
		:parameters (?character - character ?from - location ?to - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?from)
				(not (at ?character ?to))
				(connected ?from ?to)
				(doorway ?from ?to)
				(not (= ?from ?to))
			)
		:effect
			(and
				(not (at ?character ?from))
				(at ?character ?to)
			)
	)

	(:action move-through-door
		:parameters (?character - character ?from - location ?door - door ?to - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?from)
				(not (at ?character ?to))
				(not (= ?from ?to))
				(connected ?from ?to)
				(doorbetween ?door ?from ?to)
				(not (locked ?door))
			)
		:effect
			(and
				(not (at ?character ?from))
				(at ?character ?to)
			)
	)

	(:action move-through-entrance
		:parameters (?character - character ?from - location ?entrance - entrance ?to - location)
		:precondition
			(and
				(player ?character)
				(at ?character ?from)
				(not (at ?character ?to))
				(not (= ?from ?to))
				(at ?entrance ?from)
				(leadsto ?entrance ?to)
				(not (closed ?entrance))
			)
		:effect
			(and
				(not (at ?character ?from))
				(at ?character ?to)
			)
	)

	(:action unlock-door
		:parameters (?character - character ?key - key ?door - door)
		:precondition
			(and
				(player ?character)
				(locked ?door)
				(has ?character ?key)
				(unlocks ?key ?door)
			)
		:effect
			(and
				(not (locked ?door))
				(not (has ?character ?key))
			)
	)
)
