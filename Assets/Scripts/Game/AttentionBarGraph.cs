using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;

public class AttentionBarGraph : MonoBehaviour
{
    //public float[] attention_l;
    //public float[] attention_cmd_vel;
    Dictionary<string, float> attention_l;
    Dictionary<string, float> attention_cmd_vel;
    Dictionary<string, int> color_dict;

    public GameObject attentionCanvas;
    GUIStyle currentStyle;
    GUIStyle markListenStyle;
    public Image image1;
    public Image image2;
    public Image legend;
    public Text text1;
    public Text text2;

    public string topic1;
    public string topic2;

    Vector2 pos1;
    Vector2 pos2;
    Vector2 size;
    Vector2 canvasSize;
    bool isUsingAttentionL;
    bool isUsingAttentionCmdVel;
    int numChannels1;
    int numChannels2;

    void Start()
    {
        attention_l = new Dictionary<string, float> { { "image", -1.0f }, { "global", -1.0f }, { "social", -1.0f } };
        attention_cmd_vel = new Dictionary<string, float> { { "image", -1.0f }, { "local", -1.0f }, { "global", -1.0f }, { "social", -1.0f } };
        color_dict = new Dictionary<string, int> { { "image", 0 }, { "local", 1 }, { "global", 2 }, { "social", 3 } };

        isUsingAttentionL = false;
        isUsingAttentionCmdVel = false;

        var image = image1.GetComponent<Image>();
        var tempColor = image.color;
        tempColor.a = 0.5f;
        image.color = tempColor;

        image = image2.GetComponent<Image>();
        tempColor = image.color;
        tempColor.a = 0.5f;
        image.color = tempColor;

        pos1 = image1.rectTransform.position;
        pos2 = image2.rectTransform.position;

        legend.gameObject.SetActive(false);

        size = image1.rectTransform.sizeDelta;
        canvasSize = attentionCanvas.GetComponent<RectTransform>().sizeDelta;

        ROSConnection.instance.Subscribe<RosMessageTypes.Std.MFloat32MultiArray>(topic1, ReceiveMessageL);
        ROSConnection.instance.Subscribe<RosMessageTypes.Std.MFloat32MultiArray>(topic2, ReceiveMessageCmdVel);
    }

    void ReceiveMessageL(RosMessageTypes.Std.MFloat32MultiArray message)
    {
        //Debug.Log("message received attention_l");
        isUsingAttentionL = true;
        float[] data = message.data;
        if (data.Length == 2)
        {
            attention_l["image"] = data[0];
            attention_l["global"] = data[1];
            numChannels1 = 2;
        }
        else if (data.Length == 3)
        {
            attention_l["image"] = data[0];
            attention_l["global"] = data[1];
            attention_l["social"] = data[2];
            numChannels1 = 3;
        }
    }

    void ReceiveMessageCmdVel(RosMessageTypes.Std.MFloat32MultiArray message)
    {
        // Debug.Log("message received attention_cmd_vel");
        isUsingAttentionCmdVel = true;
        float[] data = message.data;
        if (data.Length == 3)
        {
            attention_cmd_vel["image"] = data[0];
            attention_cmd_vel["local"] = data[1];
            attention_cmd_vel["global"] = data[2];
            numChannels2 = 3;
        }
        else if (data.Length == 4)
        {
            attention_cmd_vel["image"] = data[0];
            attention_cmd_vel["local"] = data[1];
            attention_cmd_vel["global"] = data[2];
            attention_cmd_vel["social"] = data[3];
            numChannels2 = 4;
        }
    }

    public void MarkAttentionChannel(int graphIndex, int numChannels)
    {
        Vector2 pos;
        if (graphIndex == 1) { pos = pos1; }
        else { pos = pos2; }

        for (int i = 0; i < numChannels; i++)
        {
            float x = pos.x - size.x / 2;
            float y = canvasSize.y - pos.y - size.y / 2 + 10 + 30 * i;
            markListenStyle = new GUIStyle(GUI.skin.box);
            markListenStyle.normal.background = MakeTex(2, 2, Color.black);
            Rect b = new Rect(x + size.x - 5, y + 5, 10, 10);
            GUI.Box(b, "", markListenStyle);
        }
    }

    private void InitStyles(int color_index)
    {
        var colors = new Color[]{
            new Color(176/255f, 122/255f, 161/255f, 1f), // #B07AA1
            new Color(225/255f, 87/255f, 89/255f,1f), // #E15759
            new Color(242/255f, 142/255f, 43/255f, 1f), // #F28E2B
            new Color(255/255f, 157/255f, 167/255f, 1f), // #FF9DA7
        };

        currentStyle = new GUIStyle(GUI.skin.box);
        currentStyle.normal.background = MakeTex(2, 2, colors[color_index]);

    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    public void DrawGraph(Dictionary<string, float> attention, int graphIndex)
    {
        Vector2 pos;
        if (graphIndex == 1)
        {
            pos = pos1;
            if (isUsingAttentionL)
            {
                text1.gameObject.SetActive(false);
            }
            else
            {
                text1.gameObject.SetActive(true);
                return;
            }
        }
        else
        {
            pos = pos2;
            if (isUsingAttentionCmdVel)
            {
                text2.gameObject.SetActive(false);
            }
            else
            {
                text2.gameObject.SetActive(true);
                return;
            }
        }

        int count = 0;
        foreach (KeyValuePair<string, float> entry in attention)
        {
            InitStyles(color_dict[entry.Key]);
            float x = pos.x - size.x / 2;
            float y = canvasSize.y - pos.y - size.y / 2 + 10 + 30 * count;
            var val = entry.Value;
            // avoid weird behavior of GUI.Box when width is too small
            if (val < 0.05)
            {
                val *= 2;
                if (val < 0.05)
                {
                    return;
                }
            }

            Rect a = new Rect(x, y, val * size.x, 20);
            GUI.Box(a, "", currentStyle);
            count++;
        }

    }


    void OnGUI()
    {
        if (isUsingAttentionL || isUsingAttentionCmdVel)
        {
            legend.gameObject.SetActive(true);
        }
        // Mark channel
        MarkAttentionChannel(1, numChannels1);
        MarkAttentionChannel(2, numChannels2);
        // Draw graph
        DrawGraph(attention_l, 1);
        DrawGraph(attention_cmd_vel, 2);

    }

}
