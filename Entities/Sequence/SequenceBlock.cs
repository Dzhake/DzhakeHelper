using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.DzhakeHelper.Entities
{
    [CustomEntity("DzhakeHelper/SequenceBlock")]
    [Tracked(true)]
    public class SequenceBlock : Solid
    {
        private class BoxSide(SequenceBlock block, Color color) : Entity
        {
            public override void Render()
            {
                if (block.BackgroundBlock) Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.blockHeight, color);
            }
        }

        public int Index;

        public int BlendIndex;
        public bool BlendIndexEqualsColorIndex;

        public bool BlockedByPlayer;

        public bool BlockedByTheo;

        //public bool BlockedByHoldables;

        public string ImagePath;

        public bool UseCustomColor = false;


        public float Tempo;

        public bool Activated;

        public EntityID ID;

        public int blockHeight = 2;

        public List<SequenceBlock>? group;

        private bool groupLeader;

        private Vector2 groupOrigin;

        public Color color;

        public Color PressedColor;

        private List<Image> pressed = new List<Image>();

        private List<Image> solid = new List<Image>();

        private List<Image> all = new List<Image>();

        private LightOcclude occluder;

        private Wiggler wiggler;

        private Vector2 wigglerScaler;

        private BoxSide side;

        public bool BackgroundBlock;

        protected Vector2 blockOffset => Vector2.UnitY * (2 - blockHeight);
        public int SolidDepth;
        public int PressedDepth;


        public SequenceBlock(EntityData data, Vector2 offset, EntityID id)
            : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            SurfaceSoundIndex = 35;
            Collidable = false;
            ID = id;
            Index = data.Int("index");
            BlendIndexEqualsColorIndex = data.Bool("blendIndexEqualsColorIndex", true);
            BlendIndex = BlendIndexEqualsColorIndex ? Index : data.Int("blendIndex");
            PressedDepth = data.Int("pressedDepth", 8990);
            SolidDepth = data.Int("solidDepth", -10);
            
            BlockedByPlayer = data.Bool("blockedByPlayer");
            ImagePath = data.Attr("imagePath", "objects/DzhakeHelper/sequenceBlock/");
            BackgroundBlock = data.Bool("backgroundBlock", true);
            BlockedByTheo = data.Bool("blockedByTheo");
            //BlockedByHoldables = data.Bool("blockedByHoldables");
            UseCustomColor = data.Bool("useCustomColor");
            if (UseCustomColor)
                color = data.HexColorWithAlpha("color");
            else
                switch (Index)
                {
                default:
                    color = Calc.HexToColor("5c5bda");
                    break;
                case 1:
                    color = Calc.HexToColor("ff0051");
                    break;
                case 2:
                    color = Calc.HexToColor("ffd700");
                    break;
                case 3:
                    color = Calc.HexToColor("49dc88");
                    break;
                }

            Add(occluder = new LightOcclude());

            Color c = Calc.HexToColor("667da5");
            PressedColor = new Color(c.R / 255f * (color.R / 255f), c.G / 255f * (color.G / 255f), c.B / 255f * (color.B / 255f), 1f);
        }


        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(side = new BoxSide(this, PressedColor));
            foreach (StaticMover staticMover in staticMovers)
                switch (staticMover.Entity)
                {
                case Spikes spikes:
                    spikes.EnabledColor = this.color;
                    spikes.DisabledColor = PressedColor;
                    spikes.VisibleWhenDisabled = true;
                    spikes.SetSpikeColor(this.color);
                    break;
                case Spring spring:
                    spring.DisabledColor = PressedColor;
                    spring.VisibleWhenDisabled = true;
                    break;
                }

            if (group == null)
            {
                groupLeader = true;
                group = new List<SequenceBlock>
                {
                    this
                };
                FindInGroup(this);
                float minX = float.MaxValue;
                float maxX = float.MinValue;
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                foreach (SequenceBlock item in group)
                {
                    if (item.Left < minX) minX = item.Left;
                    if (item.Right > maxX) maxX = item.Right;
                    if (item.Bottom > maxY) maxY = item.Bottom;
                    if (item.Top < minY) minY = item.Top;
                }

                groupOrigin = new Vector2((int)(minX + (maxX - minX) / 2f), (int)maxY);
                wigglerScaler = new Vector2(Calc.ClampedMap(maxX - minX, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(maxY - minY, 32f, 96f, 1f, 0.2f));
                Add(wiggler = Wiggler.Create(0.3f, 3f));
                foreach (SequenceBlock item2 in group)
                {
                    item2.wiggler = wiggler;
                    item2.wigglerScaler = wigglerScaler;
                    item2.groupOrigin = groupOrigin;
                }
            }

            foreach (StaticMover staticMover2 in staticMovers)
                if (staticMover2.Entity is Spikes spikes2)
                    spikes2.SetOrigins(groupOrigin);

            for (float tileX = base.Left; tileX < base.Right; tileX += 8f)
            {
                for (float tileY = base.Top; tileY < base.Bottom; tileY += 8f)
                {
                    bool leftCollide = CheckForSame(tileX - 8f, tileY);
                    bool rightCollide = CheckForSame(tileX + 8f, tileY);
                    bool topCollide = CheckForSame(tileX, tileY - 8f);
                    bool bottomCollide = CheckForSame(tileX, tileY + 8f);
                    switch (leftCollide)
                    {
                    case true when (rightCollide && topCollide && bottomCollide):
                    {
                        if (!CheckForSame(tileX + 8f, tileY - 8f))
                            SetImage(tileX, tileY, 3, 0);
                        else if (!CheckForSame(tileX - 8f, tileY - 8f))
                            SetImage(tileX, tileY, 3, 1);
                        else if (!CheckForSame(tileX + 8f, tileY + 8f))
                            SetImage(tileX, tileY, 3, 2);
                        else if (!CheckForSame(tileX - 8f, tileY + 8f))
                            SetImage(tileX, tileY, 3, 3);
                        else
                            SetImage(tileX, tileY, 1, 1);
                        break;
                    }
                    case true when (rightCollide && !topCollide && bottomCollide):
                        SetImage(tileX, tileY, 1, 0);
                        break;
                    case true when (rightCollide && topCollide && !bottomCollide):
                        SetImage(tileX, tileY, 1, 2);
                        break;
                    case true when (!rightCollide && topCollide && bottomCollide):
                        SetImage(tileX, tileY, 2, 1);
                        break;
                    case false when (rightCollide && topCollide && bottomCollide):
                        SetImage(tileX, tileY, 0, 1);
                        break;
                    case true when (!rightCollide && !topCollide && bottomCollide):
                        SetImage(tileX, tileY, 2, 0);
                        break;
                    case false when (rightCollide && !topCollide && bottomCollide):
                        SetImage(tileX, tileY, 0, 0);
                        break;
                    case true when (!rightCollide && topCollide && !bottomCollide):
                        SetImage(tileX, tileY, 2, 2);
                        break;
                    case false when (rightCollide && topCollide && !bottomCollide):
                        SetImage(tileX, tileY, 0, 2);
                        break;
                    }
                }
            }

            if (!Collidable)
                DisableStaticMovers();
            UpdateVisualState();
        }

        private void FindInGroup(SequenceBlock block)
        {
            if (group is null) return;
            foreach (SequenceBlock entity in Scene.Tracker.GetEntities<SequenceBlock>())
            {
                if (entity == this || entity == block || entity.BlendIndex != BlendIndex || (!entity.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) && !entity.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) || group.Contains(entity)) continue;
                
                group.Add(entity);
                entity.group = group;
                FindInGroup(entity);
            }
        }

        private bool CheckForSame(float x, float y)
        {
            foreach (SequenceBlock entity in Scene.Tracker.GetEntities<SequenceBlock>())
                if (entity.BlendIndex == BlendIndex && entity.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
                    return true;

            return false;
        }

        private void SetImage(float x, float y, int tx, int ty)
        {
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ImagePath + "pressed");
            pressed.Add(CreateImage(x, y, tx, ty, atlasSubtextures[Index % atlasSubtextures.Count]));
            solid.Add(CreateImage(x, y, tx, ty, GFX.Game[ImagePath + "solid"]));
        }

        private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
        {
            Vector2 vector = new Vector2(x - base.X, y - base.Y);
            Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
            Vector2 vector2 = groupOrigin - Position;
            image.Origin = vector2 - vector;
            image.Position = vector2;
            image.Color = color;
            Add(image);
            all.Add(image);
            return image;
        }

        public override void Update()
        {
            base.Update();

            if (groupLeader && Activated && !Collidable)
            {
                if (group is null) return;
                bool blocked = false;
                foreach (SequenceBlock item in group)
                    if (item.BlockedCheck())
                    {
                        blocked = true;
                        break;
                    }

                if (!blocked)
                {
                    foreach (SequenceBlock item2 in group)
                    {
                        item2.Collidable = true;
                        item2.EnableStaticMovers();
                        item2.ShiftSize(-1);
                    }

                    wiggler.Start();
                }
            }
            else if (!Activated && Collidable)
            {
                ShiftSize(1);
                Collidable = false;
                DisableStaticMovers();
            }

            UpdateVisualState();
        }

        public bool BlockedCheck()
        {
            if (Scene is null) return true;
            TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
            if (BlockedByTheo && theoCrystal != null && !TryActorWiggleUp(theoCrystal))
                return true;
            Player player = CollideFirst<Player>();
            if (BlockedByPlayer && player != null && !TryActorWiggleUp(player))
                return true;
            return false;
        }

        private void UpdateVisualState()
        {
            if (!Collidable) Depth = PressedDepth;
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>(); //oh my god what I might need to change this one day.
                if (entity != null && entity.Top >= Bottom - 1f) Depth = SolidDepth + 20;
                else Depth = SolidDepth;
            }

            foreach (StaticMover staticMover in staticMovers) staticMover.Entity.Depth = Depth + 1;

            side.Depth = Depth + 5;
            side.Visible = blockHeight > 0;
            occluder.Visible = Collidable;
            foreach (Image item in solid) item.Visible = Collidable;

            foreach (Image item2 in pressed) item2.Visible = !Collidable;

            if (!groupLeader) return;

            Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
            foreach (SequenceBlock item3 in group)
            {
                foreach (Image item4 in item3.all) item4.Scale = scale;

                foreach (StaticMover staticMover2 in item3.staticMovers)
                {
                    if (staticMover2.Entity is not Spikes spikes) continue;
                    foreach (Component component in spikes.Components)
                        if (component is Image image)
                            image.Scale = scale;
                }
            }
        }


        private void ShiftSize(int amount)
        {
            MoveV(amount);
            blockHeight -= amount;
        }

        private bool TryActorWiggleUp(Entity actor)
        {
            foreach (SequenceBlock item in group)
                if (item != this && item.CollideCheck(actor, item.Position + Vector2.UnitY * 4f))
                    return false;

            bool collidable = Collidable;
            Collidable = true;
            for (int i = 1; i <= 4; i++)
                if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
                {
                    actor.Position -= Vector2.UnitY * i;
                    Collidable = collidable;
                    return true;
                }

            Collidable = collidable;
            return false;
        }

        public SequenceBlock(EntityData data, Vector2 offset)
            : this(data, offset, new EntityID(data.Level.Name, data.ID))
        {
        }

        protected void AddCenterSymbol(Image solid, Image pressed)
        {
            this.solid.Add(solid);
            this.pressed.Add(pressed);
            List<Image> all = this.all;
            Vector2 origin = groupOrigin - Position;
            Vector2 size = new(Width, Height);

            Vector2 half = (size - new Vector2(solid.Width, solid.Height)) * 0.5f;
            solid.Origin = origin - half;
            solid.Position = origin;
            solid.Color = color;
            Add(solid);
            all.Add(solid);

            half = (size - new Vector2(pressed.Width, pressed.Height)) * 0.5f;
            pressed.Origin = origin - half;
            pressed.Position = origin;
            pressed.Color = color;
            Add(pressed);
            all.Add(pressed);
        }
    }
}