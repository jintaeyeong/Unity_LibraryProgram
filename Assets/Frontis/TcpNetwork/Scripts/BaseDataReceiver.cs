using System;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    /// <summary>
    /// 수신된 패킷을 데이터 종류에 맞게 변환합니다.
    /// </summary>
    public abstract class BaseDataReceiver
    {
        protected readonly int    headerSize           = 128;
        protected readonly int    defaultTextureWidth  = 512;
        protected readonly int    defaultTextureHeight = 512;
        protected readonly string codePage             = "euc-kr";

        //protected readonly string receiveFileFolderName = "File/Receive";  // 이 경로는 프로젝트에 맞게 수정해야 합니다.

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
        /// 수신된 데이터를 종류별로 구분해서 처리할 수 있도록 합니다.
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="data"></param>
        public void ReceiveData(byte[] data)
        {
            int totalBytesLength = 4;
            byte[] typeBytes     = new byte[4];
            byte[] headerData    = new byte[headerSize];
            byte[] bodyData      = new byte[data.Length - headerSize];

            // 수신된 패킷을 [header]와 [body]로 나눠서 복사합니다.
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
