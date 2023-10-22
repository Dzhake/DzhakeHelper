local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")



local cossetteBlock = {}

local colors = {
    {92 / 255, 91 / 255, 218 / 255},
    {255 / 255, 0 / 255, 81 / 255},
    {215 / 255, 215 / 255, 0 / 255},
    {73 / 255, 220 / 255, 136 / 255},
}

local frames = {
    "objects/DzhakeHelper/cossetteBlock/solid",
    "objects/DzhakeHelper/cossetteBlock/solid",
    "objects/DzhakeHelper/cossetteBlock/solid",
    "objects/DzhakeHelper/cossetteBlock/solid",
}

local depths = {
    -10,
    -10,
    -10,
    -10
}

local colorNames = {
    ["Blue"] = 0,
    ["Rose"] = 1,
    ["Bright Sun"] = 2,
    ["Malachite"] = 3
}

cossetteBlock.name = "DzhakeHelper/CossetteBlock"
cossetteBlock.minimumSize = {16, 16}
cossetteBlock.fieldInformation = {
    index = {
        fieldType = "integer",
        options = colorNames,
        editable = false
    }
}
cossetteBlock.placements = {}

for i, _ in ipairs(colors) do
    cossetteBlock.placements[i] = {
        name = string.format("cossette_block_%s", i - 1),
        data = {
            index = i - 1,
            width = 16,
            height = 16,
            blockedByPlayer = true,
            blockedByTheo = true,
        }
    }
end

-- Filter by cassette blocks sharing the same index
local function getSearchPredicate(entity)
    return function(target)
        return entity._name == target._name and entity.index == target.index
    end
end

local function getTileSprite(entity, x, y, frame, color, depth, rectangles)
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

function cossetteBlock.sprite(room, entity)
    local relevantBlocks = utils.filter(getSearchPredicate(entity), room.entities)

    connectedEntities.appendIfMissing(relevantBlocks, entity)

    local rectangles = connectedEntities.getEntityRectangles(relevantBlocks)

    local sprites = {}

    local width, height = entity.width or 32, entity.height or 32
    local tileWidth, tileHeight = math.ceil(width / 8), math.ceil(height / 8)

    local index = entity.index or 0
    local color = colors[index + 1] or colors[1]
    local frame = frames[index + 1] or frames[1]
    local depth = depths[index + 1] or depths[1]

    for x = 1, tileWidth do
        for y = 1, tileHeight do
            local sprite = getTileSprite(entity, x, y, frame, color, depth, rectangles)

            if sprite then
                table.insert(sprites, sprite)
            end
        end
    end

    return sprites
end

return cossetteBlock