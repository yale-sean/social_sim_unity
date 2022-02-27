using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
public class StringPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName;

    public string publishedString;
    private RosMessageTypes.Std.MString message;

    void Start()
    {
        ros = ROSConnection.instance;
        InitializeMessage();
    }

    private void InitializeMessage()
    {
        message = new RosMessageTypes.Std.MString();
        message.data = "";
    }

    private void UpdateMessage()
    {
        message.data = publishedString;
        ros.Send(topicName, message);
    }

    public void Send(string message)
    {
        publishedString = message;
        UpdateMessage();
    }
}