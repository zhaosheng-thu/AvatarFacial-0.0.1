
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
    [Tooltip("Server的IPV4 'C:\\Windows\\System32\\ipconfig.exe'")]
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
            Debug.Log("等待客户端连接...");
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
            string jsonData = oVRControllerServer.allStringSend.GetAllString();//!为控制粘包的尾号
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

            Debug.Log("客户端已连接：" + handler.RemoteEndPoint);
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
                    Debug.Log("接收到的消息: " + message);

                    // 处理接收到的消息mes以%分割
                    oVRControllerServer.allStringGet = new AllString(message.Split("%")[0], message.Split("%")[1], message.Split("%")[2]);

                    // 读取文件内容，将头显读出存入toclient的数据发送给客户端
                    //string jsonData = oVRControllerServer.allStringSend.GetAllString();
                    //if (jsonData != null)
                    //{
                    //    byte[] response = Encoding.ASCII.GetBytes(jsonData + delimiter);
                    //    Debug.Log("发送了消息" + jsonData + delimiter);
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
                // 转换为字节数组
                byte[] data = Encoding.ASCII.GetBytes(message);
                Debug.Log("发送了消息" + message);
                // 发送数据给客户端
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
