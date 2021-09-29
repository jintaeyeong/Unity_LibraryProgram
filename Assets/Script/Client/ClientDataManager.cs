using Frontis.Global;
using System.IO;
using UnityEngine;

namespace Frontis.Client
{
    public class ClientDataManager : Singleton<ClientDataManager>
    {
        public string ClientName = string.Empty;

        private string clientDataFilePath = string.Empty;

        private const string ClientDataFileName = "사용자 이름.txt";

        public void InitializeClientData()
        {
            clientDataFilePath = Path.Combine(Application.dataPath, ClientDataFileName);

            FileInfo clientFile = new FileInfo(clientDataFilePath);

            if (clientFile.Exists)
            {
                ClientDataLoad();
            }

        }

        /// <summary>
        /// 저장된 사용자 이름을 불러옵니다.
        /// </summary>
        private void ClientDataLoad()
        {
            string getName = File.ReadAllText(clientDataFilePath);

            ClientName = getName;
        }

        public void ClientDataSave(string nameText)
        {
            File.WriteAllText(clientDataFilePath, nameText);

            ClientName = nameText;


        }


    }

}
