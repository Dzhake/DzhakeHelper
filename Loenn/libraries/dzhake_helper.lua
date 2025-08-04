local drawableLine = require("structs.drawable_line")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")
local connectedEntities = require("helpers.connected_entities")

local dzhakeHelper = {}



function dzhakeHelper.getTrailSprites(x, y, nodeX, nodeY, width, height, trailTexture, trailColor, trailDepth)
    local sprites = {}

    local drawWidth, drawHeight = math.abs(x - nodeX) + width, math.abs(y - nodeY) + height
    x, y = math.min(x, nodeX), math.min(y, nodeY)

    local frameNinePatch = drawableNinePatch.fromTexture(trailTexture, trailNinePatchOptions, x, y, drawWidth, drawHeight)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local depth = trailDepth or 8999
    local color = trailColor or {1, 1, 1, 1}
    for _, sprite in ipairs(frameSprites) do
        sprite.depth = depth
        sprite:setColor(color)

        table.insert(sprites, sprite)
    end

    return sprites
end

function dzhakeHelper.addTrailSprites(sprites, x, y, nodeX, nodeY, width, height, trailTexture, trailColor, trailDepth)
    for _, sprite in ipairs(dzhakeHelper.getTrailSprites(x, y, nodeX, nodeY, width, height, trailTexture, trailColor, trailDepth)) do
        table.insert(sprites, sprite)
    end
end

function dzhakeHelper.getTileSprite(entity, x, y, frame, color, depth, rectangles)
    local hasAdjacent = connectedEntities.hasAdjacent

    local drawX, drawY = (x - 1) * 8, (y - 1) * 8

    local closedLeft = hasAdjacent(entity, drawX - 8, drawY, rectangles)
    local closedRight = hasAdjacent(entity, drawX + 8, drawY, rectangles)
    local closedUp = hasAdjacent(entity, drawX, drawY - 8, rectangles)
    local closedDown = hasAdjacent(entity, drawX, drawY + 8, rectangles)
    local completelyClosed = closedLeft and closedRight and closedUp and closedDown

    local quadX, quadY = false, false

    if completelyClosed then
        if not hasAdjacent(entity, drawX + 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 0

        elseif not hasAdjacent(entity, drawX - 8, drawY - 8, rectangles) then
            quadX, quadY = 24, 8

        elseif not hasAdjacent(entity, drawX + 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 16

        elseif not hasAdjacent(entity, drawX - 8, drawY + 8, rectangles) then
            quadX, quadY = 24, 24

        else
            quadX, quadY = 8, 8
        end
    else
        if closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 8, 0

        elseif closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 8, 16

        elseif closedLeft and not closedRight and closedUp and closedDown then
            quadX, quadY = 16, 8

        elseif not closedLeft and closedRight and closedUp and closedDown then
            quadX, quadY = 0, 8

        elseif closedLeft and not closedRight and not closedUp and closedDown then
            quadX, quadY = 16, 0

        elseif not closedLeft and closedRight and not closedUp and closedDown then
            quadX, quadY = 0, 0

        elseif not closedLeft and closedRight and closedUp and not closedDown then
            quadX, quadY = 0, 16

        elseif closedLeft and not closedRight and closedUp and not closedDown then
            quadX, quadY = 16, 16
        end
    end

    if quadX and quadY then
        local sprite = drawableSprite.fromTexture(frame, entity)

        sprite:addPosition(drawX, drawY)
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite:setColor(color)

        sprite.depth = depth

        return sprite
    end
end


function dzhakeHelper.getSequenceBlocksSearchPredicate(entity)
    return function(target)
        return entity.blendIndex == target.blendIndex
    end
end


dzhakeHelper.colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

dzhakeHelper.colors = {
    {92 / 255, 91 / 255, 218 / 255},
    {255 / 255, 0 / 255, 81 / 255},
    {215 / 255, 215 / 255, 0 / 255},
    {73 / 255, 220 / 255, 136 / 255},
}

function dzhakeHelper.getSequenceBlockData(i)
    return {
        index = i - 1,
        blendIndex = i - 1,
        blendIndexEqualsColorIndex = true,
        width = 16,
        height = 16,
        blockedByPlayer = true,
        blockedByTheo = true,
        useCustomColor = false,
        color = "ffffff",
        imagePath = "objects/DzhakeHelper/sequenceBlock/",
        backgroundBlock = true,
    }
end

function dzhakeHelper.getSequenceBlockFieldInfo()
    return {
        index = {
            fieldType = "integer",
            options = dzhakeHelper.colorNames,
            editable = false,
        },
        blendIndex = {
            fieldType = "integer",
        },
        color = {
            fieldType = "color",
        },
    }
end

function dzhakeHelper.sequenceBlockSprites(room, entity)
    local relevantBlocks = utils.filter(dzhakeHelper.getSequenceBlocksSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local index = entity.index or 0
    local color = dzhakeHelper.colors[index + 1] or dzhakeHelper.colors[1]
    if entity.useCustomColor then color = entity.color end
    local frame = entity.imagePath.."solid" or "objects/DzhakeHelper/sequenceBlock/solid"
    local depth = -10

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = dzhakeHelper.getTileSprite(entity, x, y, frame, color, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return dzhakeHelper