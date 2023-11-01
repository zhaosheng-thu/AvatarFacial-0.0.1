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
//    [Tooltip("Server��IPV4 \n get:'C:\\Windows\\System32\\ipconfig.exe'")]
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
//            // �����ͻ���Socket
//            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//            // ���ӷ�����
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
//            // ����첽���Ӳ���
//            clientSocket.EndConnect(ar);

//            Debug.Log("�����ӵ���������" + clientSocket.RemoteEndPoint);

//            // ��ʼ���շ���������Ӧ
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
//        // ����Ƿ������ݿɽ��գ���������������ʱ����
//        if (state != null && clientSocket != null && clientSocket.Connected && clientSocket.Available > 0)
//        {
//            string jsonData = oVRControllerServer.allStringSend.GetAllString() + "!";//!Ϊ����ճ����β��
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
//            // ����첽���ղ���
//            StateObject state = (StateObject)ar.AsyncState;
//            Socket serverSocket = state.WorkSocket;

//            // ��ȡ���յ�����
//            int bytesRead = serverSocket.EndReceive(ar);
//            if (bytesRead > 0)
//            {
//                string message = Encoding.ASCII.GetString(state.Buffer, 0, bytesRead);
//                Debug.Log("���յ�����Ӧ��" + message);

//                // ������յ�����Ӧ
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
//            // �������ݵ�������
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
//            // ����첽���Ͳ���
//            Socket serverSocket = (Socket)ar.AsyncState;
//            int bytesSent = serverSocket.EndSend(ar);
//            //Debug.Log("������������� " + bytesSent + " �ֽڵ�����");
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
    [Tooltip("Server��IPV4 \n get:'C:\\Windows\\System32\\ipconfig.exe'")]
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
            // �����ͻ���Socket
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // ���ӷ�����
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
            // ����첽���Ӳ���
            clientSocket.EndConnect(ar);

            Debug.Log("�����ӵ���������" + clientSocket.RemoteEndPoint);

            // ��ʼ���շ���������Ӧ
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
        // ����Ƿ������ݿɽ��գ���������������ʱ����
        if (state != null && clientSocket != null && clientSocket.Connected)
        {
            //Receive();
            string jsonData = oVRControllerServer.allStringSend.GetAllString();//!Ϊ����ճ����β��
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
                    Debug.Log("���յ�����Ϣ: " + message);

                    // ������յ�����Ϣmes��%�ָ�
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
