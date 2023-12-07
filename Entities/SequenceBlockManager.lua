local sequenceBlockManager = {}

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}


sequenceBlockManager = {

    fieldInformation = {
        startWith = {
            fieldType = "integer",
            options = colorNames,
            editable = false
        }
    },

    name = "DzhakeHelper/SequenceBlockManager",
    depth = -100,
    texture = "objects/DzhakeHelper/SequenceBlockManager/idle",
    placements = {
        {
            name = "normal",
            data = {

                startWith = 0,
                everyDash = false,
            },
        },
    },
}




return sequenceBlockManager