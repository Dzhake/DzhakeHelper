utils = require("utils")

local FreezeRefill = {
    name = "DzhakeHelper/FreezeRefill",
    depth = -100,
    texture = "objects/DzhakeHelper/freezeRefill/idle00",
    width = 10,
    height = 10,
    placements = {
        {
            name = "normal",
            data = {

                oneUse = false,
                useAnyway = false,
                staminaBased = true,
                respawnTime = 2.5,
                freezeTime = 0.05,
                delay = 0,
                oldBehaviour = false,
            },
        },
    },
    selection = function(room, entity)
        local x,y = entity.x or 0, entity.y or 0
        return utils.rectangle(x - 4, y - 4, 8,8)
    end
}




return FreezeRefill