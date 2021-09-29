using Frontis.Data;
using Frontis.Global;
using Frontis.TcpNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;


namespace Frontis.Client
{
    /// <summary>
    /// BookClientPanel.Controller 
    /// 데이터, 서버 통신, 파일 저장을 관리하는 메인 로직을 담당합니다
    /// </summary>
    partial class BookClientPanel : MonoBehaviour
    {
        [SerializeField]
        private TransferClient    transferClient = null;

        [SerializeField]
        private ClientBookRecordManager bookRecordManager = null;

        [SerializeField]
        private GameObject rightPanel = null;

        [SerializeField]
        private string serverIP = string.Empty;

        private List<int> splitBookCounts = new List<int>();

        private IPAddress serverIPAddress = null;

        private readonly int ServerPort = 9112;

        private readonly string ReceivejsonFolderName = "도서 사용자 앱 프로그램/Json 파일";
        private readonly string ReceiveImageFolderName = "도서 사용자 앱 프로그램/Image 파일";

        private int currentPageNumber = 0;


        #region public 

        public void OnClickRentButton()
        {

            if (BookDataManager.Instance.BooksInformation[currentPageNumber].BookCount == splitBookCounts[currentPageNumber])
            {
                MyDebug.Log("현재 대출 가능한 책의 개수를 넘음");
                return;
            }

            RentData rentData = new RentData();

            // RentData에 존재하지 않으면 빌릴 수 있음 
            if (BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas.Count == 0)
            {
                rentData.ClientName = ClientDataManager.Instance.ClientName;
                rentData.RentDay = DateTime.Now.ToString();
                rentData.ReturnDay = string.Empty;

                BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas.Add(rentData);

                splitBookCounts[currentPageNumber] = splitBookCounts[currentPageNumber] + 1;

                // 도서관 서버에게 대여정보 전송
                SendRentData(bookName, rentData.ClientName, DateTime.Now.ToString());

                UISettingPanel(currentPageNumber, true, false);

                bookRecordManager.UpdateClientBookRecord();
            }
            else
            {
                for (int i = 0; i < BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas.Count; i++)
                {

                    // 대여 날짜는 존재하지만 반납을 안 했을 때는 대여 불가
                    if (BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].RentDay != string.Empty 
                        && BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].ReturnDay == string.Empty)
                    {
                        Debug.Log($" 반납을 안해서 또 대여를 할 수 없음  {i}");
                        break;
                    }
                    else
                    {
                        rentData.ClientName = ClientDataManager.Instance.ClientName;
                        rentData.RentDay = DateTime.Now.ToString();
                        rentData.ReturnDay = string.Empty;

                        BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas.Add(rentData);

                        splitBookCounts[currentPageNumber] = splitBookCounts[currentPageNumber] + 1;

                        // 도서관 서버에게 대여정보 전송
                        SendRentData(bookName, rentData.ClientName, DateTime.Now.ToString());

                        UISettingPanel(currentPageNumber, true, false);

                        bookRecordManager.UpdateClientBookRecord();

                        break;
                    }

                }

            }

        }


        /// <summary>
        /// 반납 버튼을 클릭했을 때 호출
        /// </summary>
        public void OnClickReturnButton()
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas.Count; i++)
            {
                // 대여는 한 상태
                if (BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].ReturnDay == string.Empty)
                {
                    BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].ReturnDay = DateTime.Now.ToString();

                    splitBookCounts[currentPageNumber] = splitBookCounts[currentPageNumber] - 1;

                    SendReturnData(BookDataManager.Instance.BooksInformation[currentPageNumber].BookName,
                                            BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].ClientName,
                                            BookDataManager.Instance.BooksInformation[currentPageNumber].RentDatas[i].ReturnDay);

                    UISettingPanel(currentPageNumber, false, true);

                    bookRecordManager.UpdateClientBookRecord();

                    break;
                }
            }

        }

        /// <summary>
        /// 다음 페이지 버튼을 클릭했을 때 호출
        /// </summary>
        public void OnClickNextButton()
        {
            if (currentPageNumber < BookDataManager.Instance.BooksInformation.Count - 1)
            {
                currentPageNumber++;

                UISettingPanel(currentPageNumber, false, false);
            }
        }

        /// <summary>
        /// 이전 페이지 버튼을 클릭했을 때 호출
        /// </summary>
        public void OnClickPreviousButton()
        {
            if (currentPageNumber > 0)
            {
                currentPageNumber--;

                UISettingPanel(currentPageNumber, false, false);
            }
        }


        /// <summary>
        /// 기록 버튼을 클릭했을 때 호출 
        /// </summary>
        public void OnClickRecordButton()
        {
            if (rightPanel.activeSelf)
            {
                rightPanel.SetActive(false);
            }
            else
            {
                bookRecordManager.UpdateClientBookRecord();

                rightPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 사용자 이름을 입력하고 확인 버튼을 눌렀을 때 호출
        /// </summary>
        public void OnClickClientDataSaveButton()
        {

            if (inputTextField.text != ClientDataManager.Instance.ClientName && inputTextField.text != string.Empty)
            {
                ClientDataManager.Instance.ClientDataSave(inputTextField.text);
                ClientDataManager.Instance.ClientName = inputTextField.text;
            }
            else
            {
                nameText.text = inputTextField.text;
                ClientDataManager.Instance.ClientName = inputTextField.text;
            }

            firstCanvas.SetActive(false);

            mainCanvas.SetActive(true);

            SendMessageData(inputTextField.text);

            LoadBookImage();
        }

        /// <summary>
        /// 초기화 버튼을 클릭했을 때 대여/반납 기록이 남아있는 도서만 삭제 후 메세지 전송
        /// </summary>
        public void OnClickResetButton()
        {
            SendMessageData("Reset");

            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                for (int j = BookDataManager.Instance.BooksInformation[i].RentDatas.Count - 1; j >= 0; j--)
                {
                    if (BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay != string.Empty)
                    {
                        BookDataManager.Instance.BooksInformation[i].RentDatas.RemoveAt(j);
                    }
                }
            }

            UISettingPanel(currentPageNumber, false, false);

            bookRecordManager.UpdateClientBookRecord();
        }

        #endregion Public

        #region private 

        private void InitializeControl()
        {
            if (!string.IsNullOrEmpty(serverIP))
            {
                serverIPAddress = IPAddress.Parse(serverIP);
            }

            transferClient.RegisterReceiveCallback(OnReceivedFile, OnReceivedMessage, OnReceivedImage, OnReceiveRentData, null);

            ConnectNetwork(ConnectSuccess);

        }

        /// <summary>
        /// 서버로부터 받은 데이터 로드 및 저장
        /// </summary>
        private void DataLoad()
        {
            string jsonPath = Path.Combine(Application.dataPath, ReceivejsonFolderName);

            BookDataManager.Instance.LoadData(jsonPath);
        }

        private void ConnectSuccess()
        {
            if (transferClient.IsConnected)
            {
                SendMessageData("Connect");
                Debug.Log("서버랑 연결 성공했다");
            }
        }

        /// <summary>
        /// 서버 IP와 Port 번호를 가지고 서버와 연결
        /// </summary>
        /// <param name="callback"> 서버 연결 성공 하면 실행되는 Callback 메소드 </param>
        private void ConnectNetwork(Action callback)
        {
            if (string.IsNullOrEmpty(serverIP))
            {
                return;
            }
            else if (serverIPAddress == null)
            {
                serverIPAddress = IPAddress.Parse(serverIP);
            }

            transferClient.Connect(serverIPAddress, ServerPort, callback, CallbackError);
        }

        private void CallbackError(string errorText)
        {
            Debug.Log(errorText);
        }


        /// <summary>
        /// 서버로부터 파일 데이터를 받은 후 호출되는 Callback 메소드 
        /// </summary>
        /// <param name="fileName"></param>
        private void OnReceivedFile(string fileName)
        {
            //Debug.LogFormat($"{fileName} 파일 받음");

            //receiveFileLog.Add(fileName);

        }

        /// <summary>
        /// 서버로부터 메세지를 받은 후 호출되는 Callback 메소드
        /// </summary>
        /// <param name="message"></param>
        private void OnReceivedMessage(string message)
        {
            if (message == "Initialize")
            {
                DataLoad();
            }
            else if (message.Split(',')[0].Equals("bookCount"))
            {
                ReceiveBookCount(message);
            }
        }

        /// <summary>
        /// 서버로부터 책 권수 정보 메세지를 받았을 때 호출되는 메소드  
        /// </summary>
        /// <param name="message"></param>
        private void ReceiveBookCount(string message)
        {
            string[] bookCounts = message.Split(',');
            splitBookCounts.Clear();

            for (int i = 0; i < bookCounts.Length; i++)
            {
                if (i == BookDataManager.Instance.BooksInformation.Count + 1)
                {
                    UISettingPanel(currentPageNumber, false, false);
                    return;
                }

                if (bookCounts[i] != "bookCount" && bookCounts[i] != null)
                {
                    splitBookCounts.Add(int.Parse(bookCounts[i]));
                }
            }

        }


        /// <summary>
        /// 서버로부터 이미지를 전달받았을 때 호출되는 Callback 메소드 
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="data"></param>
        private void OnReceivedImage(string imageName, Texture2D data)
        {
            //Debug.LogFormat($"imageName : {imageName}");

            //receiveImageLog.Add(imageName);
        }

        /// <summary>
        /// 서버로부터 대여했던 책의 데이터를 받은 후 호출되는 Callback 메소드 
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="jsonStringData"></param>
        private void OnReceiveRentData(string clientName, string jsonStringData)
        {
            Debug.Log($"clientName {clientName}, jsonStringData {jsonStringData}");

            char seperator = '=';
            string[] jsonStringArray = jsonStringData.Split(seperator);

            for (int i = 0; i < jsonStringArray.Length - 1; i++)
            {
                UserRentData rentData = JsonUtility.FromJson<UserRentData>(jsonStringArray[i]);

                AddBookData(rentData.BookName, clientName, rentData.RentDay, rentData.ReturnDay);
            }
        }


        private void AddBookData(string bookName, string clientName, string rentDay, string returnDay)
        {
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                if (BookDataManager.Instance.BooksInformation[i].BookName.Equals(bookName))
                {
                    RentData rentdata = new RentData();
                    rentdata.ClientName = clientName;
                    rentdata.RentDay = rentDay;
                    rentdata.ReturnDay = returnDay;

                    BookDataManager.Instance.BooksInformation[i].RentDatas.Add(rentdata);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        private void SendMessageData(string Text)
        {
            Text = Text.Replace(" ", string.Empty);

            byte[] messageData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(Text);
            int totalDataSize = transferClient.HeaderSize + messageData.Length;
            byte[] headerData = transferClient.ConvertToHeader(TransferDataType.Message, totalDataSize);
            byte[] sendData = new byte[transferClient.HeaderSize + messageData.Length];

            int offset = 0;

            Array.Copy(headerData, 0, sendData, 0, headerData.Length);
            offset += headerData.Length;

            Array.Copy(messageData, 0, sendData, offset, messageData.Length);


            transferClient.Send(sendData, CallbackError);
        }

        private void SendRentData(string bookName, string clientName, string rentDay)
        {
            byte[] bookNameData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(bookName);
            byte[] clientNameData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(clientName);
            byte[] rentDayData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(rentDay.ToString());

            int totalDataSize = transferClient.HeaderSize + bookNameData.Length + clientNameData.Length + rentDayData.Length;

            byte[] headerData = transferClient.ConvertToClientHeader(TransferDataType.Rent, bookName, clientName, rentDay, totalDataSize);

            byte[] sendData = new byte[totalDataSize];

            int offset = 0;

            Array.Copy(headerData, 0, sendData, offset, headerData.Length);
            offset += headerData.Length;

            Array.Copy(bookNameData, 0, sendData, offset, bookNameData.Length);
            offset += bookNameData.Length;

            Array.Copy(clientNameData, 0, sendData, offset, clientNameData.Length);
            offset += clientNameData.Length;

            Array.Copy(rentDayData, 0, sendData, offset, rentDayData.Length);

            transferClient.Send(sendData, CallbackError);

        }

        private void SendReturnData(string bookName, string clientName, string returnday)
        {
            byte[] bookNameData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(bookName);
            byte[] clientNameData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(clientName);
            byte[] returnDayData = Encoding.GetEncoding(transferClient.CodePage).GetBytes(returnday.ToString());

            int totalDataSize = transferClient.HeaderSize + bookNameData.Length + clientNameData.Length + returnDayData.Length;

            byte[] headerData = transferClient.ConvertToClientHeader(TransferDataType.Return, bookName, clientName, returnday, totalDataSize);

            byte[] sendData = new byte[totalDataSize];

            int offset = 0;

            Array.Copy(headerData, 0, sendData, offset, headerData.Length);
            offset += headerData.Length;

            Array.Copy(bookNameData, 0, sendData, offset, bookNameData.Length);
            offset += bookNameData.Length;

            Array.Copy(clientNameData, 0, sendData, offset, clientNameData.Length);
            offset += clientNameData.Length;

            Array.Copy(returnDayData, 0, sendData, offset, returnDayData.Length);

            transferClient.Send(sendData, CallbackError);
        }

        #endregion private 


    }

}
