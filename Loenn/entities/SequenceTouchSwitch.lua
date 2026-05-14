local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local SequenceTouchSwitch = {}


local iconsTextures = {
    ["rhombus"] = "objects/DzhakeHelper/sequenceTouchSwitch/icons/rhombus/icon",
    ["rectangle"] = "objects/DzhakeHelper/sequenceTouchSwitch/icons/rectangle/icon",
    ["square"] = "objects/DzhakeHelper/sequenceTouchSwitch/icons/square/icon",
    ["original"] = "objects/touchswitch/icon"
}


local containerTexture = "objects/DzhakeHelper/sequenceTouchSwitch/container/container"

SequenceTouchSwitch.fieldInformation = {
    iconTexture = {
        fieldType = "string",
        options = iconsTextures,
        editable = true
    },
    group = {
        fieldType = "integer",
        minimumValue = 0,
    },
    fieldInformation = {
        sequenceGroup = {
            fieldType = "integer",
        }
    },
}

SequenceTouchSwitch.name = "DzhakeHelper/SequenceTouchSwitch"
SequenceTouchSwitch.depth = 2000
SequenceTouchSwitch.width = 14
SequenceTouchSwitch.height = 14
SequenceTouchSwitch.placements = {
    {
        name = "normal",
        data = {
            group = 1,
            iconTexture = "objects/DzhakeHelper/sequenceTouchSwitch/icons/rhombus/icon",
            groundReset = false,
            sequenceGroup = 0,
        },
    }
}



function SequenceTouchSwitch.sprite(room, entity)
    local containerSprite = drawableSprite.fromTexture(containerTexture, entity)
    local iconSprite = drawableSprite.fromTexture(entity.iconTexture.."00", entity)

    return {containerSprite, iconSprite}
end

function SequenceTouchSwitch.selection(room, entity)
    local x,y = entity.x or 0, entity.y or 0
    return utils.rectangle(x-7,y-7,14,14)
end

return SequenceTouchSwitch