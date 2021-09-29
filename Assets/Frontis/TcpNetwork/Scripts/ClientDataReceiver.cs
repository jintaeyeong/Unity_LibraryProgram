using System;
using System.Text;
using System.IO;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    public class ClientDataReceiver : BaseDataReceiver
    {
        private readonly string receivejsonFolderName = "도서 사용자 앱 프로그램/Json 파일";
        private readonly string receiveImageFolderName = "도서 사용자 앱 프로그램/Image 파일";
        
        /// <summary>
        /// 데이터 종류가 파일인 패킷을 처리합니다.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessFile(byte[] header, byte[] body)
        {
            int offset = 8;  // 전체 데이터 크기와 TransferDataType 부분은 건너뛰고 시작

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

            //Debug.Log(fileName);

            // 파일 저장
            //
            string folderPath = Path.Combine(Application.dataPath, receivejsonFolderName);
            string filePath = Path.Combine(Application.dataPath, receivejsonFolderName, fileName);

            //Debug.LogFormat($"fileName : {fileName}");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllBytes(filePath, body);

            receiveFileCallback?.Invoke(fileName);
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

            string folderPath = Path.Combine(Application.dataPath, receiveImageFolderName);
            string filePath = Path.Combine(Application.dataPath, receiveImageFolderName, fileName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllBytes(filePath, body);

            //receiveImageCallback?.Invoke(fileName, receiveTexture);
        }

        /// <summary>
        /// 수신된 메시지를 처리합니다.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessMessage(byte[] header, byte[] body)
        {
            string message = Encoding.GetEncoding(codePage).GetString(body);

            receiveMessageCallback?.Invoke(message);
        }

        /// <summary>
        /// 수신된 Rent 데이터를 처리합니다
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessRentData(byte[] header, byte[] body)
        {
            //Debug.Log($"header.Length : {header.Length}");
            //Debug.Log($"body.Length : {body.Length}");

            int offset = 8;

            // 사용자 이름 길이
            byte[] clientNameLengthData = new byte[4];

            Array.Copy(header, offset, clientNameLengthData, 0, clientNameLengthData.Length);
            offset += clientNameLengthData.Length;

            int clientNameLength = BitConverter.ToInt32(clientNameLengthData, 0);

            //Debug.Log($"clientNameLength : {clientNameLength}");

            // 사용자 이름 
            byte[] clientNameData = new byte[clientNameLength];

            Array.Copy(body, 0, clientNameData, 0, clientNameData.Length);

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);

            //Debug.Log($"clientName {clientName}");

            // json 데이터 길이
            byte[] jsonStringLegnthbyte = new byte[4];

            Array.Copy(header, offset, jsonStringLegnthbyte, 0, jsonStringLegnthbyte.Length);

            int jsonStringLength = BitConverter.ToInt32(jsonStringLegnthbyte, 0);


            byte[] jsonString = new byte[jsonStringLength];

            Array.Copy(body, clientNameData.Length, jsonString, 0, jsonString.Length);

            string jsonStringData = Encoding.GetEncoding(codePage).GetString(jsonString);

            //Debug.Log($"jsonStringData : {jsonStringData}");

            receiveRentcallback?.Invoke(clientName, jsonStringData);



        }

        protected override void ProcessReturnData(byte[] header, byte[] body)
        {
            throw new NotImplementedException();
        }
    }


}
