using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class LightProbeDataSender : MonoBehaviour
{

    private UdpClient udpClient = new UdpClient();
    public string udpIPAddress = "192.168.178.3"; // The IP address to send the data to
    public int udpPort = 7778; // The port to send the data to

    // Start is called before the first frame update
    void Start()
    {
        udpClient.EnableBroadcast = true;
    }

    // Update is called once per frame
    void Update()
    {
        SendLightProbeData(udpClient);
    }

    void SendLightProbeData(UdpClient udpClient)
    {
        Vector3[] probePositions = LightmapSettings.lightProbes.positions;
        int probeCount = probePositions.Length;

        // Serialize the data to a byte array
        for (int i = 0; i < probeCount; i++)
        {
            SphericalHarmonicsL2 shCoefficient;
            LightProbes.GetInterpolatedProbe(probePositions[i], null, out shCoefficient);
            byte[] data = SerializeLightProbeData(i, shCoefficient);

            // Send the data over UDP
            udpClient.Send(data, data.Length, udpIPAddress, udpPort);
        }
    }

    byte[] SerializeLightProbeData(int idx, SphericalHarmonicsL2 shCoefficients)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                // Write the index
                writer.Write(idx);

                // Write SH coefficients
                for (int rgb = 0; rgb < 3; rgb++)
                {
                    for (int coefficient = 0; coefficient < 9; coefficient++)
                    {
                        writer.Write(shCoefficients[rgb, coefficient]);
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}
