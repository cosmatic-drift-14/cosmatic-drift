topping-verb-add-topping = Add topping
topping-verb-portion-too-small = Portion too small
topping-verb-no-space = No space left
topping-examine = {$format ->
        [one-submerged] With {$lastSubmerged} submerged.
        [many-submerged] With {$listSubmerged} and {$lastSubmerged} submerged.
        [one-topping] With {$lastTopping} as topping.
        [one-topping-one-submerged] With {$lastSubmerged} submerged and {$lastTopping} as topping.
        [one-topping-many-submerged] With {$listSubmerged} and {$lastSubmerged}. As a toping there is {$lastTopping}.
        [many-topping] With {$listTopping} and {$lastTopping} as topping.
        [many-topping-one-submerged] With {$lastSubmerged} submerged. As a topping there is {$listTopping} and {$lastTopping}.
        *[many-topping-many-submerged] With {$listSubmerged} and {$lastSubmerged} submerged. As a topping there is {$listTopping} and {$lastTopping}.
    }
