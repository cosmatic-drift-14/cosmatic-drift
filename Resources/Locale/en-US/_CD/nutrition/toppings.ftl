topping-verb-add-topping = Garnish with
topping-verb-portion-too-small = Portion too small
topping-verb-no-space = No space left
topping-examine = {$format ->
        [one-submerged] With {$lastSubmerged} submerged.
        [many-submerged] With {$listSubmerged} and {$lastSubmerged} submerged.
        [one-topping] Garnished with {$lastTopping}.
        [one-topping-one-submerged] With {$lastSubmerged} submerged and garnished with {$lastTopping}.
        [one-topping-many-submerged] With {$listSubmerged} and {$lastSubmerged}. As a garnish there is {$lastTopping}.
        [many-topping] With {$listTopping} and garnished with {$lastTopping}.
        [many-topping-one-submerged] With {$lastSubmerged} submerged. As a garnish there is {$listTopping} and {$lastTopping}.
        *[many-topping-many-submerged] With {$listSubmerged} and {$lastSubmerged} submerged. As a garnish there is {$listTopping} and {$lastTopping}.
    }
