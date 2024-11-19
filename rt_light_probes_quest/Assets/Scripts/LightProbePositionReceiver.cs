using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using UnityEngine.Rendering;
using System.IO;

[ExecuteInEditMode]
public class LightProbePositionReceiver : MonoBehaviour
{
    private UdpClient _ReceiveClient;
    private Thread _ReceiveThread;

    public int udpPort = 7777; // The port to send the data to

    public bool CanReceiveButton = false;

    private Vector3[] newLightProbePositions = null;

    // Gets called when the mapping button was pressed
    void OnValidate()
    {
        if (CanReceiveButton)
        {
            EnableReceiving();
        } else
        {
            DisableReceiving();
        }
    }

    void Update()
    {
        if (newLightProbePositions != null)
        {
            LightProbeGroup lightProbeGroup = gameObject.GetComponent<LightProbeGroup>();
            lightProbeGroup.probePositions = newLightProbePositions;

            newLightProbePositions = null;
        }
    }

    void EnableReceiving()
    {
        CanReceiveButton = true;

        _ReceiveThread = new Thread(new ThreadStart(ReceiveData));
        _ReceiveThread.IsBackground = true;
        _ReceiveThread.Start();
    }

    void DisableReceiving()
    {
        CanReceiveButton = false;

        try
        {
            _ReceiveThread.Abort();
            _ReceiveThread = null;
            _ReceiveClient.Close();
        }
        catch (Exception err)
        {
            Debug.Log("<color=red>" + err.Message + "</color>");
        }
    }

    void ReceiveData()
    {
        _ReceiveClient = new UdpClient(udpPort);
        _ReceiveClient.EnableBroadcast = true;

        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _ReceiveClient.Receive(ref anyIP);

                newLightProbePositions = DeserializeLightProbePositions(data);
            }
            catch (Exception err)
            {
                Debug.Log("<color=red>" + err.Message + "</color>");
            }
        }
    }

    Vector3[] DeserializeLightProbePositions(byte[] data)
    {
        using (MemoryStream memoryStream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                // Read the number of light probes
                int count = reader.ReadInt32();

                Vector3[] positions = new Vector3[count];
                for (int i = 0; i < count; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    positions[i] = new Vector3(x, y, z);
                }

                return positions;
            }
        }
    }

    void OnApplicationQuit()
    {
        DisableReceiving();
    }
}
