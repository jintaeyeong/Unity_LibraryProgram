using System;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    /// <summary>
    /// ���ŵ� ��Ŷ�� ������ ������ �°� ��ȯ�մϴ�.
    /// </summary>
    public abstract class BaseDataReceiver
    {
        protected readonly int    headerSize           = 128;
        protected readonly int    defaultTextureWidth  = 512;
        protected readonly int    defaultTextureHeight = 512;
        protected readonly string codePage             = "euc-kr";

        //protected readonly string receiveFileFolderName = "File/Receive";  // �� ��δ� ������Ʈ�� �°� �����ؾ� �մϴ�.

        protected Action<string>                  receiveFileCallback    = null;
        protected Action<string>                  receiveMessageCallback = null;
        protected Action<string, Texture2D>       receiveImageCallback   = null;
        protected Action<string, string>  receiveRentcallback    = null;
        protected Action<string, string>  receiveReturncallback  = null;

        protected abstract void ProcessFile(byte[] header, byte[] body);
        protected abstract void ProcessImage(byte[] header, byte[] body);
        protected abstract void ProcessMessage(byte[] header, byte[] body);
        protected abstract void ProcessRentData(byte[] header, byte[] body);
        protected abstract void ProcessReturnData(byte[] header, byte[] body);


        /// <summary>
        /// ���ŵ� �����͸� �������� �����ؼ� ó���� �� �ֵ��� �մϴ�.
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        public void ReceiveData(byte[] data)
        {
            int totalBytesLength = 4;
            byte[] typeBytes     = new byte[4];
            byte[] headerData    = new byte[headerSize];
            byte[] bodyData      = new byte[data.Length - headerSize];

            // ���ŵ� ��Ŷ�� [header]�� [body]�� ������ �����մϴ�.
            //
            Array.Copy(data, totalBytesLength, typeBytes, 0, typeBytes.Length);
            Array.Copy(data, 0, headerData, 0, headerData.Length);
            Array.Copy(data, headerData.Length, bodyData, 0, bodyData.Length);

            TransferDataType dataType = (TransferDataType)BitConverter.ToInt32(typeBytes, 0);

            //Debug.LogFormat($"datatype {dataType}, data Length {data.Length}"); 

            switch (dataType)
            {
                case TransferDataType.File:
                    ProcessFile(headerData, bodyData);
                    break;

                case TransferDataType.Image:
                    ProcessImage(headerData, bodyData);
                    break;

                case TransferDataType.Message:
                    ProcessMessage(headerData, bodyData);
                    break;

                case TransferDataType.Rent:
                    ProcessRentData(headerData, bodyData);
                    break;

                case TransferDataType.Return:
                    ProcessReturnData(headerData, bodyData);
                    break;
            }
        }

        public void RegisterCallback(Action<string> receiveFileCallback, Action<string> receiveMessageCallback, Action<string, Texture2D> receiveImageCallback, Action<string, string> receiveRentCallback, Action<string, string> receiveReturnCallback)
        {
            this.receiveFileCallback = receiveFileCallback;
            this.receiveImageCallback = receiveImageCallback;
            this.receiveMessageCallback = receiveMessageCallback;
            this.receiveRentcallback  = receiveRentCallback;
            this.receiveReturncallback = receiveReturnCallback;
        }

    }


}
