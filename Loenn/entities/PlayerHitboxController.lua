local PlayerHitboxController = {
    name = "DzhakeHelper/PlayerHitboxController",
    depth = -100,
	texture = "objects/DzhakeHelper/playerHitboxController/idle",
    fieldOrder = {"x","y","normalHitbox","normalHurtbox","duckHitbox","duckHurtbox","featherHitbox","featherHurtbox",
    "normalHitboxOffset","normalHurtboxOffset","duckHitboxOffset","duckHurtboxOffset","featherHitboxOffset","featherHurtboxOffset"},
    placements = {
        name = "normal",
        data = {
            normalHitbox = "8,11",
            normalHurtbox = "8,9",
            duckHurtbox = "8,4",
            duckHitbox = "8,6",
            featherHitbox = "8,8",
            featherHurtbox = "6,6",

            normalHitboxOffset = "-4,-11",
            normalHurtboxOffset = "-4,-11",
            duckHitboxOffset = "-4,-6",
            duckHurtboxOffset = "-4,-6",
            featherHitboxOffset = "-4,-10",
            featherHurtboxOffset = "-3,-9"
        }
    }
}



return PlayerHitboxController