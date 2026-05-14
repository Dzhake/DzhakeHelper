local drawableRectangle = require "structs.drawable_rectangle"
local drawableNinePatch = require "structs.drawable_nine_patch"

local colorSwitch = {}

colorSwitch.name = "DzhakeHelper/SequenceSwitchBlock"
colorSwitch.resizable = {true, true}
colorSwitch.minimumSize = {16, 16}
colorSwitch.fieldInformation = {
    sequenceGroup = {
        fieldType = "integer",
    }
}

colorSwitch.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            blue = true,
            rose = true,
            brightSun = true,
            malachite = true,
            useCustomColors = false,
            customColors = "5c5bda,ff0051,ffd700,49dc88",
            sprite = "objects/DzhakeHelper/sequenceSwitchBlock/switch",
            allowsDashOnTop = false,
            sequenceGroup = 0,
        },

    }
}

local nine_patch_options = {
    mode = "border",
    borderMode = "repeat",
    color = {0.5, 0.5, 0.5, 1.0}
}
local bgColor = {40 / 255, 40 / 255, 40 / 255, 1.0}

function colorSwitch.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    return {
        drawableRectangle.fromRectangle("fill", x + 1, y + 1, width - 2, height - 2, bgColor):getDrawableSprite(),
        drawableNinePatch.fromTexture(entity.sprite, nine_patch_options, x, y, width, height)
    }
end

return colorSwitch