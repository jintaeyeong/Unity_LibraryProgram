using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Frontis.TcpNetwork
{

    /// <summary>
    /// TCP 통신을 담당합니다.
    /// </summary>
    public class TransferServer : TransferBase
    {
        public readonly int     Port        = 9112;

        private TcpListener     tcpListener = null;

        private List<TcpClient> clients     = new List<TcpClient>();
        public List<TcpClient>  Clients { get { return clients; } }



        #region Unity Method

        private void Awake()
        {
            Initialize(ListenForIncommingRequests, new ServerDataReceiver());

            threadListener.Start();
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Update()
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (!clients[i].Connected)
                {
                    clients.RemoveAt(i);
                    break;
                }
            }

            ReceiveData();
        }

        #endregion Unity Method


        #region Public

        public override void Disconnect()
        {
            while(clients.Count > 0)
            {
                if (!clients[0].Connected)
                {
                    clients.RemoveAt(0);
                }
                else
                {
                    clients[0].Close();
                    clients.RemoveAt(0);
                }
            }

            if(threadListener != null)
            {
                threadListener.Abort();
            }

            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }
        }

        public override void Send(byte[] data, Action<string> errorCallback)
        {
            if(clients.Count == 0)
            {
                return;
            }

            try
            {
                NetworkStream stream = clients[0].GetStream();

                if (stream.CanWrite)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (SocketException ex)
            {
                errorCallback?.Invoke(ex.Message);
            }
        }

        #endregion Public



        #region Private

        /// <summary>
        /// 클라이언트로부터 수신된 데이터를 처리합니다.
        /// </summary>
        /// <param name="token"></param>
        private void HandleClientWorker(object token)
        {
            byte[] receiveBuffer = new byte[ReceiveBufferSize];

            using (var client = token as TcpClient)
            {
                using (var stream = client.GetStream())
                {
                    if(stream.CanRead)
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

        /// <summary>
        /// 클라이언트 연결 요청에 대한 수신 대기를 시작합니다.
        /// </summary>
        private void ListenForIncommingRequests()
        {
            tcpListener = new TcpListener(IPAddress.Any, Port);

            tcpListener.Start();

            ThreadPool.QueueUserWorkItem(ListenerWorker, null);
        }

        /// <summary>
        /// 클라이언트 연결 요청을 처리합니다.
        /// </summary>
        /// <param name="token"></param>
        private void ListenerWorker(object token)
        {            
            while (tcpListener != null)
            {
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                
                clients.Add(tcpClient);

                ThreadPool.QueueUserWorkItem(HandleClientWorker, tcpClient);
            }
        }

        private void RemoveClients()
        {

        }

        #endregion Private

    }


}

