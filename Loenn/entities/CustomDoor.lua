local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local CustomDoor = {
    name = "DzhakeHelper/CustomDoor",
    depth = -100,
}

CustomDoor.placements = {
    name = "normal",
    data = {
        color = "b86f50",
        group = 0,
        openedByAny = false,
        particleColor1 = "FF3D63",
        particleColor2 = "FF75DE",
        unlockSfx = "event:/game/03_resort/key_unlock",
        sprite = "objects/DzhakeHelper/customDoor/"
    },
}

CustomDoor.fieldInformation = {
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

function CustomDoor.sprite(room, entity)
    local sprites = {}
    local frame = entity.sprite.."lockdoor00"
    local color = entity.color
    local depth = entity.depth

    local sprite = drawableSprite.fromTexture(frame, entity)
    sprite:setColor(color)
    sprite.depth = depth
    sprite:addPosition(16,16)

    table.insert(sprites, sprite)

    return sprites
end

function CustomDoor.selection(room, entity)
    local x,y = entity.x or 0, entity.y or 0
    return utils.rectangle(x, y, 32,32)
end


return CustomDoor