using System;
using Celeste.Mod.DzhakeHelper.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using System.ComponentModel;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.DzhakeHelper;

public static class Extensions
{
    public static Color Mult(this Color color, Color other)
    {
        color.R = (byte)(color.R * other.R / 256f);
        color.G = (byte)(color.G * other.G / 256f);
        color.B = (byte)(color.B * other.B / 256f);
        color.A = (byte)(color.A * other.A / 256f);
        return color;
    }

    public static Color Mix(this Color color, Color other, float secondPower)  
    {
        secondPower = secondPower / 10; // somehow this works i have no clue why and how
        float firstPower = 1 - secondPower;
        if (secondPower < 0 || secondPower > 0.1) // if broken
        {
            Logger.Log(LogLevel.Debug,"DzhakeHelper/Extensions/Mix()",$"Variable 'second power' is ${secondPower}, but it should be between 0 and 1");
            return color;
        }
        color.R = (byte)((color.R * firstPower) + (other.R * secondPower));
        color.G = (byte)((color.G * firstPower) + (other.G * secondPower));
        color.B = (byte)((color.B * firstPower) + (other.B * secondPower));
        color.A = (byte)((color.A * firstPower) + (other.A * secondPower));
        return color;
    }

    public static Color ShiftHue(this Color color, float amount)
    {
        HSVColor hsvColor = new(color);
        hsvColor.H = (hsvColor.H + amount) % 360f;
        return Calc.HsvToColor(hsvColor.H / 360, hsvColor.S, hsvColor.V); ;
    }

    public static Rectangle GetBounds(this Camera camera)
    {
        int top = (int)camera.Top;
        int bottom = (int)camera.Bottom;
        int left = (int)camera.Left;
        int right = (int)camera.Right;

        return new(left, top, right - left, bottom - top);
    }

    public static Color HexColorWithAlpha(this EntityData data, string key, Color defaultValue = default(Color))
    {
        if (data.Values.TryGetValue(key, out var value))
        {
            string text = value.ToString();
            return Calc.HexToColorWithAlpha(text);
        }

        return defaultValue;
    }


    public static Vector2 Vector(this EntityData data, string name) //thanks snip!!!!@!!!!!! :333
    {
        var splitValue = data.Attr(name).Split(',');

        if (splitValue.Length != 2)
        {
            throw new InvalidOperationException($"\"{name}\" is not a valid vector; expected 2 comma-separated values, but got {splitValue.Length}");
        }

        if (!int.TryParse(splitValue[0], out var x))
            throw new InvalidOperationException($"\"{name}\" is not a valid vector; \"{splitValue[0]}\" (X component) is not a number");

        if (!int.TryParse(splitValue[1], out var y))
            throw new InvalidOperationException($"\"{name}\" is not a valid vector; \"{splitValue[1]}\" (Y component) is not a number");

        return new Vector2(x, y);
    }
}