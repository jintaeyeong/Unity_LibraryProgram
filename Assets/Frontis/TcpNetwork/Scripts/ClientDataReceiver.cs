using System;
using System.Text;
using System.IO;
using UnityEngine;


namespace Frontis.TcpNetwork
{
    public class ClientDataReceiver : BaseDataReceiver
    {
        private readonly string receivejsonFolderName = "���� ����� �� ���α׷�/Json ����";
        private readonly string receiveImageFolderName = "���� ����� �� ���α׷�/Image ����";
        
        /// <summary>
        /// ������ ������ ������ ��Ŷ�� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessFile(byte[] header, byte[] body)
        {
            int offset = 8;  // ��ü ������ ũ��� TransferDataType �κ��� �ǳʶٰ� ����

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

            //Debug.Log(fileName);

            // ���� ����
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
        /// ���ŵ� �޽����� ó���մϴ�.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessMessage(byte[] header, byte[] body)
        {
            string message = Encoding.GetEncoding(codePage).GetString(body);

            receiveMessageCallback?.Invoke(message);
        }

        /// <summary>
        /// ���ŵ� Rent �����͸� ó���մϴ�
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        protected override void ProcessRentData(byte[] header, byte[] body)
        {
            //Debug.Log($"header.Length : {header.Length}");
            //Debug.Log($"body.Length : {body.Length}");

            int offset = 8;

            // ����� �̸� ����
            byte[] clientNameLengthData = new byte[4];

            Array.Copy(header, offset, clientNameLengthData, 0, clientNameLengthData.Length);
            offset += clientNameLengthData.Length;

            int clientNameLength = BitConverter.ToInt32(clientNameLengthData, 0);

            //Debug.Log($"clientNameLength : {clientNameLength}");

            // ����� �̸� 
            byte[] clientNameData = new byte[clientNameLength];

            Array.Copy(body, 0, clientNameData, 0, clientNameData.Length);

            string clientName = Encoding.GetEncoding(codePage).GetString(clientNameData);

            //Debug.Log($"clientName {clientName}");

            // json ������ ����
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
