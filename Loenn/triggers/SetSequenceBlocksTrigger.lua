local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

local SetSequenceBlocksTrigger = {
    name = "DzhakeHelper/SetSequenceBlocksTrigger",
    depth = -100,
    fieldInformation = {
        newIndex = {
            fieldType = "integer",
            options = colorNames,
            editable = false
        },
    },
    placements = {
        {
            name = "normal",
            data = {
                newIndex = 0,
                oneUse = true,
            },
        },
    },
}




return SetSequenceBlocksTrigger