using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;


public class DisplaySocialSituation : MonoBehaviour
{
    public Text text;

    void Start()
    {
        ROSConnection.instance.Subscribe<RosMessageTypes.Std.MString>("lifecycle_learner/social_situation", ReceiveMessage);
    }

    void ReceiveMessage(RosMessageTypes.Std.MString message)
    {
        text.text = "Social situation: " + message.data;
    }
}
