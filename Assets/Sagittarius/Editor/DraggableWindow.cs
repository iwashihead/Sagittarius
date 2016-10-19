using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Griphone.Sagittarius
{
    /// <summary>
    /// ドラッグ処理を内包したEditorWindow
    /// </summary>
    public class DraggableWindow : EditorWindow
    {
        #region Public Declaration
        /// <summary>
        /// ドラッグ用のデリゲート型
        /// </summary>
        /// <param name="mousePos">マウス位置</param>
        /// <param name="obj">ドラッグ対象</param>
        public delegate void DragDelegate(Vector2 mousePos, DragObject obj);

        /// <summary>
        /// 座標軸
        /// </summary>
        public enum Pivot
        {
            TopLeft,
            Center,
        }

        /// <summary>
        /// ドラッグ対象.
        /// </summary>
        public class DragObject
        {
            /// <summary>
            /// 現在のドラッグ対象
            /// </summary>
            public static DragObject Current;

            public int priority;
            public Pivot pivot;
            public Vector2 dragOffset;
            public Func<float> scaleFunc;
            public Func<float> zoomFunc;
            public Func<Vector2> focusFunc;
            public Func<Vector2> editorPosFunc;
            public Rectangle rect;
            public object data;
            public DragDelegate onDragStart;
            public DragDelegate onDrag;
            public DragDelegate onDragEnd;
        }
        #endregion

        #region Private Declaration
        protected List<DragObject> dragObjects = new List<DragObject>();
        protected Vector2 dragStartPos;
        private static readonly Func<float> DefaultScaleFunc = () => { return 1f; };
        private static readonly Func<Vector2> DefaultEditorPosFunc = () => { return Vector2.zero; };
        #endregion

        #region Public Method
        /// <summary>
        /// ドラッグオブジェクトの登録.
        /// </summary>
        /// <param name="priority">優先度</param>
        /// <param name="rect">領域データ</param>
        /// <param name="pivot">座標軸</param>
        /// <param name="scaleFunc">スケール取得用メソッド</param>
        /// <param name="zoomFunc">ズーム値取得用メソッド</param>
        /// <param name="focusFunc">注視点座標取得用メソッド</param>
        /// <param name="data">ドラッグ対象に渡すオブジェクト</param>
        /// <param name="onDragStart">ドラッグ開始時コールバック</param>
        /// <param name="onDrag">ドラッグ時コールバック</param>
        /// <param name="onDragEnd">ドラッグ終了コールバック</param>
        /// <returns></returns>
        public DragObject RegisterDrag(int priority,
            Rectangle rect,
            Pivot pivot = Pivot.TopLeft,
            Func<float> scaleFunc = null,
            Func<float> zoomFunc = null,
            Func<Vector2> focusFunc = null,
            object data = null,
            DragDelegate onDragStart = null,
            DragDelegate onDrag = null,
            DragDelegate onDragEnd = null)
        {
            var drag = new DragObject()
            {
                priority = priority,
                rect = rect,
                pivot = pivot,
                scaleFunc = scaleFunc ?? DefaultScaleFunc,
                zoomFunc = zoomFunc ?? DefaultScaleFunc,
                focusFunc = focusFunc ?? DefaultEditorPosFunc,
                data = data,
                onDragStart = onDragStart,
                onDrag = onDrag,
                onDragEnd = onDragEnd
            };
            dragObjects.Add(drag);
            dragObjects = dragObjects.OrderBy(_ => _.priority).ToList();
            return drag;
        }

        /// <summary>
        /// ドラッグオブジェクトのクリア
        /// </summary>
        public void ClearDrag()
        {
            dragObjects.Clear();
        }
        #endregion

        #region Private Method
        /// <summary>
        /// ドラッグ位置の確認用メソッド. (for Debug)
        /// </summary>
        [Conditional("DEBUG")]
        void CheckRect(Rectangle rect, Pivot pivot, float scale, float zoom, Vector2 focus, Vector2 mousePos)
        {
            //var x = (position.width / 2 - rect.width / 2 * zoom) + (rect.x + focus.x) * zoom;
            //var y = (position.height / 2 - rect.height / 2 * zoom) + (rect.y + focus.y) * zoom;
            //GUI.Box(new Rect(x, y, rect.width*zoom*scale, rect.height*zoom*scale), "");
        }

        /// <summary>
        /// 指定のRect上にマウスカーソルが位置しているかどうか.
        /// </summary>
        protected bool IsHover(Rectangle rect, Pivot pivot, float scale, float zoom, Vector2 focus, Vector2 mousePos)
        {
            Debug.Log(string.Format("mouse:{0}  rect:{1}  scale:{2}  winScale:{3}", mousePos, rect, scale, zoom));
            if (pivot == Pivot.Center)
            {
                var x = (position.width / 2 - rect.width / 2 * zoom) + (rect.x + focus.x) * zoom;
                var y = (position.height / 2 - rect.height / 2 * zoom) + (rect.y + focus.y) * zoom;

                return mousePos.x > x &&
                       mousePos.x < x + rect.width * zoom * Mathf.Abs(scale) &&
                       mousePos.y > y &&
                       mousePos.y < y + rect.height * zoom * Mathf.Abs(scale);
            }
            return mousePos.x > rect.x * zoom &&
                   mousePos.x < rect.x + rect.width * zoom * scale &&
                   mousePos.y > rect.y * zoom &&
                   mousePos.y < rect.y + rect.height * zoom * scale;
        }
        #endregion

        #region Unity Event
        /// <summary>
        /// GUI描画イベント
        /// </summary>
        protected virtual void OnGUI()
        {
            Event e = Event.current;
            do
            {
                OnGuiEvent(e);
            }
            while (Event.PopEvent(e));
        }

        /// <summary>
        /// GUI入力イベント
        /// </summary>
        protected virtual void OnGuiEvent(Event e)
        {
            foreach (var o in dragObjects.OrderBy(_ => _.priority))
            {
                if (o.pivot == Pivot.TopLeft) continue;
                CheckRect(o.rect, o.pivot, o.scaleFunc(), o.zoomFunc(), o.focusFunc(), e.mousePosition);
            }
            if (e == null) return;

            if (e.type == EventType.MouseDown)
            {
                foreach (var o in dragObjects.OrderBy(_ => _.priority))
                {
                    if (o == null) continue;
                    if (IsHover(o.rect, o.pivot, o.scaleFunc(), o.zoomFunc(), o.focusFunc(), e.mousePosition))
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
                    //Debug.Log(DragObject.Current.data);
                    DragObject.Current.onDrag(e.mousePosition, DragObject.Current);
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (DragObject.Current != null)
                {
                    //Debug.Log(DragObject.Current.data);
                    DragObject.Current.onDragEnd(e.mousePosition, DragObject.Current);
                    DragObject.Current.dragOffset = Vector2.zero;
                }

                // Current解除
                DragObject.Current = null;
            }
        }
        #endregion
    }
}