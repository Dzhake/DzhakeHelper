local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local CustomKey = {
    name = "DzhakeHelper/CustomKey",
    depth = -100,
    nodeLimits = {0,2}
}

CustomKey.placements = {
    name = "normal",
    data = {
        particleColor1 = "e2d926",
        particleColor2 = "fffeef",
        color = "e1d417",
        group = 0,
        bubbleReturn = false,
        bubbleReturnDelay = 0.3,
        openAny = false,
        temporary = false,
        getSfx = "event:/game/general/key_get",
        sprite = "objects/DzhakeHelper/customKey/",
    },
}

CustomKey.fieldInformation = {
    color = {
        fieldType = "color"
    },
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    },
    group = {
        fieldType = "integer"
    }
}

function CustomKey.sprite(room, entity)
    local sprites = {}
    local frame = entity.sprite.."idle00"
    local color = entity.color
    local depth = entity.depth

    local sprite = drawableSprite.fromTexture(frame, entity)
    sprite:setColor(color)
    sprite.depth = depth

    table.insert(sprites, sprite)

    return sprites
end


return CustomKey