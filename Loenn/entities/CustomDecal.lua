local spriteBanks = {
    ["Game"] = 0,
    ["Gui"] = 1,
    ["Portraits"] = 2,
    ["Misc"] = 3
}

local CustomDecal = {
    name = "DzhakeHelper/CustomDecal",
    placements = {
        name = "normal",
        data = {
            texture = "_fallback",
            scaleX = 1,
            scaleY = 1,
            rotation = 0,
            color = "FFFFFF",
            depth = 0,
            flags = "",

            pathRoot = 0,
            updateSpriteOnlyIfFlag = true,
            hiRes = false,
            removeDecalsFromPath = true,
        },
    },
    fieldInformation = {
        depth = {
            fieldType = "integer"
        },
        color = {
            fieldType = "color"
        },
        pathRoot = {
            fieldType = "integer",
            editable = false,
            options = spriteBanks
        }
    },
    fieldOrder = {
        "x","y","pathRoot","texture","depth","scaleX","scaleY","rotation","color","flags","hiRes","updateSpriteOnlyIfFlag","removeDecalsFromPath"
    },
    ignoredFields = {
        {"_name", "_id", "originX", "originY", "removeDecalsFromPath"}    
    },
}

function CustomDecal.texture(room, entity)
    if entity.hiRes then return "objects/DzhakeHelper/customHiResDecal/preview" end
    if not entity.removeDecalsFromPath then return "decals/"..entity.texture.."00" end
    return entity.texture.."00"
end

return CustomDecal