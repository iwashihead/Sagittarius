using UnityEngine;
using UnityEditor;
using System.Collections;
using Griphone.Sagittarius;

public class ScriptEditor : EditorWindow
{
    [MenuItem("Window/ScriptEditor")]
    static void init()
    {
        ScriptEditorDebughelpers.openScriptEditor();
    }
    Rect wr = new Rect(100, 100, 100, 100);
    Rect wr2 = new Rect(300, 100, 100, 100);
    Rect wr3 = new Rect(150, 300, 100, 100);
    void doWindow(int id)
    {
        GUI.Button(new Rect(0, 30, 100, 50), "Wee!");
        GUI.DragWindow();
    }
    void curveFromTo(Rect wr, Rect wr2, Color color, Color shadow)
    {
        Drawing.bezierLine(
            new Vector2(wr.x + wr.width, wr.y + 3 + wr.height / 2),
            new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + 3 + wr.height / 2),
            new Vector2(wr2.x, wr2.y + 3 + wr2.height / 2),
            new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + 3 + wr2.height / 2), shadow, 5, true, 20);
        Drawing.bezierLine(
            new Vector2(wr.x + wr.width, wr.y + wr.height / 2),
            new Vector2(wr.x + wr.width + Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr.y + wr.height / 2),
            new Vector2(wr2.x, wr2.y + wr2.height / 2),
            new Vector2(wr2.x - Mathf.Abs(wr2.x - (wr.x + wr.width)) / 2, wr2.y + wr2.height / 2), color, 2, true, 20);
    }
    void OnGUI()
    {
        Color s = new Color(0.4f, 0.4f, 0.5f);
        curveFromTo(wr, wr2, new Color(0.3f, 0.7f, 0.4f), s);
        curveFromTo(wr2, wr3, new Color(0.7f, 0.2f, 0.3f), s);

        BeginWindows();
        wr = GUI.Window(0, wr, doWindow, "hello");
        wr2 = GUI.Window(1, wr2, doWindow, "world");
        wr3 = GUI.Window(2, wr3, doWindow, "!");
        wr = ConstrainRect(wr, new Rect(position.x - 100, position.y - 100, position.width - 100, position.height - 100));
        EndWindows();
    }

    // WindowÇÃóÃàÊÇ…êßå¿ÇÇ©ÇØÇ‹Ç∑
    Rect ConstrainRect(Rect window, Rect constraintsSize)
    {
        Debug.Log(string.Format("window : {0}    const : {1}", window, constraintsSize));
        window.x = Mathf.Clamp(window.x, position.x - constraintsSize.x, constraintsSize.width - window.width);
        window.y = Mathf.Clamp(window.y, position.y - constraintsSize.y, constraintsSize.height - window.height);
        return window;
    }
}