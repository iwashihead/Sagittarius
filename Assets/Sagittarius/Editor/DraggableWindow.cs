using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

#region Sample
//public class SampleWindow : DraggableWindow
//{
//    [MenuItem("Custom/SampleWindow/Open")]
//    public static void Open()
//    {
//        instance = Instance;
//    }
//
//    private static SampleWindow instance;
//    public static SampleWindow Instance
//    {
//        get
//        {
//            if (instance == null)
//            {
//                instance = GetWindow<SampleWindow>();
//                instance.Initialize();
//            }
//            return instance;
//        }
//    }
//
//    private List<Rectangle> BoxRects = new List<Rectangle>(); 
//
//    private void Initialize()
//    {
//        BoxRects.Add(new Rectangle(10, 10, 100, 100));
//        BoxRects.Add(new Rectangle(150, 10, 100, 100));
//        BoxRects.Add(new Rectangle(10, 150, 100, 100));
//
//        int priority = 0;
//        foreach (var boxRect in BoxRects)
//        {
//            RegisterDrag(priority, boxRect, null,
//                (mousePos, data) => {  },
//                (mousePos, data) =>
//                {
//                    DragObject.Current.rect.position = mousePos + DragObject.Current.dragOffset;
//                    Repaint();
//                },
//                (mousePos, data) =>
//                {
//                    DragObject.Current.rect.position = mousePos + DragObject.Current.dragOffset;
//                    Repaint();
//                });
//            priority++;
//        }
//    }
//
//    protected override void OnGUI()
//    {
//        int priority = 0;
//        foreach (var boxRect in BoxRects)
//        {
//            GUI.Box(boxRect.Rect, priority.ToString());
//            priority++;
//        }
//
//        base.OnGUI();
//    }
//}
#endregion

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
        public Rectangle rect;
        public object data;
        public DragDelegate onDragStart;
        public DragDelegate onDrag;
        public DragDelegate onDragEnd;
    }

    // ドラッグ対象のデータ.
    protected List<DragObject> dragObjects = new List<DragObject>();
    protected Vector2 dragStartPos;

    // ドラッグオブジェクトの登録.
    public void RegisterDrag(int priority, Rectangle rect, object data = null, DragDelegate onDragStart = null, DragDelegate onDrag = null, DragDelegate onDragEnd = null)
    {
        dragObjects.Add(new DragObject()
        {
            priority = priority,
            rect = rect,
            data = data,
            onDragStart = onDragStart,
            onDrag = onDrag,
            onDragEnd = onDragEnd
        });
        dragObjects = dragObjects.OrderBy(_ => _.priority).ToList();
    }


    // 指定のRect上にマウスカーソルが位置しているかどうか.
    protected bool IsHover(Rectangle rect, Vector2 mousePos)
    {
        return mousePos.x > rect.x &&
               mousePos.x < rect.x + rect.width &&
               mousePos.y > rect.y &&
               mousePos.y < rect.y + rect.height;
    }

    protected virtual void OnGuiEvent(Event e)
    {
        if (e == null) return;

        if (e.type == EventType.MouseDown)
        {
            Debug.Log("Mouse Down");
            foreach (var o in dragObjects.OrderBy(_ => _.priority))
            {
                if (o == null) continue;
                if (IsHover(o.rect, e.mousePosition))
                {
                    dragStartPos = e.mousePosition;
                    DragObject.Current = o;
                    DragObject.Current.dragOffset = DragObject.Current.rect.position - e.mousePosition;
                    DragObject.Current.onDragStart(e.mousePosition, DragObject.Current);
                    break;
                }
            }
        }
        else if (e.type == EventType.MouseDrag)
        {
            Debug.Log("Mouse Drag");
            if (DragObject.Current != null)
            {
                DragObject.Current.onDrag(e.mousePosition, DragObject.Current);
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            Debug.Log("Mouse Up");
            if (DragObject.Current != null)
            {
                DragObject.Current.onDragEnd(e.mousePosition, DragObject.Current);
                DragObject.Current.dragOffset = Vector2.zero;
            }

            // Current解除
            DragObject.Current = null;
        }
    }

    protected virtual void OnGUI()
    {
        Event e = Event.current;
        do
        {
            OnGuiEvent(e);
        } while (Event.PopEvent(e));
    }
}
