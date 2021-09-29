using Frontis.Data;
using Frontis.TcpNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Frontis.Server
{
    public class LibraryServer : MonoBehaviour
    {
        [SerializeField]
        private RawImage bookImage        = null;
                                          
        [SerializeField]                  
        private Text bookNameText         = null;

        [SerializeField]
        private Text totalBookCountText   = null;

        [SerializeField]
        private Text currentBookCountText = null;

        [SerializeField]
        private Text currentPageNumber    = null;

        [SerializeField]
        private Transform rightPanelScrollViewContent = null;

        [SerializeField]
        private Transform rightPanelUnUsedObject      = null;

        [SerializeField]
        private TransferServer transferServer         = null;

        private Queue<BookUseLog> useBookUseLogItem    = new Queue<BookUseLog>();
        private Queue<BookUseLog> unUsedBookUseLogItem = new Queue<BookUseLog>();

        private int pageCount = 0;

        private string bookManageFileFolderPath = string.Empty;
        private string clientName               = string.Empty;

        private 
            string BookUseLogPrefabPath     = "Prefab/Book Use Log";
        private readonly string BookManageFileFolderName = "서버 도서 관리 폴더";

        string receiveBookName      = string.Empty;
        string receiveClientName    = string.Empty;
        string receiveRentDayName   = string.Empty;
        string receiveReturnDayName = string.Empty;

        #region Unity Method
        void Start()
        {
            transferServer.RegisterReceiveCallback(null, OnReceivedMessage, null, OnReceivedRentData, OnReceivedReturnData);

            InitializePath();
            BookDataManager.Instance.InitializeBookData();

            BookDataManager.Instance.LoadData(BookDataManager.Instance.JsonFolderPath);
            InitializeBookImage();

            LoadRentData();

            SettingPanel(pageCount);

        }

        #endregion Unity Method

        #region Private

        private BookUseLog CreateBookUseLogItem()
        {
            GameObject prefab = Resources.Load(BookUseLogPrefabPath) as GameObject;
            GameObject itemObject = Instantiate(prefab, rightPanelScrollViewContent);

            return itemObject.GetComponent<BookUseLog>();
        }

        /// <summary>
        /// 클라이언트로부터 받은 메세지
        /// </summary>
        /// <param name="receiveMessage"></param>
        private void OnReceivedMessage(string receiveMessage)
        {
            if (receiveMessage == "Connect")
            {
                ServerSendDataFile();
                ServerSendImage();
                ServerSendMessage("Initialize");
            }
            else if (receiveMessage == "Reset")
            {

                ReceivedReSetMessage();
            }
            else
            {
                SendUserData(receiveMessage);
                clientName = receiveMessage;
                SendDataBorrowingBookCount();
            }
        }

        /// <summary>
        /// 서버 File 폴더 경로 초기화
        /// </summary>
        private void InitializePath()
        {
            bookManageFileFolderPath = Path.Combine(Application.dataPath, BookManageFileFolderName);

            if (!Directory.Exists(bookManageFileFolderPath))
            {
                Directory.CreateDirectory(bookManageFileFolderPath);
            }
        }

       
        /// <summary>
        /// 서버 책 이미지 데이터 초기화
        /// </summary>
        private void InitializeBookImage()
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                string extension = ".png";
                string imagePath = Path.Combine(BookDataManager.Instance.ImageFolderPath, BookDataManager.Instance.BooksInformation[i].BookName + extension);

                byte[] byteTexture = File.ReadAllBytes(imagePath);
                Texture2D texture = null;

                texture = new Texture2D(2, 2);
                texture.LoadImage(byteTexture);

                Rect rect = new Rect(0, 0, texture.width, texture.height);

                BookDataManager.Instance.BooksInformation[i].BookSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }
        }

        /// <summary>
        /// 서버 앱을 처음 실행했을 때 가지고 있는 Json 데이터를 BookDataManager Information 형식으로 저장 
        /// </summary>
        private void LoadRentData()
        {
            if (Directory.Exists(bookManageFileFolderPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(bookManageFileFolderPath);

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    if (file.Extension.ToLower().CompareTo(".txt") == 0)
                    {
                        string fileName = file.Name.Replace(".txt", "");
                        string filePath = file.FullName;

                        string jsonString = File.ReadAllText(filePath);

                        string[] splitJsonString = jsonString.Split('/');

                        JsonFileConvertToBookData(fileName, splitJsonString);
                    }
                }
            }
        }

        private void JsonFileConvertToBookData(string fileName, string[] splitjsonString)
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                if (fileName == BookDataManager.Instance.BooksInformation[i].BookName)
                {
                    for (int j = 0; j < splitjsonString.Length - 1; j++)
                    {
                        RentData rentData = JsonUtility.FromJson<RentData>(splitjsonString[j]);
                        BookDataManager.Instance.BooksInformation[i].RentDatas.Add(rentData);
                    }
                }
            }
        }


        /// <summary>
        /// 사용자가 대여, 반납한 도서 데이터 전송
        /// </summary>
        /// <param name="clientName"></param>
        private void SendUserData(string clientName)
        {
            List<UserRentData> userRentData = new List<UserRentData>();
            string userRentalDataJson = string.Empty;

            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                for (int j = 0; j < BookDataManager.Instance.BooksInformation[i].RentDatas.Count; j++)
                {
                    if (BookDataManager.Instance.BooksInformation[i].RentDatas[j].ClientName.ToLower() == clientName.ToLower())
                    {
                        UserRentData data = new UserRentData();

                        data.BookName = BookDataManager.Instance.BooksInformation[i].BookName;
                        data.RentDay = BookDataManager.Instance.BooksInformation[i].RentDatas[j].RentDay;
                        data.ReturnDay = BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay;

                        userRentData.Add(data);

                        string jsonString = JsonUtility.ToJson(data, true);
                        userRentalDataJson += jsonString + "=";

                    }
                }
            }

            if (userRentalDataJson == string.Empty)
            {
                Debug.Log("전송 데이터 없음");
            }
            else
            {
                SendToClientRentData(clientName, userRentalDataJson);
            }
        }

        /// <summary>
        /// 전체 책 대여 상황을 String으로 전송
        /// </summary>
        private void SendDataBorrowingBookCount()
        {
            string bookCountText = "bookCount,";

            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                int bookCount = 0;

                for (int j = 0; j < BookDataManager.Instance.BooksInformation[i].RentDatas.Count; j++)
                {
                    if (BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay == string.Empty)
                    {
                        bookCount++;
                    }
                }

                bookCountText += bookCount.ToString() + ",";

            }

            ServerSendMessage(bookCountText);

        }

        /// <summary>
        /// 대여 데이터 받았을 때 불려지는 Callback 메소드
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        private void OnReceivedRentData(string type, string value)
        {
            if (type == "bookName")
            {
                receiveBookName = value;
            }
            else if (type == "clientName")
            {
                receiveClientName = value;
            }
            else if (type == "rentDay")
            {
                receiveRentDayName = value;

                RentDataSave(receiveBookName, receiveClientName, receiveRentDayName);

                SettingPanel(pageCount);

                receiveBookName = string.Empty;
                receiveClientName = string.Empty;
                receiveRentDayName = string.Empty;
            }
        }

        /// <summary>
        /// 반납 데이터 받았을 때 불려지는 Callback 메소드
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        private void OnReceivedReturnData(string type, string value)
        {
 
            if (type == "bookName")
            {
                receiveBookName = value;
            }
            else if (type == "clientName")
            {
                receiveClientName = value;
            }
            else if (type == "returnDay")
            {
                receiveReturnDayName = value;

                ReturnDataSave(receiveBookName, receiveClientName, receiveReturnDayName);

                SettingPanel(pageCount);

                receiveBookName = string.Empty;
                receiveClientName = string.Empty;
                receiveReturnDayName = string.Empty;
            }

        }


        /// <summary>
        /// 대여 정보 데이터를 받으면 현재 가지고 있는 데이터에서 클라이언트의 이름을 검색, Json 형식으로 데이터 저장
        /// </summary>
        /// <param name="bookName"></param>
        /// <param name="clientName"></param>
        /// <param name="rentDay"></param>
        private void RentDataSave(string bookName, string clientName, string rentDay)
        {
            //Debug.LogFormat($"도서 대여 정보");
            //Debug.LogFormat($"bookName {bookName}");
            //Debug.LogFormat($"clientName {clientName}");
            //Debug.LogFormat($"rentDay {rentDay}");

            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                if (BookDataManager.Instance.BooksInformation[i].BookName == bookName)
                {
                    for (int j = 0; j < BookDataManager.Instance.BooksInformation[i].RentDatas.Count; j++)
                    {
                        if(BookDataManager.Instance.BooksInformation[i].RentDatas[j].ClientName == clientName 
                            && BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay == string.Empty)
                        {
                            Debug.Log("대여를 할 수 없음");
                            return;
                        }
                    }

                    RentData rentData = new RentData();

                    rentData.ClientName = clientName.Trim();
                    rentData.RentDay = rentDay;
                    rentData.ReturnDay = string.Empty;

                    BookDataManager.Instance.BooksInformation[i].RentDatas.Add(rentData);

                    SettingPanel(pageCount);

                    // Json 형식으로 데이터 저장
                    RentDataSaveJson(rentData, bookName);

                }

            }
        }

        /// <summary>
        /// 도서 대여 할 때 Json 파일 저장
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bookName"></param>
        private void RentDataSaveJson(RentData data, string bookName)
        {
            string jsonString = JsonUtility.ToJson(data);

            string bookManageFilePath = Path.Combine(bookManageFileFolderPath, bookName + ".txt");

            if (!File.Exists(bookManageFilePath))
            {
                File.WriteAllText(bookManageFilePath, jsonString);
                File.AppendAllText(bookManageFilePath, "/");
            }
            else
            {
                File.AppendAllText(bookManageFilePath, jsonString);
                File.AppendAllText(bookManageFilePath, "/");
            }
        }

        /// <summary>
        /// 도서 반납 할 때 파일 수정
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bookName"></param>
        private void ReturnDataSaveJson(List<RentData> data, string bookName)
        {
            string bookManageFilePath = Path.Combine(bookManageFileFolderPath, bookName + ".txt");

            for (int i = 0; i < data.Count; i++)
            {
                string jsonString = JsonUtility.ToJson(data[i]);

                if (i == 0)
                {
                    File.WriteAllText(bookManageFilePath, jsonString);
                    File.AppendAllText(bookManageFilePath, "/");
                }
                else
                {
                    File.AppendAllText(bookManageFilePath, jsonString);
                    File.AppendAllText(bookManageFilePath, "/");
                }
            }
        }

        /// <summary>
        /// 초기화 할 때 책에 대한 Json 데이터를 새로 저장
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bookName"></param>
        private void ResetDataSaveJson(List<RentData> data, string bookName)
        {
            string bookManageFilePath = Path.Combine(bookManageFileFolderPath, bookName + ".txt");
            Debug.Log(bookManageFilePath);

            if(data.Count == 0)
            {
                File.WriteAllText(bookManageFilePath, "");
            }

            for (int i = 0; i < data.Count; i++)
            {
                string jsonString = JsonUtility.ToJson(data[i]);
                if (i == 0)
                {
                    File.WriteAllText(bookManageFilePath, jsonString);
                    File.AppendAllText(bookManageFilePath, "/");
                }
                else
                {
                    File.AppendAllText(bookManageFilePath, jsonString);
                    File.AppendAllText(bookManageFilePath, "/");
                }
            }
        }


        /// <summary>
        /// 반납했다는 신호를 받았을 때 현재 가지고 있는 데이터에서 클라이언트의 이름을 검색, Json 형식으로 데이터 저장
        /// </summary>
        /// <param name="bookName"></param>
        /// <param name="clientName"></param>
        /// <param name="returnDay"></param> 
        private void ReturnDataSave(string bookName, string clientName, string returnDay)
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                if (BookDataManager.Instance.BooksInformation[i].BookName == bookName)
                {
                    for (int j = 0; j < BookDataManager.Instance.BooksInformation[i].RentDatas.Count; j++)
                    {
                        if (BookDataManager.Instance.BooksInformation[i].RentDatas[j].ClientName == clientName && BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay == string.Empty)
                        {
                            BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay = returnDay;

                            // Json 형식으로 데이터 저장
                            ReturnDataSaveJson(BookDataManager.Instance.BooksInformation[i].RentDatas, bookName);

                            SettingPanel(pageCount);

                            break;
                        }
                    }
                }
            }

        }


        private void SendToClientRentData(string clientName, string jsonString)
        {
            byte[] clientNameData = Encoding.GetEncoding(transferServer.CodePage).GetBytes(clientName);
            byte[] jsonData = Encoding.GetEncoding(transferServer.CodePage).GetBytes(jsonString);

            int totalDataSize = transferServer.HeaderSize + clientNameData.Length + jsonData.Length;

            byte[] headerData = transferServer.ConvertToServerRentHeader(TransferDataType.Rent, clientName, jsonString, totalDataSize);

            byte[] sendData = new byte[totalDataSize];

            int offset = 0;

            Array.Copy(headerData, 0, sendData, 0, headerData.Length);
            offset += headerData.Length;

            Array.Copy(clientNameData, 0, sendData, offset, clientNameData.Length);
            offset += clientNameData.Length;

            Array.Copy(jsonData, 0, sendData, offset, jsonData.Length);

            transferServer.Send(sendData, SendRentDataErrorCallback);
        }

        private void SendRentDataErrorCallback(string error)
        {
            Debug.Log(error);
        }

        private void ServerSendImage()
        {
            string imageFolderPath = BookDataManager.Instance.ImageFolderPath;
            string imagePath = string.Empty;
            string fileName = string.Empty;

            DirectoryInfo directInfo = new DirectoryInfo(imageFolderPath);

            foreach (FileInfo file in directInfo.GetFiles())
            {
                for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
                {
                    if (file.Name.Contains(BookDataManager.Instance.BooksInformation[i].BookName) && file.Extension.Equals(".png"))
                    {
                        fileName = file.Name;
                        imagePath = file.FullName;

                        byte[] fileData = File.ReadAllBytes(imagePath);
                        int totalDataSize = transferServer.HeaderSize + fileData.Length;
                        byte[] headerData = transferServer.ConvertToHeader(TransferDataType.Image, fileName, totalDataSize);
                        byte[] sendData = new byte[totalDataSize];

                        int offset = 0;

                        Array.Copy(headerData, 0, sendData, 0, headerData.Length);
                        offset += headerData.Length;

                        Array.Copy(fileData, 0, sendData, offset, fileData.Length);

                        transferServer.Send(sendData, null);
                    }
                }
            }
        }

        private void ServerSendDataFile()
        {
            string jsonFolderPath = BookDataManager.Instance.JsonFolderPath;
            string jsonFileName = BookDataManager.Instance.JsonFileName;
            string filePath = string.Empty;

            DirectoryInfo directInfo = new DirectoryInfo(jsonFolderPath);

            foreach (FileInfo file in directInfo.GetFiles())
            {
                if (file.Name.Contains(jsonFileName))
                {
                    filePath = file.FullName;

                    byte[] fileData = File.ReadAllBytes(filePath);
                    int totalDataSize = transferServer.HeaderSize + fileData.Length;
                    byte[] headerData = transferServer.ConvertToHeader(TransferDataType.File, file.Name, totalDataSize);
                    byte[] sendData = new byte[totalDataSize];

                    Array.Copy(headerData, 0, sendData, 0, headerData.Length);
                    Array.Copy(fileData, 0, sendData, headerData.Length, fileData.Length);

                    transferServer.Send(sendData, null);
                }
            }
        }

        private void ServerSendMessage(string message)
        {
            byte[] messageData = Encoding.GetEncoding(transferServer.CodePage).GetBytes(message);
            int totalDataSize = transferServer.HeaderSize + messageData.Length;
            byte[] headerData = transferServer.ConvertToHeader(TransferDataType.Message, totalDataSize);
            byte[] sendData = new byte[transferServer.HeaderSize + messageData.Length];

            int offset = 0;

            Array.Copy(headerData, 0, sendData, 0, headerData.Length);
            offset += headerData.Length;

            Array.Copy(messageData, 0, sendData, offset, messageData.Length);

            transferServer.Send(sendData, null);
        }


        /// <summary>
        /// 화면을 초기화합니다.
        /// </summary>
        /// <param name="bookDataPageNumber"></param>
        private void SettingPanel(int bookDataPageNumber)
        {
            bookNameText.text = "도서명 : " + BookDataManager.Instance.BooksInformation[bookDataPageNumber].BookName;

            bookImage.texture = BookDataManager.Instance.BooksInformation[bookDataPageNumber].BookSprite.texture;

            totalBookCountText.text = BookDataManager.Instance.BooksInformation[bookDataPageNumber].BookCount.ToString() + " 권";

            currentBookCountText.text = BorrowingBookCountCalculation(bookDataPageNumber).ToString() + " 권";

            currentPageNumber.text = (bookDataPageNumber + 1) + "/" + BookDataManager.Instance.BooksInformation.Count;

            UpdateRightPanel(bookDataPageNumber);




        }

        /// <summary>
        /// 현재 보고있는 페이지 책의 대여 수를 계산하여 반환합니다.
        /// </summary>
        /// <param name="bookDataPageNumber"></param>
        /// <returns></returns>
        private int BorrowingBookCountCalculation(int bookDataPageNumber)
        {
            int borrowingBookCount = 0;

            for (int i = 0; i < BookDataManager.Instance.BooksInformation[bookDataPageNumber].RentDatas.Count; i++)
            {
                if(BookDataManager.Instance.BooksInformation[bookDataPageNumber].RentDatas[i].ReturnDay == string.Empty)
                {
                    borrowingBookCount++;
                }
            }
            return borrowingBookCount;
        }

        /// <summary>
        /// 초기화 버튼을 받았을 때 사용자가 빌렸던 책 데이터 중에 반납한 것들만 데이터를 삭제
        /// </summary>
        private void ReceivedReSetMessage()
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                for (int j = BookDataManager.Instance.BooksInformation[i].RentDatas.Count - 1; j >= 0; j--)
                {
                    if (BookDataManager.Instance.BooksInformation[i].RentDatas[j].ClientName == clientName && BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay != string.Empty)
                    {
                        BookDataManager.Instance.BooksInformation[i].RentDatas.RemoveAt(j);

                        ResetDataSaveJson(BookDataManager.Instance.BooksInformation[i].RentDatas, BookDataManager.Instance.BooksInformation[i].BookName);
                    }
                }
            }


            SettingPanel(pageCount);
        }


        #endregion Private

        #region Public

        /// <summary>
        /// 서버 화면의 다음 페이지 버튼을 클릭했을 때 호출됨
        /// </summary>
        public void OnClickNextPageButton()
        {
            if (pageCount < BookDataManager.Instance.BooksInformation.Count - 1)
            {
                pageCount++;

                SettingPanel(pageCount);
            }
        }

        /// <summary>
        /// 서버 화면의 이전 페이지 버튼을 클릭했을 때 호출됨
        /// </summary>
        public void OnClickPreviousPageButton()
        {
            if (0 < pageCount)
            {
                pageCount--;

                SettingPanel(pageCount);
            }
        }

        #endregion Public

        #region UI

        /// <summary>
        /// 화면 오른쪽에 표시되는 대여 정보 UI를 갱신합니다.
        /// </summary>
        private void UpdateRightPanel(int pageNumber)
        {
            while(useBookUseLogItem.Count > 0)
            {
                BookUseLog item = useBookUseLogItem.Dequeue();

                item.SetActive(false);
                item.SetParent(rightPanelUnUsedObject);

                unUsedBookUseLogItem.Enqueue(item);
            }

            for (int i = 0; i < BookDataManager.Instance.BooksInformation[pageNumber].RentDatas.Count; i++)
            {
                BookUseLog item;

                if(unUsedBookUseLogItem.Count > 0)
                {
                    item = unUsedBookUseLogItem.Dequeue();
                }
                else
                {
                    item = CreateBookUseLogItem();
                }

                item.Initialize(
                    BookDataManager.Instance.BooksInformation[pageNumber].RentDatas[i].ClientName, 
                    BookDataManager.Instance.BooksInformation[pageNumber].RentDatas[i].RentDay, 
                    BookDataManager.Instance.BooksInformation[pageNumber].RentDatas[i].ReturnDay);

                item.SetParent(rightPanelScrollViewContent);
                useBookUseLogItem.Enqueue(item);
            }
        }

        #endregion UI


    }

}