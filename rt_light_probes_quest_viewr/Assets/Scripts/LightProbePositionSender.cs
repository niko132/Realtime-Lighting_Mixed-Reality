using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class LightProbePositionSender : MonoBehaviour
{

    private UdpClient udpClient = new UdpClient();
    public string udpIPAddress = "192.168.178.7"; // The IP address to send the data to
    public int udpPort = 7777; // The port to send the data to

    public bool SyncButton = false;

    // Gets called when the mapping button was pressed
    void OnValidate()
    {
        if (SyncButton)
        {
            SendLightProbePositions();
            SyncButton = false;
        }
    }

    void SendLightProbePositions()
    {
        LightProbeGroup lightProbeGroup = gameObject.GetComponent<LightProbeGroup>();
        Vector3[] lightProbePositions = lightProbeGroup.probePositions;

        byte[] data = SerializeLightProbePositions(lightProbePositions);

        // Send the data over UDP
        udpClient.Send(data, data.Length, udpIPAddress, udpPort);
    }

    byte[] SerializeLightProbePositions(Vector3[] lightProbePositions)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                // Write the number of light probes
                writer.Write(lightProbePositions.Length);

                foreach (var position in lightProbePositions)
                {
                    // Write position
                    writer.Write(position.x);
                    writer.Write(position.y);
                    writer.Write(position.z);
                }
            }

            return memoryStream.ToArray();
        }
    }
}
