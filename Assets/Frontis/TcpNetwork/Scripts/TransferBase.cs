using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;


namespace Frontis.TcpNetwork
{

    /// <summary>
    /// 전송 데이터 종류
    /// </summary>
    public enum TransferDataType
    {
        File = 0,
        Image,
        Message,
        Rent,
        Return
    }

    public abstract class TransferBase : MonoBehaviour
    {
        public readonly int    HeaderSize        = 128;
        public readonly int    ReceiveBufferSize = 1048576;  // 1MB

        public readonly string CodePage          = "euc-kr";

        protected Thread       threadListener    = null;
        protected PacketReader packetReader      = null;
        
        private   BaseDataReceiver dataReceiver = null;

        private Queue<byte[]> receiveData = new Queue<byte[]>();


        public abstract void Disconnect();
        public abstract void Send(byte[] data, Action<string> errorCallback);


        #region Public

        /// <summary>
        /// 전송 패킷의 헤더를 구성합니다.
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public byte[] ConvertToHeader(TransferDataType dataType, int totalSize)
        {
            byte[] totalSizeData = BitConverter.GetBytes(totalSize);
            byte[] typeBytes     = BitConverter.GetBytes((int)dataType);
            byte[] headerData    = new byte[HeaderSize];

            int offset = 0;

            Array.Copy(totalSizeData, 0, headerData, offset, totalSizeData.Length);
            offset += totalSizeData.Length;

            Array.Copy(typeBytes, 0, headerData, offset, typeBytes.Length);

            return headerData;
        }


        /// <summary>
        /// 전송 패킷의 헤더를 구성합니다.
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public byte[] ConvertToHeader(TransferDataType dataType, string contents, int totalSize)
        {
            byte[] totalSizeData          = BitConverter.GetBytes(totalSize);
            byte[] typeBytes              = BitConverter.GetBytes((int)dataType);
            byte[] contentsData           = Encoding.GetEncoding(CodePage).GetBytes(contents);
            byte[] contentsDataLengthData = BitConverter.GetBytes(contentsData.Length);
            byte[] headerData             = new byte[HeaderSize];

            int offset = 0;

            Array.Copy(totalSizeData, 0, headerData, offset, totalSizeData.Length);
            offset += totalSizeData.Length;

            Array.Copy(typeBytes, 0, headerData, offset, typeBytes.Length);
            offset += typeBytes.Length;

            Array.Copy(contentsDataLengthData, 0, headerData, offset, contentsDataLengthData.Length);
            offset += contentsDataLengthData.Length;

            Array.Copy(contentsData, 0, headerData, offset, contentsData.Length);

            return headerData;
        }

        public byte[] ConvertToServerRentHeader(TransferDataType dataType, string clientName, string jsonString, int totalSize)
        {
            byte[] totalSizeData = BitConverter.GetBytes(totalSize);
            byte[] typeBytes = BitConverter.GetBytes((int)dataType);
            byte[] clientNameData = Encoding.GetEncoding(CodePage).GetBytes(clientName);
            byte[] clientNameDataLengthData = BitConverter.GetBytes(clientNameData.Length);
            byte[] jsonStringData = Encoding.GetEncoding(CodePage).GetBytes(jsonString);
            byte[] jsonStringDataLengthData = BitConverter.GetBytes(jsonStringData.Length);

            byte[] headerData = new byte[HeaderSize];

            int offset = 0;

            Array.Copy(totalSizeData, 0, headerData, offset, totalSizeData.Length);
            offset += totalSizeData.Length;

            Array.Copy(typeBytes, 0, headerData, offset, typeBytes.Length);
            offset += typeBytes.Length;

            Array.Copy(clientNameDataLengthData, 0, headerData, offset, clientNameDataLengthData.Length);
            offset += clientNameDataLengthData.Length;

            Array.Copy(jsonStringDataLengthData, 0, headerData, offset, clientNameData.Length);

            return headerData;
        }


        public byte[] ConvertToClientHeader(TransferDataType dataType, string bookName, string clientName, string day, int totalSize)
        {
            byte[] totalSizeData = BitConverter.GetBytes(totalSize);

            byte[] typeBytes = BitConverter.GetBytes((int)dataType);

            byte[] bookNameData = Encoding.GetEncoding(CodePage).GetBytes(bookName);
            byte[] bookNameDataLengthData = BitConverter.GetBytes(bookNameData.Length);

            byte[] clientNameData = Encoding.GetEncoding(CodePage).GetBytes(clientName);
            byte[] clientNameDataLengthData = BitConverter.GetBytes(clientNameData.Length);

            byte[] dayData = Encoding.GetEncoding(CodePage).GetBytes(day.ToString());
            byte[] dayDataLengthData = BitConverter.GetBytes(dayData.Length);

            byte[] headerData = new byte[HeaderSize];

            int offset = 0;

            Array.Copy(totalSizeData, 0, headerData, offset, totalSizeData.Length);
            offset += totalSizeData.Length;

            Array.Copy(typeBytes, 0, headerData, offset, typeBytes.Length);
            offset += typeBytes.Length;

            // 도서명 
            //
            Array.Copy(bookNameDataLengthData, 0, headerData, offset, bookNameDataLengthData.Length);
            offset += bookNameDataLengthData.Length;

            Array.Copy(bookNameData, 0, headerData, offset, bookNameData.Length);
            offset += bookNameData.Length;

            // 사용자 이름
            //
            Array.Copy(clientNameDataLengthData, 0, headerData, offset, clientNameDataLengthData.Length);
            offset += clientNameDataLengthData.Length;

            Array.Copy(clientNameData, 0, headerData, offset, clientNameData.Length);
            offset += clientNameData.Length;

            // 대여 Or 반납 날짜 
            //
            Array.Copy(dayDataLengthData, 0, headerData, offset, dayDataLengthData.Length);
            offset += dayDataLengthData.Length;

            Array.Copy(dayData, 0, headerData, offset, dayData.Length);

            return headerData;
        }



        public void RegisterReceiveCallback(Action<string> receiveFileCallback, Action<string> receiveMessageCallback, Action<string, Texture2D> receiveImageCallback, Action<string, string> receiveRentCallback, Action<string, string> receiveReturnCallback)
        {
            dataReceiver.RegisterCallback(receiveFileCallback, receiveMessageCallback, receiveImageCallback, receiveRentCallback, receiveReturnCallback);
        }

        #endregion Public


        #region Protected

        protected void Initialize(ThreadStart startListener, BaseDataReceiver receiver)
        {
            packetReader   = new PacketReader(OnCompletedReceiveData);
            dataReceiver   =  receiver;
            

            threadListener = new Thread(startListener)
            {
                IsBackground = true
            };
        }

        /// <summary>
        /// 수신된 데이터를 처리합니다.
        /// </summary>
        protected void ReceiveData()
        {
            for (int i = 0; i < receiveData.Count; i++)
            {
                dataReceiver.ReceiveData(receiveData.Dequeue());
            }
        }

        #endregion Protected


        #region Private

        private void OnCompletedReceiveData(byte[] data)
        {
            receiveData.Enqueue(data);
        }

        #endregion Private


    }


}
