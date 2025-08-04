utils = require("utils")

local VoidLift = {
    name = "DzhakeHelper/VoidLift",
    depth = 100,
    placements = {
        name = "normal",
        data = {
            newSpeed = -300,
            refillDash = false,
            dashToLeft = true,
            dashToRight = true,
            dashToTop = false,
            dashToBottom = false,
            dashToBottomRight = false,
            dashToBottomLeft = false,
            dashToTopRight = false,
            dashToTopLeft = false,
            setSpeedxToZero = true,
            sprite = "objects/DzhakeHelper/voidLift/",
            sfx = "",
            respawnTime = 0.1,
        },
    },
    selection = function(room, entity)
        local x,y = entity.x or 0, entity.y or 0
        return utils.rectangle(x - 7, y - 7, 14,14)
    end,
    texture = function(room, entity)
        return entity.sprite.."idle00"
    end
}




return VoidLift