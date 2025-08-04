local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")
local matrixLib = require("utils.matrix")
local drawableSprite = require("structs.drawable_sprite")
local connectedEntities = require("helpers.connected_entities")
local dzhakeHelper = require("mods").requireFromPlugin("libraries.dzhake_helper")


local sequenceBlock = {}

sequenceBlock.name = "DzhakeHelper/SequenceBlock"
sequenceBlock.minimumSize = {16, 16}
sequenceBlock.fieldInformation = dzhakeHelper.getSequenceBlockFieldInfo()
sequenceBlock.placements = {}

for i, _ in ipairs(dzhakeHelper.colors) do
    sequenceBlock.placements[i] = {
        name = string.format("sequence_block_%s", i - 1),
        data = dzhakeHelper.getSequenceBlockData(i),
    }
end

function sequenceBlock.sprite(room, entity)
    return dzhakeHelper.sequenceBlockSprites(room, entity)
end

return sequenceBlock