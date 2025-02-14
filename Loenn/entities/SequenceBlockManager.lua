local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}


local sequenceBlockManager = {

    fieldInformation = {
        startWith = {
            fieldType = "integer",
            options = colorNames,
            editable = false
        }
    },

    name = "DzhakeHelper/SequenceBlockManager",
    depth = -100,
    texture = "objects/DzhakeHelper/sequenceBlockManager/idle",
    placements = {
        {
            name = "normal",
            data = {
                startWith = 0,
                everyDash = false,
                flag = "",
                cycleWhenFlagOn = false,
                cycleWhenFlagOff = false
            },
        },
    },
}




return sequenceBlockManager