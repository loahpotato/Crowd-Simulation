using UnityEngine;


public class FpsDisplay : MonoBehaviour
{
    [Header("Show")]
    public bool isShow = true;
    public float updateInterval = 1f;
    public int fontSize = 25;
    public Color fontColor = Color.white;
    public int margin = 50;
    public TextAnchor alignment = TextAnchor.UpperLeft;

    private GUIStyle guiStyle;
    private Rect rect;
    private int frames;
    private float fps;
    private float lastInterval;

    void Start()
    {
        guiStyle = new GUIStyle();
        guiStyle.fontStyle = FontStyle.Bold;        
        guiStyle.fontSize = fontSize;               
        guiStyle.normal.textColor = fontColor;     
        guiStyle.alignment = alignment;            

        rect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
        fps = 0.0f;
    }
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
        }
    }
    void OnGUI()
    {
        if (!isShow) return;
        GUI.Label(rect, "FPS: " + fps.ToString("F2"), guiStyle);
    }
}
