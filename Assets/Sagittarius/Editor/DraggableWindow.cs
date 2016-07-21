using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ドラッグ処理を内包したEditorWindow
/// </summary>
public class DraggableWindow : EditorWindow
{
    public delegate void DragDelegate(Vector2 mousePos, DragObject obj);

    // ドラッグオブジェクト.
    public class DragObject
    {
        public static DragObject Current;

        public int priority;
        public Vector2 dragOffset;
        public Func<float> scaleFunc;
        public Func<float> windowScaleFunc; 
        public Rectangle rect;
        public object data;
        public DragDelegate onDragStart;
        public DragDelegate onDrag;
        public DragDelegate onDragEnd;
    }

    // ドラッグ対象のデータ.
    protected List<DragObject> dragObjects = new List<DragObject>();
    protected Vector2 dragStartPos;
    private static readonly Func<float> DefaultScaleFunc = () => { return 1f; };

    // ドラッグオブジェクトの登録.
    public void RegisterDrag(int priority, Rectangle rect, Func<float> scaleFunc = null, Func<float> windowScaleFunc = null, object data = null, DragDelegate onDragStart = null, DragDelegate onDrag = null, DragDelegate onDragEnd = null)
    {
        dragObjects.Add(new DragObject()
        {
            priority = priority,
            rect = rect,
            scaleFunc = scaleFunc ?? DefaultScaleFunc,
            windowScaleFunc = windowScaleFunc ?? DefaultScaleFunc,
            data = data,
            onDragStart = onDragStart,
            onDrag = onDrag,
            onDragEnd = onDragEnd
        });
        dragObjects = dragObjects.OrderBy(_ => _.priority).ToList();
    }

    // 指定のRect上にマウスカーソルが位置しているかどうか.
    protected bool IsHover(Rectangle rect, float scale, float windowScale, Vector2 mousePos)
    {
        Debug.Log(string.Format("mouse:{0}  rect:{1}  scale:{2}  winScale:{3}", mousePos, rect, scale, windowScale));
        return mousePos.x > rect.x * windowScale &&
               mousePos.x < rect.x + rect.width * windowScale * scale &&
               mousePos.y > rect.y * windowScale &&
               mousePos.y < rect.y + rect.height * windowScale * scale;
    }

    // GUI描画イベント.
    protected virtual void OnGUI()
    {
        Event e = Event.current;
        do
        {
            OnGuiEvent(e);
        } while (Event.PopEvent(e));
    }

    // GUI入力イベント.
    protected virtual void OnGuiEvent(Event e)
    {
        if (e == null) return;

        if (e.type == EventType.MouseDown)
        {
            foreach (var o in dragObjects.OrderBy(_ => _.priority))
            {
                if (o == null) continue;
                if (IsHover(o.rect, o.scaleFunc(), o.windowScaleFunc(), e.mousePosition))
                {
                    dragStartPos = e.mousePosition;
                    DragObject.Current = o;
                    DragObject.Current.dragOffset = DragObject.Current.rect.position - e.mousePosition;
                    DragObject.Current.onDragStart(e.mousePosition, DragObject.Current);
                    Debug.Log(o.data);
                    break;
                }
            }
        }
        else if (e.type == EventType.MouseDrag)
        {
            if (DragObject.Current != null)
            {
                Debug.Log(DragObject.Current.data);
                DragObject.Current.onDrag(e.mousePosition, DragObject.Current);
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            if (DragObject.Current != null)
            {
                Debug.Log(DragObject.Current.data);
                DragObject.Current.onDragEnd(e.mousePosition, DragObject.Current);
                DragObject.Current.dragOffset = Vector2.zero;
            }

            // Current解除
            DragObject.Current = null;
        }
    }
}
