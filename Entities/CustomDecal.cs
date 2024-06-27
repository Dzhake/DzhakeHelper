using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.IO;
using System.Text.RegularExpressions;

namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/CustomDecal")]
    public class CustomDecal : Decal
    {

        // -_-
        private static float camScale = 6f;
        private static Vector2 entityOffset = new(12f, 12f);

        public enum SpriteBank
        {
            Game, Gui, Portraits, Misc
        };
        public SpriteBank PathRoot;

        public string Flags;
        public bool UpdateSpriteOnlyIfFlag;

        public float Delay;

        public Vector2 shakeOffset;

        public bool HD;
        public bool RemoveDecalsFromPath;

        public static Entity Load(EntityData data, Vector2 offset)
        {
            return IsOldVersion(data)
#pragma warning disable CS0618 // yes, i know, shut up vs
              ? new ObsoleteCustomDecal(data, offset)
#pragma warning restore CS0618
              : new CustomDecal(data, offset);
        }

        public CustomDecal(EntityData data, Vector2 offset) : this(data.Position, offset, data.Attr("texture"),
            data.Int("depth"), data.HexColorWithAlpha("color"), new Vector2(data.Float("scaleX"),
                data.Float("scaleY")), data.Float("rotation"), data.Attr("flags"),
            data.Bool("updateSpriteOnlyIfFlag"), data.Bool("hiRes"), data.Enum("pathRoot", SpriteBank.Game),
            data.Bool("removeDecalsFromPath"))
        {}


        public CustomDecal(Vector2 position,Vector2 offset, string texture, int depth, Color color, Vector2 scale, float rotation, string flag,
            bool updateSpriteOnlyIfFlag, bool hd, SpriteBank PathRoot, bool removeDecalsFromPath) 
           : base("generic/algae_a",position + offset, scale, depth, rotation, color)
        {
            RemoveDecalsFromPath = removeDecalsFromPath;
            this.PathRoot = PathRoot;
            HD = hd;
            if (HD)
            {
                Tag = TagsExt.SubHUD;
            }

            Flags = flag;
            UpdateSpriteOnlyIfFlag = updateSpriteOnlyIfFlag;

            Atlas atlas = PathRoot switch
            {
                SpriteBank.Game => GFX.Game,
                SpriteBank.Gui => GFX.Gui,
                SpriteBank.Portraits => GFX.Portraits,
                SpriteBank.Misc => GFX.Misc,
                _ => GFX.Game,
            };

            string extension = Path.GetExtension(texture);
            if (!string.IsNullOrEmpty(extension)) texture = texture.Replace(extension, "");
            string input = texture.Replace('\\', '/');

            if (!removeDecalsFromPath)
            {
                input = Path.Combine("decals", input);
            }

            Name = Regex.Replace(input, "\\d+$", string.Empty);
            textures = atlas.GetAtlasSubtextures(Name);
        }

        public override void Update()
        {
            if (UpdateSpriteOnlyIfFlag)
            {
                if (!FlagIsTrue())
                {
                    frame -= AnimationSpeed * Engine.DeltaTime;
                    if (frame < 0) frame = textures.Count; 
                }
            }
            base.Update();
        }

        public override void Render()
        {
            if (FlagIsTrue())
            {
                Vector2 oldPos = Position;
                if (HD)
                {
                    Camera cam = SceneAs<Level>().Camera;
                    Vector2 displayPos = camScale * (this.Position - cam.Position) + camScale * entityOffset;
                    Position = displayPos;
                }
                
                base.Render();
                Position = oldPos;
            }
        }

        public bool FlagIsTrue()
        {
            return Flags == "" || Util.ParseFlags(base.Scene as Level,Flags);
        }

        public static bool IsOldVersion(EntityData data)
        {
            return data.Has("imagePath");
        }
    }
}
