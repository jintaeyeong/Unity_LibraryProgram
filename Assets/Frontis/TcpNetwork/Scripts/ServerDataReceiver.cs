using System;
using System.Text;
using System.IO;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    public class ServerDataReceiver : BaseDataReceiver
    {
        /// <summary>
        /// ������ ������ ������ ��Ŷ�� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessFile(byte[] header, byte[] body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ���ŵ� �̹����� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessImage(byte[] header, byte[] body)
        {
            int offset = 8;  // ��ü ������ ũ��� TransferDataType �����ʹ� �ǳʶٰ� ����

            // ���� �̸� ����
            //
            byte[] fileNameLengthData = new byte[4];

            Array.Copy(header, offset, fileNameLengthData, 0, fileNameLengthData.Length);
            offset += fileNameLengthData.Length;

            int fileNameLength = BitConverter.ToInt32(fileNameLengthData, 0);

            // ���� �̸�
            //
            byte[] fileNameData = new byte[fileNameLength];

            Array.Copy(header, offset, fileNameData, 0, fileNameData.Length);

            string fileName = Encoding.GetEncoding(codePage).GetString(fileNameData);

            // �ؽ�ó ����
            //
            Texture2D receiveTexture = new Texture2D(defaultTextureWidth, defaultTextureHeight, TextureFormat.ARGB32, false); ;

            receiveTexture.LoadImage(body);

            receiveImageCallback?.Invoke(fileName, receiveTexture);
        }

        /// <summary>
        /// ���ŵ� �޽����� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessMessage(byte[] header, byte[] body)
        {
            string message = Encoding.GetEncoding(codePage).GetString(body);

            //Debug.LogFormat($"message {message},message.Length {message.Length}, body {body.Length}");
            receiveMessageCallback?.Invoke(message);
        }

        /// <summary>
        /// ���ŵ� �뿩 ������, ������̸�, �ݳ���¥ �����͸� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessRentData(byte[] header, byte[] body)
        {
            int offset = 8;  // ��ü ������ ũ��� TransferDataType �����ʹ� �ǳʶٰ� ����

            // ���� �̸� ����
            byte[] bookNameLength = new byte[4];

            Array.Copy(header, offset, bookNameLength, 0, bookNameLength.Length);
            offset += bookNameLength.Length;

            int bookStringLength = BitConverter.ToInt32(bookNameLength, 0);

            // ���� �̸�
            //
            byte[] BookNameData = new byte[bookStringLength];

            Array.Copy(header, offset, BookNameData, 0, BookNameData.Length);
            offset += BookNameData.Length;

            string bookName = Encoding.GetEncoding(codePage).GetString(BookNameData);
            receiveRentcallback?.Invoke("bookName", bookName);

            // ����� �̸� ����
            byte[] clientNameLength = new byte[4];

            Array.Copy(header, offset, clientNameLength, 0, clientNameLength.Length);
            offset += clientNameLength.Length;

            int clientStringLength = BitConverter.ToInt32(clientNameLength, 0);

            // ����� �̸�
            //
            byte[] clientNameData = new byte[clientStringLength];

            Array.Copy(header, offset, clientNameData, 0, clientNameData.Length);
            offset += clientNameData.Length;

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);
            receiveRentcallback?.Invoke("clientName", clientName);
            // �뿩 ��¥ ���� 
            byte[] rentDayLength = new byte[4];

            Array.Copy(header, offset, rentDayLength, 0, rentDayLength.Length);
            offset += rentDayLength.Length;

            int rentDayStringLength = BitConverter.ToInt32(rentDayLength, 0);

            // �뿩 ��¥ 
            //
            byte[] rentDayData = new byte[rentDayStringLength];

            Array.Copy(header, offset, rentDayData, 0, rentDayData.Length);

            string rentDay = Encoding.GetEncoding(codePage).GetString(rentDayData);
            receiveRentcallback?.Invoke("rentDay", rentDay);




        }

        /// <summary>
        /// ���ŵ� �뿩 ������, ������̸�, �ݳ���¥ �����͸� ó���մϴ�
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessReturnData(byte[] header, byte[] body)
        {
            int offset = 8;  // ��ü ������ ũ��� TransferDataType �����ʹ� �ǳʶٰ� ����

            // ������ ����
            byte[] bookNameLength = new byte[4];

            Array.Copy(header, offset, bookNameLength, 0, bookNameLength.Length);
            offset += bookNameLength.Length;

            int bookStringLength = BitConverter.ToInt32(bookNameLength, 0);

            // ������
            //
            byte[] BookNameData = new byte[bookStringLength];

            Array.Copy(header, offset, BookNameData, 0, BookNameData.Length);
            offset += BookNameData.Length;

            string bookName = Encoding.GetEncoding(codePage).GetString(BookNameData);
            receiveReturncallback?.Invoke("bookName", bookName);

            // ����� �̸� ����
            byte[] clientNameLength = new byte[4];

            Array.Copy(header, offset, clientNameLength, 0, clientNameLength.Length);
            offset += clientNameLength.Length;

            int clientStringLength = BitConverter.ToInt32(clientNameLength, 0);

            // ����� �̸�
            //
            byte[] clientNameData = new byte[clientStringLength];

            Array.Copy(header, offset, clientNameData, 0, clientNameData.Length);
            offset += clientNameData.Length;

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);
            receiveReturncallback?.Invoke("clientName", clientName);

            // �ݳ� ��¥ ���� 
            byte[] returnDayLength = new byte[4];

            Array.Copy(header, offset, returnDayLength, 0, returnDayLength.Length);
            offset += returnDayLength.Length;

            int ReturnDayStringLength = BitConverter.ToInt32(returnDayLength, 0);

            // �ݳ� ��¥ 
            byte[] returnDayData = new byte[ReturnDayStringLength];

            Array.Copy(header, offset, returnDayData, 0, returnDayData.Length);

            string returnDay = Encoding.GetEncoding(codePage).GetString(returnDayData);
            receiveReturncallback?.Invoke("returnDay", returnDay);


        }
    }

}
