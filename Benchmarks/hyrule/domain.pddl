(define (domain HYRULE)
   (:requirements :adl :typing :universal-preconditions)
   (:types location entity prefab - object
           actant portal - entity
           door entrance - portal
           character item - actant
           key - item
   )
   (:predicates 
        
        ;; ---- Space ----
        (at         ?x - entity ?y - location)   ;; entity ?x is at location ?y
        (has        ?x - character ?y - item)    ;; character ?x has item ?y
        (leadsto    ?x - entrance ?y - location) ;; entrance ?x leads to location ?y
        (connected  ?x - location ?y - location) ;; location ?x is connected to location ?y
        
        ;; ---- Keys and Locks ----
        (closed         ?x - entrance)  ;; entrance ?x is closed
        (locked         ?x - portal)    ;; portal ?x is locked
        (unlocks        ?x - key        ?y - door)      ;; key ?x unlocks door ?y
        (doorway        ?x - location   ?y - location)  ;; there is no door between ?y and ?z
        (doorbetween    ?x - door       ?y - location   ?z - location) ;; door ?x is between locations ?y and ?z

        ;; ---- Game-specific ----
        (player     ?x - character)
        (wants-item ?x - character  ?y - item)      ;; character ?x wants item ?y

        ;; ---- Rendering ----
        (prefab     ?x - object ?y - prefab)        ;; the prefab of ?x is ?y
    )

    ;; ----------------------------------------------------------------
    ;; Adventure Game Commands
    ;; ----------------------------------------------------------------
    
    ;; A ?character opens a closed ?entrance at a ?location.
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
            (and 
                (not (closed ?entrance))
            )
    )
    
    ;; A ?character closes an not-closed ?entrance at a ?location.
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
            (and
                (closed ?entrance)
            )
    )
    
    ;; A ?character picks up an ?item at a ?location
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
    
    ;; A ?sender gives an ?item to a ?receiver at a ?location
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
    
    ;; A ?character moves from the ?from location to the ?to location.
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
    
    ;; A ?character moves from the ?from location to the ?to location, through an unlocked door.
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
    
    ;; A ?character moves from the ?from location to the ?to location, through an ?entrance.
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
    
    ;; ----------------------------------------------------------------
    ;; Use (context-sensitive) Commands
    ;; ----------------------------------------------------------------

    ;; A ?character unlocks a ?door with a ?key. Consumes the ?key in the action.
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