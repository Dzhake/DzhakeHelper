local CycleSequenceBlocksTrigger = {
    name = "DzhakeHelper/CycleSequenceBlocksTrigger",
    depth = -100,
    fieldInformation = {
        cyclesCount = {
            fieldType = "integer",
            minimumValue = 1,
            maximumValue = 3,
        },
    },
    placements = {
        {
            name = "normal",
            data = {
                cyclesCount = 1,
            },
        },
    },
}




return CycleSequenceBlocksTrigger