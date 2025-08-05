local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local dzhakeHelper = require("mods").requireFromPlugin("libraries.dzhake_helper")
local logging = require("logging")


local sequenceBlock = {}


sequenceBlock.name = "DzhakeHelper/SequenceSwapBlock"
sequenceBlock.minimumSize = {16, 16}
sequenceBlock.nodeLimits = {1, 1}
sequenceBlock.fieldInformation = dzhakeHelper.getSequenceBlockFieldInfo()
sequenceBlock.placements = {}

for i, _ in ipairs(dzhakeHelper.colors) do
    sequenceBlock.placements[i] = {
        name = string.format("sequence_block_%s", i - 1),
        data = dzhakeHelper.getSequenceBlockData(i),
    }

    data = {
        noReturn = false,
        returnTime = 0.8,
        maxForwardSpeedMult = 1,
        maxBackwardSpeedMult = 1,
        onlyStartMoveIfActive = false,
        onlyMoveIfActive = false,
        pathImagePath = "objects/swapblock/target",
        crossImagePath = "objects/DzhakeHelper/sequenceSwapBlock/",
    }
    
    for k,v in pairs(data) do sequenceBlock.placements[i].data[k] = v end
    
    logging.warning(data)
end

function sequenceBlock.sprite(room, entity)
    local sprites = dzhakeHelper.sequenceBlockSprites(room, entity)

    if entity.noReturn then
        local cross = drawableSprite.fromTexture(entity.crossImagePath .. "x", entity)
        cross:addPosition(math.floor(entity.width / 2), math.floor(entity.height / 2))
        
        local color = dzhakeHelper.colors[index + 1] or dzhakeHelper.colors[1]
        if entity.useCustomColor then color = entity.color end
        cross:setColor(color)
        cross.depth = -11

        table.insert(sprites, cross)
    end

    dzhakeHelper.addTrailSprites(sprites, entity, entity.nodes[1].x or entity.x, entity.nodes[1].y or entity.y, entity.pathImagePath)

    return sprites
end


function sequenceBlock.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 8, entity.height or 8

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end



return sequenceBlock