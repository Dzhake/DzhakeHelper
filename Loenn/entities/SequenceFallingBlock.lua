local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local dzhakeHelper = require("mods").requireFromPlugin("libraries.dzhake_helper")


local sequenceBlock = {}

sequenceBlock.name = "DzhakeHelper/SequenceFallingBlock"
sequenceBlock.minimumSize = {16, 16}
sequenceBlock.fieldInformation = dzhakeHelper.getSequenceBlockFieldInfo()
sequenceBlock.placements = {}

for i, _ in ipairs(dzhakeHelper.colors) do
    sequenceBlock.placements[i] = {
        name = string.format("sequence_block_%s", i - 1),
        data = dzhakeHelper.getSequenceBlockData(i),
    }

    data = {    
        fallDelay = 0.4,
        maxSpeed = 160,
        speedMultiplier = 1,
    }

    for k,v in pairs(data) do sequenceBlock.placements[i].data[k] = v end
end

function sequenceBlock.sprite(room, entity)
    return dzhakeHelper.sequenceBlockSprites(room, entity)
end

return sequenceBlock