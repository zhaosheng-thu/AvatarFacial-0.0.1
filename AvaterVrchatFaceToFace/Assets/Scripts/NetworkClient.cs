//using UnityEngine;
//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using network;
//using OVRServer;

//public class NetworkClient : MonoBehaviour
//{
//    private const int port = 5000;
//    [Tooltip("Server的IPV4 \n get:'C:\\Windows\\System32\\ipconfig.exe'")]
//    public string serverIP;

//    private Socket clientSocket;
//    private StateObject state;
//    public OVRControllerServer oVRControllerServer;

//    private void Start()
//    {
//        ConnectToServer();
//    }

//    private void ConnectToServer()
//    {
//        try
//        {
//            // 创建客户端Socket
//            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//            // 连接服务器
//            IPAddress ipAddress = IPAddress.Parse(serverIP);
//            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
//            clientSocket.BeginConnect(remoteEndPoint, ConnectCallback, null);
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.ToString());
//        }
//    }

//    private void ConnectCallback(IAsyncResult ar)
//    {
//        try
//        {
//            // 完成异步连接操作
//            clientSocket.EndConnect(ar);

//            Debug.Log("已连接到服务器：" + clientSocket.RemoteEndPoint);

//            // 开始接收服务器的响应
//            state = new StateObject();
//            state.WorkSocket = clientSocket;
//            clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.ToString());
//        }
//    }

//    private void Update()
//    {
//        // 检查是否有数据可接收，并且在满足条件时发送
//        if (state != null && clientSocket != null && clientSocket.Connected && clientSocket.Available > 0)
//        {
//            string jsonData = oVRControllerServer.allStringSend.GetAllString() + "!";//!为控制粘包的尾号
//            if (jsonData != null)
//            {
//                SendDataToServer(jsonData);
//            }
//            //clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
//        }
//        if (!clientSocket.Connected)
//        {
//            Debug.Log("Disconnected");
//        }
//    }

//    public void OnDestroy()
//    {
//        if (clientSocket != null)
//        {
//            clientSocket.Close();
//        }
//    }


//    private void ReceiveCallback(IAsyncResult ar)
//    {
//        try
//        {
//            // 完成异步接收操作
//            StateObject state = (StateObject)ar.AsyncState;
//            Socket serverSocket = state.WorkSocket;

//            // 读取接收的数据
//            int bytesRead = serverSocket.EndReceive(ar);
//            if (bytesRead > 0)
//            {
//                string message = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
//                Debug.Log("接收到的响应：" + message);

//                // 处理接收到的响应
//                oVRControllerServer.allStringGet = new AllString(message.Split("%")[0], message.Split("%")[1], message.Split("%")[2]);
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.ToString());
//        }
//    }

//    private void SendDataToServer(string message)
//    {
//        try
//        {
//            // 发送数据到服务器
//            byte[] data = Encoding.ASCII.GetBytes(message);
//            clientSocket.BeginSend(data, 0, data.Length, 0, SendCallback, clientSocket);
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.ToString());
//        }
//    }

//    private void SendCallback(IAsyncResult ar)
//    {
//        try
//        {
//            // 完成异步发送操作
//            Socket serverSocket = (Socket)ar.AsyncState;
//            int bytesSent = serverSocket.EndSend(ar);
//            //Debug.Log("向服务器发送了 " + bytesSent + " 字节的数据");
//        }
//        catch (Exception ex)
//        {
//            Debug.Log(ex.ToString());
//        }
//    }

//    public bool IsConnected()
//    {
//        return clientSocket.Connected;
//    }
//}


using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using network;
using OVRServer;

public class NetworkClient : MonoBehaviour
{
    private const int port = 5000;
    [Tooltip("Server的IPV4 \n get:'C:\\Windows\\System32\\ipconfig.exe'")]
    public string serverIP;

    private Socket clientSocket;
    private StateObject state;
    public OVRControllerServer oVRControllerServer;

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        try
        {
            // 创建客户端Socket
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // 连接服务器
            IPAddress ipAddress = IPAddress.Parse(serverIP);
            IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, port);
            clientSocket.BeginConnect(remoteEndPoint, ConnectCallback, null);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // 完成异步连接操作
            clientSocket.EndConnect(ar);

            Debug.Log("已连接到服务器：" + clientSocket.RemoteEndPoint);

            // 开始接收服务器的响应
            state = new StateObject();
            state.WorkSocket = clientSocket;
            clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void Update()
    {
        // 检查是否有数据可接收，并且在满足条件时发送
        if (state != null && clientSocket != null && clientSocket.Connected)
        {
            //Receive();
            string jsonData = oVRControllerServer.allStringSend.GetAllString();//!为控制粘包的尾号
            if (jsonData != null)
            {
                SendDataToServer(jsonData + "!");
            }
        }
        if (!clientSocket.Connected)
        {
            Debug.Log("Disconnected");
        }
    }

    public void OnDestroy()
    {
        if (clientSocket != null)
        {
            clientSocket.Close();
        }
    }

    private void Receive()
    {
        try
        {
            StateObject state = new StateObject();
            state.WorkSocket = clientSocket;

            clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket serverSocket = state.WorkSocket;

            int bytesRead = serverSocket.EndReceive(ar);

            if (bytesRead > 0)
            {
                byte[] receivedBytes = new byte[bytesRead];
                Array.Copy(state.Buffer, 0, receivedBytes, 0, bytesRead);
                state.ReceivedData.Append(Encoding.ASCII.GetString(receivedBytes));

                string delimiter = "!";
                string receivedData = state.ReceivedData.ToString();

                while (receivedData.Contains(delimiter))
                {
                    int delimiterIndex = receivedData.IndexOf(delimiter);
                    string message = receivedData.Substring(0, delimiterIndex);
                    Debug.Log("接收到的消息: " + message);

                    // 处理接收到的消息mes以%分割
                    oVRControllerServer.allStringGet = new AllString(message.Split("%")[0], message.Split("%")[1], message.Split("%")[2]);

                    receivedData = receivedData.Substring(delimiterIndex + delimiter.Length);
                }

                state.ReceivedData.Clear();
                state.ReceivedData.Append(receivedData);

                clientSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    public void SendDataToServer(string message)
    {
        try
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            clientSocket.BeginSend(data, 0, data.Length, 0, SendCallback, clientSocket);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket serverSocket = (Socket)ar.AsyncState;
            int bytesSent = serverSocket.EndSend(ar);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
    }

    public bool IsConnected()
    {
        return clientSocket.Connected;
    }
}
