local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local dzhakeHelper = require("mods").requireFromPlugin("libraries.dzhake_helper")

local sequenceZipMover = {}

local ropeColors = {
    {110 / 255, 189 / 255, 245 / 255, 1.0},
    {194 / 255, 116 / 255, 171 / 255, 1.0},
    {227 / 255, 214 / 255, 148 / 255, 1.0},
    {128 / 255, 224 / 255, 141 / 255, 1.0}
}

sequenceZipMover.name = "DzhakeHelper/SequenceZipMover"
sequenceZipMover.minimumSize = {16, 16}
sequenceZipMover.nodeLimits = {1, -1}
sequenceZipMover.nodeVisibility = "never"
sequenceZipMover.fieldInformation = dzhakeHelper.getSequenceBlockFieldInfo()

sequenceZipMover.placements = {}
for i, _ in ipairs(dzhakeHelper.colors) do
    sequenceZipMover.placements[i] = {
        name = string.format("sequence_zipMover_%s", i - 1),
        data = dzhakeHelper.getSequenceBlockData(i),
    }
    
    data = {
        backgroundBlock = true,
        delayBetweenNodes = 0.5,
        goBackByNodes = false,
    }

    for k,v in pairs(data) do sequenceZipMover.placements[i].data[k] = v end
end


function sequenceZipMover.sprite(room, entity)
    local sprites = dzhakeHelper.sequenceBlockSprites(room, entity)
    
    local i = entity.index or 0
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 32, entity.height or 32
    local halfWidth, halfHeight = math.floor(width / 2), math.floor(height / 2)
    local centerX, centerY = x + halfWidth, y + halfHeight

    local ropeColor = dzhakeHelper.colors[i + 1]

    local nodes = entity.nodes or {{x = 0, y = 0}}

    local nodeSprites = {}


    local cx, cy = centerX, centerY
    for _, node in ipairs(nodes) do
        local centerNodeX, centerNodeY = node.x + halfWidth, node.y + halfHeight

        local nodeCogSprite = drawableSprite.fromTexture("objects/DzhakeHelper/sequenceZipMover/cog")
        nodeCogSprite:setColor(color)

        nodeCogSprite:setPosition(centerNodeX, centerNodeY)
        nodeCogSprite:setJustification(0.5, 0.5)

        local points = {cx, cy, centerNodeX, centerNodeY}
        local leftLine = drawableLine.fromPoints(points, ropeColor, 1)
        local rightLine = drawableLine.fromPoints(points, ropeColor, 1)

        leftLine:setOffset(0, 4.5)
        rightLine:setOffset(0, -4.5)

        leftLine.depth = 5000
        rightLine.depth = 5000

        for _, sprite in ipairs(leftLine:getDrawableSprite()) do
            table.insert(sprites, sprite)
        end

        for _, sprite in ipairs(rightLine:getDrawableSprite()) do
            table.insert(sprites, sprite)
        end

        table.insert(sprites, nodeCogSprite)

        cx, cy = centerNodeX, centerNodeY
    end


    return sprites
end

function sequenceZipMover.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local mainRectangle = utils.rectangle(x, y, width, height)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeRectangles = {}
    for _, node in ipairs(nodes) do
        local centerNodeX, centerNodeY = node.x + halfWidth, node.y + halfHeight

        table.insert(nodeRectangles, utils.rectangle(centerNodeX - 5, centerNodeY - 5, 10, 10))
    end

    return mainRectangle, nodeRectangles
end

return sequenceZipMover