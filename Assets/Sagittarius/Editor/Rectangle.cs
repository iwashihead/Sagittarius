using UnityEngine;

// Rect構造体のクラス版
public class Rectangle
{
    public float x, y, width, height;

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

    public Rect ConvertToRect()
    {
        return new Rect(x, y, width, height);
    }
}