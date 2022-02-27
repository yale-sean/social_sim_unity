using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

public class RosConnectorPortFromEnv : MonoBehaviour
{
    void Awake()
    {
        ROSConnection connection = GetComponent<ROSConnection>();

        string port = System.Environment.GetEnvironmentVariable("TCP_BRIDGE_PORT");
        if (!string.IsNullOrEmpty(port))
        {
            connection.RosPort = int.Parse(port);
        }
        string host = System.Environment.GetEnvironmentVariable("TCP_BRIDGE_HOST");
        if (!string.IsNullOrEmpty(host))
        {
            connection.RosIPAddress = host;
        }
    }
}
