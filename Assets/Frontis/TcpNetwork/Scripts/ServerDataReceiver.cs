using System;
using System.Text;
using System.IO;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    public class ServerDataReceiver : BaseDataReceiver
    {
        /// <summary>
        /// 데이터 종류가 파일인 패킷을 처리합니다.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessFile(byte[] header, byte[] body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 수신된 이미지를 처리합니다.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessImage(byte[] header, byte[] body)
        {
            int offset = 8;  // 전체 데이터 크기와 TransferDataType 데이터는 건너뛰고 시작

            // 파일 이름 길이
            //
            byte[] fileNameLengthData = new byte[4];

            Array.Copy(header, offset, fileNameLengthData, 0, fileNameLengthData.Length);
            offset += fileNameLengthData.Length;

            int fileNameLength = BitConverter.ToInt32(fileNameLengthData, 0);

            // 파일 이름
            //
            byte[] fileNameData = new byte[fileNameLength];

            Array.Copy(header, offset, fileNameData, 0, fileNameData.Length);

            string fileName = Encoding.GetEncoding(codePage).GetString(fileNameData);

            // 텍스처 생성
            //
            Texture2D receiveTexture = new Texture2D(defaultTextureWidth, defaultTextureHeight, TextureFormat.ARGB32, false); ;

            receiveTexture.LoadImage(body);

            receiveImageCallback?.Invoke(fileName, receiveTexture);
        }

        /// <summary>
        /// 수신된 메시지를 처리합니다.
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
        /// 수신된 대여 도서명, 사용자이름, 반납날짜 데이터를 처리합니다.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessRentData(byte[] header, byte[] body)
        {
            int offset = 8;  // 전체 데이터 크기와 TransferDataType 데이터는 건너뛰고 시작

            // 도서 이름 길이
            byte[] bookNameLength = new byte[4];

            Array.Copy(header, offset, bookNameLength, 0, bookNameLength.Length);
            offset += bookNameLength.Length;

            int bookStringLength = BitConverter.ToInt32(bookNameLength, 0);

            // 도서 이름
            //
            byte[] BookNameData = new byte[bookStringLength];

            Array.Copy(header, offset, BookNameData, 0, BookNameData.Length);
            offset += BookNameData.Length;

            string bookName = Encoding.GetEncoding(codePage).GetString(BookNameData);
            receiveRentcallback?.Invoke("bookName", bookName);

            // 사용자 이름 길이
            byte[] clientNameLength = new byte[4];

            Array.Copy(header, offset, clientNameLength, 0, clientNameLength.Length);
            offset += clientNameLength.Length;

            int clientStringLength = BitConverter.ToInt32(clientNameLength, 0);

            // 사용자 이름
            //
            byte[] clientNameData = new byte[clientStringLength];

            Array.Copy(header, offset, clientNameData, 0, clientNameData.Length);
            offset += clientNameData.Length;

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);
            receiveRentcallback?.Invoke("clientName", clientName);
            // 대여 날짜 길이 
            byte[] rentDayLength = new byte[4];

            Array.Copy(header, offset, rentDayLength, 0, rentDayLength.Length);
            offset += rentDayLength.Length;

            int rentDayStringLength = BitConverter.ToInt32(rentDayLength, 0);

            // 대여 날짜 
            //
            byte[] rentDayData = new byte[rentDayStringLength];

            Array.Copy(header, offset, rentDayData, 0, rentDayData.Length);

            string rentDay = Encoding.GetEncoding(codePage).GetString(rentDayData);
            receiveRentcallback?.Invoke("rentDay", rentDay);




        }

        /// <summary>
        /// 수신된 대여 도서명, 사용자이름, 반납날짜 데이터를 처리합니다
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessReturnData(byte[] header, byte[] body)
        {
            int offset = 8;  // 전체 데이터 크기와 TransferDataType 데이터는 건너뛰고 시작

            // 도서명 길이
            byte[] bookNameLength = new byte[4];

            Array.Copy(header, offset, bookNameLength, 0, bookNameLength.Length);
            offset += bookNameLength.Length;

            int bookStringLength = BitConverter.ToInt32(bookNameLength, 0);

            // 도서명
            //
            byte[] BookNameData = new byte[bookStringLength];

            Array.Copy(header, offset, BookNameData, 0, BookNameData.Length);
            offset += BookNameData.Length;

            string bookName = Encoding.GetEncoding(codePage).GetString(BookNameData);
            receiveReturncallback?.Invoke("bookName", bookName);

            // 사용자 이름 길이
            byte[] clientNameLength = new byte[4];

            Array.Copy(header, offset, clientNameLength, 0, clientNameLength.Length);
            offset += clientNameLength.Length;

            int clientStringLength = BitConverter.ToInt32(clientNameLength, 0);

            // 사용자 이름
            //
            byte[] clientNameData = new byte[clientStringLength];

            Array.Copy(header, offset, clientNameData, 0, clientNameData.Length);
            offset += clientNameData.Length;

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);
            receiveReturncallback?.Invoke("clientName", clientName);

            // 반납 날짜 길이 
            byte[] returnDayLength = new byte[4];

            Array.Copy(header, offset, returnDayLength, 0, returnDayLength.Length);
            offset += returnDayLength.Length;

            int ReturnDayStringLength = BitConverter.ToInt32(returnDayLength, 0);

            // 반납 날짜 
            byte[] returnDayData = new byte[ReturnDayStringLength];

            Array.Copy(header, offset, returnDayData, 0, returnDayData.Length);

            string returnDay = Encoding.GetEncoding(codePage).GetString(returnDayData);
            receiveReturncallback?.Invoke("returnDay", returnDay);


        }
    }

}
