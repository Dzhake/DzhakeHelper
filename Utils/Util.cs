using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Monocle;
using Celeste.Mod.DzhakeHelper.Entities;

namespace Celeste.Mod.DzhakeHelper;


// This file is not really by me :p
public static class Util
{
    public static Random random = new Random(Guid.NewGuid().GetHashCode());
    public static void Log(LogLevel logLevel, string str)
    {
        Logger.Log(logLevel, "Dzhake Helper", str);
    }

    public static void Log(string str)
    {
        Log(LogLevel.Warn, str);
    }

    public static bool TryGetPlayer(out Player player)
    {
        player = Engine.Scene?.Tracker?.GetEntity<Player>();
        return player != null;
    }

    public static Level GetLevel()
    {
        return Engine.Scene as Level;
    }

    private static readonly PropertyInfo[] namedColors = typeof(Color).GetProperties();

    public static Color CopyColor(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, (byte)alpha * 255);
    }

    public static Color CopyColor(Color color, int alpha)
    {
        return new Color(color.R, color.G, color.B, alpha);
    }

    public static Color ColorArrayLerp(float lerp, params Color[] colors)
    {
        float m = Mod(lerp, colors.Length);
        int fromIndex = (int)Math.Floor(m);
        int toIndex = Mod(fromIndex + 1, colors.Length);
        float clampedLerp = m - fromIndex;

        return Color.Lerp(colors[fromIndex], colors[toIndex], clampedLerp);
    }

    public static Color TryParseColor(string str, float alpha = 1f)
    {
        foreach (PropertyInfo prop in namedColors)
        {
            if (str.Equals(prop.Name))
            {
                return CopyColor((Color)prop.GetValue(null), alpha);
            }
        }
        return CopyColor(Calc.HexToColor(str.Trim('#')), alpha);
    }

    public static int ToInt(bool b)
    {
        return b ? 1 : 0;
    }

    public static int ToBitFlag(params bool[] b)
    {
        int ret = 0;
        for (int i = 0; i < b.Length; i++)
            ret |= ToInt(b[i]) << i;
        return ret;
    }

    public static float Mod(float x, float m)
    {
        return ((x % m) + m) % m;
    }

    public static int Mod(int x, int m)
    {
        return ((x % m) + m) % m;
    }

    public static Vector2 RandomDir(float length)
    {
        return Calc.AngleToVector(Calc.Random.NextAngle(), length);
    }

    public static string StrTrim(string str)
    {
        return str.Trim();
    }

    public static Vector2 Min(Vector2 a, Vector2 b)
    {
        return new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    public static Vector2 Max(Vector2 a, Vector2 b)
    {
        return new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    public static Rectangle Rectangle(Vector2 a, Vector2 b)
    {
        Vector2 min = Min(a, b);
        Vector2 size = Max(a, b) - min;
        return new((int)min.X, (int)min.Y, (int)size.X, (int)size.Y);
    }

    /// <summary>
    /// Triangle wave function.
    /// </summary>
    public static float TriangleWave(float x)
    {
        return (2 * Math.Abs(Mod(x, 2) - 1)) - 1;
    }

    /// <summary>
    /// Triangle wave between mapped between two values.
    /// </summary>
    /// <param name="x">The input value.</param>
    /// <param name="from">The ouput when <c>x</c> is an even integer.</param>
    /// <param name="to">The output when <c>x</c> is an odd integer.</param>
    public static float MappedTriangleWave(float x, float from, float to)
    {
        return ((from - to) * Math.Abs(Mod(x, 2) - 1)) + to;
    }

    public static float PowerBounce(float x, float p)
    {
        return -(float)Math.Pow(Math.Abs(2 * (Mod(x, 1) - .5f)), p) + 1;
    }

    public static bool Blink(float time, float duration)
    {
        return time % (duration * 2) < duration;
    }

    /// <summary>
    /// Checks if two line segments are intersecting.
    /// </summary>
    /// <param name="p0">The first end of the first line segment.</param>
    /// <param name="p1">The second end of the first line segment.</param>
    /// <param name="p2">The first end of the second line segment.</param>
    /// <param name="p3">The second end of the second line segment.</param>
    /// <returns>The result of the intersection check.</returns>
    public static bool SegmentIntersection(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float sax = p1.X - p0.X; float say = p1.Y - p0.Y;
        float sbx = p3.X - p2.X; float sby = p3.Y - p2.Y;

        float s = (-say * (p0.X - p2.X) + sax * (p0.Y - p2.Y)) / (-sbx * say + sax * sby);
        float t = (sbx * (p0.Y - p2.Y) - sby * (p0.X - p2.X)) / (-sbx * say + sax * sby);

        return s is >= 0 and <= 1
            && t is >= 0 and <= 1;
    }

    public static ColliderList GenerateColliderGrid(bool[,] tilemap)
    {
        bool[,] copy = tilemap.Clone() as bool[,];

        ColliderList colliders = new();

        int sx = copy.GetLength(0), sy = copy.GetLength(1);
        for (int x = 0; x < sx; x++)
        {
            List<Hitbox> prevColliders = new();
            Hitbox currentPrevCollider = null;
            for (int y = 0; y <= sy; y++)
            {
                if (y == sy)
                {
                    if (currentPrevCollider is not null)
                        prevColliders.Add(currentPrevCollider);
                    break;
                }

                // basic vertical expansion of the colliders.
                if (copy[x, y])
                {
                    copy[x, y] = false;

                    if (currentPrevCollider == null)
                        currentPrevCollider = new Hitbox(8, 8, x * 8, y * 8);
                    else
                        currentPrevCollider.Height += 8;

                }
                else if (currentPrevCollider != null)
                {
                    prevColliders.Add((Hitbox)currentPrevCollider.Clone());
                    currentPrevCollider = null;
                }
            }

            // once we are done with them, we can extend them horizontally to the right as much as possible.
            foreach (Hitbox prevCollider in prevColliders)
            {
                int cx = (int)prevCollider.Position.X / 8;
                int cy = (int)prevCollider.Position.Y / 8;
                int cw = (int)prevCollider.Width / 8;
                int ch = (int)prevCollider.Height / 8;

                while (cx + cw < sx)
                {
                    bool canExtend = true;

                    for (int j = cy; j < cy + ch; j++)
                    {
                        if (!copy[cx + cw, j])
                        {
                            canExtend = false;
                            break;
                        }
                    }

                    if (canExtend)
                    {
                        for (int j = cy; j < cy + ch; j++)
                        {
                            copy[cx + cw, j] = false;
                        }
                        prevCollider.Width += 8;
                        cw++;
                    }
                    else break;
                }

                colliders.Add(prevCollider);
            }
        }

        return colliders.colliders.Length > 0 ? colliders : null;
    }

    public static IEnumerator Interpolate(float duration, Action<float> action)
    {
        float t = duration;
        while (t > 0.0f)
        {
            action(1 - t / duration);
            t = Calc.Approach(t, 0.0f, Engine.DeltaTime);
            yield return null;
        }
        action(1.0f);
    }



    // Hi, no, i didn't just copypaste this

    public enum DirectionEnum
    {
        Right, BottomRight,
        Bottom, BottomLeft,
        Left, TopLeft,
        Top, TopRight
    }

    public static Dictionary<DirectionEnum,Vector2> Directions = new() {
        { DirectionEnum.Right, new Vector2(1f, 0f) },
        { DirectionEnum.Left, new Vector2(-1f, 0f) },
        { DirectionEnum.Bottom, new Vector2(0f, 1f) },
        { DirectionEnum.Top, new Vector2(0f, -1f) },
        { DirectionEnum.BottomRight, new Vector2(1f, 1f) },
        { DirectionEnum.BottomLeft, new Vector2(-1f, 1f) },
        { DirectionEnum.TopRight, new Vector2(1f, -1f) },
        { DirectionEnum.TopLeft, new Vector2(-1f, -1f) },
    };

    public static Dictionary<int,Color> DefaultSequenceColors = new() {
        {0,Calc.HexToColor("5c5bda") },
        {1,Calc.HexToColor("ff0051") },
        {2,Calc.HexToColor("ffd700") },
        {3,Calc.HexToColor("49dc88") },
    };



    public static void CycleSequenceColor(int times = 1)
    {
        SequenceBlockManager manager = Engine.Scene.Tracker.GetEntity<SequenceBlockManager>();
        manager?.CycleSequenceBlocks(times);
    }

    public static void SetSequenceColor(int index)
    {
        SequenceBlockManager manager = Engine.Scene.Tracker.GetEntity<SequenceBlockManager>();
        manager?.SetSequenceBlocks(index);
    }


    // https://github.com/Viv-0/VivHelper/blob/master/_Code/Module%2C%20Extensions%2C%20Etc/VivHelperModule.cs#L907
    public static bool ParseFlags(Level l, string[] flags, string and_or = "and")
    {
        if (l == null)
            return false;
        bool b = and_or == "and";
        if (flags == null || flags.Length == 0 || (flags.Length == 1 && flags[0] == ""))
            return true;
        foreach (string flag in flags)
        {
            if (and_or == "or") { b |= flag[0] != '!' ? l.Session.GetFlag(flag) : !l.Session.GetFlag(flag.TrimStart('!')); } else { b &= flag[0] != '!' ? l.Session.GetFlag(flag) : !l.Session.GetFlag(flag.TrimStart('!')); }
        }
        return b;
    }

    public static bool ParseFlags(Level l,string flags, string and_or = "and")
    {
        return ParseFlags(l, flags.Split(','),and_or);
    }


    public static EntityData GenerateEntityData(Dictionary<string,object> data)
    {
        EntityData entityData = new EntityData();

        if (data["_x"] != null) entityData.Position.X = (float)data["_x"];
        if (data["_y"] != null)  entityData.Position.Y = (float)data["_y"];

        if (data["_name"] != null) entityData.Name = (string)data["_name"];
        if (data["_width"] != null) entityData.Width = (int)data["_width"];
        if (data["_height"] != null) entityData.Height = (int)data["_height"];
        if (data["_nodes"] != null)
        {
            entityData.Nodes = (Vector2[])data["_nodes"];
        }
        if (data["_id"] != null)
        {
            entityData.ID = (int)data["_id"];
        }
        else
        {
            entityData.ID = Util.random.Next(500, 9999);
        }

        entityData.Level = Util.GetLevel().Session.LevelData;

        foreach (KeyValuePair<string,object> kvp in data)
        {
            if (kvp.Key is "_x" or "_y" or "_id" or "_name" or "_nodes" or "_width" or "_height") continue;
            entityData.Values[kvp.Key] = kvp.Value;
        }

        return entityData;
    }
}