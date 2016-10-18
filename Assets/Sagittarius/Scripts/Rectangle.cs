using System;
using UnityEngine;

// Rect構造体のクラス版
[Serializable]
public class Rectangle
{
    public float x, y, width, height;

    public Vector2 position
    {
        get { return new Vector2(x, y); }
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public Vector2 Min
    {
        get { return new Vector2(xMin, yMin); }
    }
    public Vector2 Max
    {
        get { return new Vector2(xMax, yMax); }
    }
    public float xMin
    {
        get { return Mathf.Min(x, x + width); }
    }
    public float xMax
    {
        get { return Mathf.Max(x, x + width); }
    }
    public float yMin
    {
        get { return Mathf.Min(y, y + height); }
    }
    public float yMax
    {
        get { return Mathf.Max(y, y + height); }
    }

    public Rect Rect
    {
        get { return new Rect(x, y, width, height); }
    }

    public Rectangle(Rect source)
    {
        x = source.x;
        y = source.y;
        width = source.width;
        height = source.height;
    }

    public Rectangle(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public override string ToString()
    {
        return string.Format("[Rectangle x:{0} y:{1}, width:{2}, height:{3}]", x, y, width, height);
    }
}