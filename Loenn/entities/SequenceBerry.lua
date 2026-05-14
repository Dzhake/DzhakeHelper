local drawableSprite = require("structs.drawable_sprite")

local strawberry = {}

local colors = {
    {92 / 255, 91 / 255, 218 / 255},
    {255 / 255, 0 / 255, 81 / 255},
    {215 / 255, 215 / 255, 0 / 255},
    {73 / 255, 220 / 255, 136 / 255},
}

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

strawberry.name = "DzhakeHelper/SequenceBerry"
strawberry.depth = -100
strawberry.fieldInformation = {
    order = {
        fieldType = "integer",
    },
    checkpointID = {
        fieldType = "integer"
    },
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    },
    color = {
        fieldType = "color"
    }
}

function strawberry.sprite(room, entity)
    local sprites = {}
    local texture = ""
    local color = colors[entity.index + 1] or colors[1]
    if entity.useCustomColor then color = entity.color end

    if entity.winged then
        texture = "collectables/strawberry/wings01"
    else
        texture = "collectables/strawberry/normal00"
    end

    local sprite = drawableSprite.fromTexture(texture, entity)
    sprite:setColor(color)
    table.insert(sprites,sprite)

    return sprites

end


strawberry.placements = {
    {
        name = "normal",
        data = {
            winged = false,
            checkpointID = -1,
            order = -1,
            index = 0,
            color = "FFFFFF",
            useCustomColor = false,
            onlyGrabIfActive = true,
            onlyCollectIfActive = true,
            onlyFlyAwayIfActive = true,
            sequenceGroup = 0,
        },
    },
    {
        name = "normal_winged",
        data = {
            winged = true,
            checkpointID = -1,
            order = -1,
            index = 0,
            color = "FFFFFF",
            useCustomColor = false,
            onlyGrabIfActive = true,
            onlyCollectIfActive = true,
            onlyFlyAwayIfActive = true,
            sequenceGroup = 0,
        },
    }
}

return strawberry