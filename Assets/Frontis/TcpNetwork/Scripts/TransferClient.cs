using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Frontis.TcpNetwork
{
    /// <summary>
    /// TCP 통신을 담당합니다.
    /// </summary>
    public class TransferClient : TransferBase
    {
        private TcpClient client = null;

        private Queue<string> logs = new Queue<string>();


        public bool IsConnected
        {
            get
            {
                if (client == null || !client.Connected)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }



        #region Unity Method

        private void Awake()
        {
            client = new TcpClient();

            Initialize(ListenForData, new ClientDataReceiver());

            packetReader.LogCallback = OnPacketLog;
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Update()
        {
            ReceiveData();

            DisPlayLog();
        }

        #endregion Unity Method


        #region Public

        public void Connect(IPAddress ip, int port, Action successCallback, Action<string> errorCallback)
        {
            if(IsConnected)
            {
                return;
            }

            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ip, port);

                client.Connect(remoteEP);
                threadListener.Start();

                successCallback?.Invoke();
            }
            catch (Exception ex)
            {
                errorCallback?.Invoke(ex.Message);
            }
        }

        public override void Disconnect()
        {
            if(threadListener != null)
            {
                threadListener.Abort();
            }

            if (client != null)
            {
                if(client.Connected)
                {
                    client.Close();
                }
            }
        }

        public override void Send(byte[] data, Action<string> errorCallback)
        {
            if(!IsConnected)
            {
                return;
            }
            else if (data == null || data.Length == 0)
            {
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();

                // data는 [header] + [body]로 구성되어 있습니다.
                // 
                if(stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch(SocketException ex)
            {
                errorCallback?.Invoke(ex.Message);
            }
        }

        #endregion Public


        #region Private

        /// <summary>
        /// 수신된 데이터를 받습니다.
        /// </summary>
        private void ListenForData()
        {
            byte[] receiveBuffer = new byte[ReceiveBufferSize];

            while (IsConnected)
            {
                using (NetworkStream stream = client.GetStream())
                {
                    if (stream.CanRead)
                    {
                        int length;

                        while (true)
                        {
                            length = stream.Read(receiveBuffer, 0, receiveBuffer.Length);

                            if (length != 0)
                            {
                                // 읽어들인 크기 만큼만 복사해서 전달합니다.
                                //
                                byte[] readBuffer = new byte[length];

                                Array.Copy(receiveBuffer, 0, readBuffer, 0, length);

                                packetReader.Receive(readBuffer);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OnPacketLog(string log)
        {
            logs.Enqueue(log);
        }

        private void DisPlayLog()
        {
            for (int i = 0; i < logs.Count; i++)
            {
                UnityEngine.Debug.Log(logs.Dequeue());
            }
        }

        #endregion Private


    }

}
