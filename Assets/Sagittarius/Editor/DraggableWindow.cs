using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// ドラッグ処理を内包したEditorWindow
/// </summary>
public class DraggableWindow : EditorWindow
{
    public delegate Rect DragDelegate(Rect rect, object data);

    // ドラッグオブジェクト.
    private class DragRect
    {
        public int priority;
        public Rectangle rect;
        public object data;
        public DragDelegate onDragStart;
        public DragDelegate onDrag;
        public DragDelegate onDragEnd;
    }

    // ドラッグ対象のデータ.
    private List<DragRect> dragObject = new List<DragRect>();

    // ドラッグオブジェクトの登録.
    public void RegisterDrag(int priority, Rectangle rect, object data = null, DragDelegate onDragStart = null, DragDelegate onDrag = null, DragDelegate onDragEnd = null)
    {
        dragObject.Add(new DragRect()
        {
            priority = priority,
            rect = rect,
            data = data,
            onDragStart = onDragStart,
            onDrag = onDrag,
            onDragEnd = onDragEnd
        });
    }


    private bool IsHover(Rectangle rect)
    {
        //TODO
        return false;
    }
}
