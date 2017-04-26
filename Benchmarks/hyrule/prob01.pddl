(define (problem 01)
   (:domain HYRULE)
   (:objects 
        ;; Dramatis Personae
        arthur mel alli michael ian giovanna jim roger frank - character

        ;; Locations
        junkyard docks bar townarch townsquare hut forge shop bank valley barracks cliff - location

        ;; Entrances
        barentrance barexit hutentrance hutexit forgeentrance forgeexit shopentrance shopexit bankentrance bankexit barracksentrance barracksexit - entrance               

        ;; Items
        knightsword knightshield rubyring pileofcoins humanskull candelabra ash lovecontract tastycupcake - item

        ;; Keys
        townkey - key

        ;; Doors
        towngate - door

        ;; Character Prefabs
        player wizard orc riddler quartermaster appraiser fortuneteller knight paladin - prefab

        ;; Location Prefabs
        sand woods cave town beach junk woodenhouse brickhouse - prefab

        ;; Item Prefabs
        sword shield ring key coins skull candle cat cupcake contract woodendoor gate - prefab
   )
   (:init 
        ;; Player Character
        (player arthur)
          
        ;; ---- Characters ----
        (prefab arthur player)
        (prefab mel wizard) 
        (prefab michael fortuneteller)
        (prefab ian quartermaster)
        (prefab alli orc)
        (prefab giovanna appraiser)
        (prefab jim appraiser)
        (prefab roger knight)
        (prefab frank paladin)

        ;; ---- Items ----
        (prefab knightsword sword)
        (prefab rubyring ring)
        (prefab knightshield shield)
        (prefab tastycupcake cupcake)
        (prefab ash cat)
        (prefab pileofcoins coins)
        (prefab lovecontract contract)
        (prefab humanskull skull)
        (prefab candelabra candle)

        ;; ---- Keys and Locks ----
        (prefab townkey key)
        (prefab towngate gate)
        (unlocks townkey towngate)

        ;; ---- Locations ----
        (prefab junkyard junk)
        (prefab docks beach)
        (prefab bar woodenhouse)
        (prefab townarch town)
        (prefab hut woodenhouse)
        (prefab forge woodenhouse)
        (prefab townsquare town)
        (prefab shop brickhouse)
        (prefab bank brickhouse)
        (prefab valley cave)
        (prefab barracks brickhouse)
        (prefab cliff beach)
        
        ;; ---- Entrances ----
        (prefab barentrance woodendoor)
        (prefab barexit woodendoor)
        
        (prefab forgeentrance woodendoor)
        (prefab forgeexit woodendoor)

        (prefab hutentrance woodendoor)
        (prefab hutexit woodendoor)
        
        (prefab shopentrance woodendoor)
        (prefab shopexit woodendoor)
        
        (prefab bankentrance woodendoor)
        (prefab bankexit woodendoor)
        
        (prefab barracksentrance woodendoor)
        (prefab barracksexit woodendoor)
        
        
        ;; ---- World Map ----
        
        ;; The junkyard connects to the docks
        (connected junkyard docks)        (doorway junkyard docks)
        (connected docks junkyard)        (doorway docks junkyard)
        
        
        ;; The docks contains the bar and connects to the town archway
        (at barentrance docks)            (leadsto barentrance bar)
        (at barexit bar)                  (leadsto barexit docks)
        (closed barexit)

        (connected docks townarch)        (doorway docks townarch)
        (connected townarch docks)        (doorway townarch docks)
        
        

        
        ;; ---- Initial Configuration ----
        
        ;; location: junkyard                
        (at ash junkyard)
        (at alli junkyard)      (wants-item alli tastycupcake) 
                                (wants-item alli ash)



        ;; location: bar
        (at mel bar)            (wants-item mel rubyring)
        (at arthur bar)       



        ;; location: townarch
        (at townkey townarch)
        (at lovecontract townarch)        





         )
    (:goal 
        (and

            ;; Pilgrimage Quest
            (has alli ash)
            

        )
    )
)