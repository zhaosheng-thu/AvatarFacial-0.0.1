
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using network;
using OVRServer;
using System.IO;

namespace network
{
    public class StateObject
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public Socket WorkSocket;
        public StringBuilder ReceivedData = new StringBuilder();
    }
}

public class NetworkServer : MonoBehaviour
{
    private const int port = 5000;
    [Tooltip("Server��IPV4 'C:\\Windows\\System32\\ipconfig.exe'")]
    public string serverIP;

    private Socket listener;
    private Socket handler;

    public OVRControllerServer oVRControllerServer;

    private void Start()
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse(serverIP);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);
            Debug.Log("�ȴ��ͻ�������...");
            listener.BeginAccept(AcceptCallback, null);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void Update()
    {
        if (handler != null && handler.Connected)
        {
            string jsonData = oVRControllerServer.allStringSend.GetAllString();//!Ϊ����ճ����β��
            if (jsonData != null)
            {
                SendToClient(jsonData + "!");
            }
        }
    }

    public void OnDestroy()
    {
        if (handler != null)
        {
            handler.Close();
        }
        if (listener != null)
        {
            listener.Close();
        }
    }

    private void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            handler = listener.EndAccept(ar);

            Debug.Log("�ͻ��������ӣ�" + handler.RemoteEndPoint);
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket clientSocket = state.WorkSocket;

            int bytesRead = clientSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                byte[] receivedBytes = new byte[bytesRead];
                Array.Copy(state.Buffer, 0, receivedBytes, 0, bytesRead);
                state.ReceivedData.Append(Encoding.ASCII.GetString(receivedBytes));

                // Check if the received data contains the delimiter
                string receivedData = state.ReceivedData.ToString();
                string delimiter = "!";
                int delimiterIndex;
                while ((delimiterIndex = receivedData.IndexOf(delimiter)) != -1)
                {
                    string message = receivedData.Substring(0, delimiterIndex);
                    Debug.Log("���յ�����Ϣ: " + message);

                    // ������յ�����Ϣmes��%�ָ�
                    oVRControllerServer.allStringGet = new AllString(message.Split("%")[0], message.Split("%")[1], message.Split("%")[2]);

                    // ��ȡ�ļ����ݣ���ͷ�Զ�������toclient�����ݷ��͸��ͻ���
                    //string jsonData = oVRControllerServer.allStringSend.GetAllString();
                    //if (jsonData != null)
                    //{
                    //    byte[] response = Encoding.ASCII.GetBytes(jsonData + delimiter);
                    //    Debug.Log("��������Ϣ" + jsonData + delimiter);
                    //    clientSocket.BeginSend(response, 0, response.Length, 0, SendCallback, clientSocket);
                    //}

                    receivedData = receivedData.Substring(delimiterIndex + delimiter.Length);
                }

                state.ReceivedData.Clear();
                state.ReceivedData.Append(receivedData);

                clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void SendToClient(string message)
    {
        try
        {
            if (handler != null && handler.Connected)
            {
                // ת��Ϊ�ֽ�����
                byte[] data = Encoding.ASCII.GetBytes(message);
                Debug.Log("��������Ϣ" + message);
                // �������ݸ��ͻ���
                handler.BeginSend(data, 0, data.Length, 0, SendCallback, handler);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            int bytesSent = clientSocket.EndSend(ar);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void Receive()
    {
        try
        {
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    public bool HasClient()
    {
        if (handler == null)
        {
            return false;
        }
        else
        {
            return handler.Connected;
        }
    }
}
