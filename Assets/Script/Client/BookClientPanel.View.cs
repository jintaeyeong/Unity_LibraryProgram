using Frontis.Data;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Frontis.Client
{
    /// <summary>
    /// BookClientPanel.View 
    /// 클라이언트 Left 패널의 UI를 관리합니다.
    /// </summary>
    partial class BookClientPanel : MonoBehaviour
    {
        [SerializeField]
        private Text bookNameTextObject = null;

        [SerializeField]
        private Text totalBookCount = null;

        [SerializeField]
        private Text pageNumber = null;

        [SerializeField]
        private Text borrowingCount = null;

        [SerializeField]
        private Text nameText = null;

        [SerializeField]
        private RawImage bookImage = null;

        [SerializeField]
        private GameObject borrowPossibility = null;

        [SerializeField]
        private GameObject borrowImpossibility = null;

        [SerializeField]
        private GameObject borrowStateOn = null;

        [SerializeField]
        private GameObject borrowStateOff = null;

        [SerializeField]
        private InputField inputTextField = null;

        private List<Texture2D> bookTexture2D = null;


        #region private 

        /// <summary>
        /// 클라이언트 초기화
        /// </summary>
        private void InitializeView()
        {
            firstCanvas.SetActive(true);

            mainCanvas.SetActive(false);

            if (!string.IsNullOrEmpty(ClientDataManager.Instance.ClientName))
            {
                inputTextField.text = ClientDataManager.Instance.ClientName;

            }

            
        }

        /// <summary>
        /// 클라이언트 대여,반납 상태에 따라 UI 화면을 변경
        /// </summary>
        /// <param name="PageNumber"></param>
        /// <param name="isRent"></param>
        /// <param name="isReturn"></param>
        private void UISettingPanel(int PageNumber, bool isRent, bool isReturn)
        {
            BookDataUIApply(PageNumber);

            if (isRent)
            {
                // 대여를 했다면 
                ShowRentState();
            }
            else if (isReturn)
            {
                // 반납을 했다면
                ShowReturnState();
            }
            else
            {
                // 초기화 Or 페이지 이동을 했다면
                ShowCurrentState(PageNumber);
                bookRecordManager.UpdateClientBookRecord();
            }

        }

        /// <summary>
        /// 전체보유, 대여 상황과 이미지를 설정
        /// </summary>
        /// <param name="number"></param>
        private void BookDataUIApply(int number)
        {
            bookName = BookDataManager.Instance.BooksInformation[number].BookName;
            bookNameTextObject.text = "도서명: " + bookName;
            totalBookCount.text = BookDataManager.Instance.BooksInformation[number].BookCount.ToString() + " 권";

            borrowingCount.text = splitBookCounts[number] + " 권";

            bookImage.texture = bookTexture2D[number];

            pageNumber.text = (number + 1) + "/" + BookDataManager.Instance.BooksInformation.Count;
        }


        /// <summary>
        /// 책을 대여했다면 UI 화면을 변경 함
        /// </summary>
        /// <param name="number"></param>
        private void ShowRentState()
        {
            // 대여 불가상태 / 대출 상태 On
            //
            borrowPossibility.SetActive(false);
            borrowImpossibility.SetActive(true);

            borrowStateOn.SetActive(true);
            borrowStateOff.SetActive(false);
        }

        /// <summary>
        /// 책을 반납했다면 UI 화면을 변경함
        /// </summary>
        /// <param name="number"></param>
        private void ShowReturnState()
        {
            // 대여 가능상태 / 대출상태 off
            //
            borrowPossibility.SetActive(true);
            borrowImpossibility.SetActive(false);

            borrowStateOn.SetActive(false);
            borrowStateOff.SetActive(true);

        }


        /// <summary>
        /// 초기 UI를 설정하거나 페이지를 이동할 때 화면을 변경함
        /// </summary>
        /// <param name="number"></param>
        private void ShowCurrentState(int number)
        {
            int RentDataCount = BookDataManager.Instance.BooksInformation[number].RentDatas.Count;

            if (RentDataCount > 0)
            {
                if (BookDataManager.Instance.BooksInformation[number].RentDatas[RentDataCount - 1].ReturnDay == string.Empty)
                {
                    // 대여 불가상태 / 대출 상태 On
                    //
                    borrowPossibility.SetActive(false);
                    borrowImpossibility.SetActive(true);

                    borrowStateOn.SetActive(true);
                    borrowStateOff.SetActive(false);
                }
                else
                {
                    // 대여 불가상태 / 대출 상태 On
                    //
                    borrowPossibility.SetActive(true);
                    borrowImpossibility.SetActive(false);

                    borrowStateOn.SetActive(false);
                    borrowStateOff.SetActive(true);
                }
            }
            else
            {
                if (splitBookCounts[number] < BookDataManager.Instance.BooksInformation[number].BookCount)
                {
                    // 대여 가능상태 / 대출상태 off
                    //
                    borrowPossibility.SetActive(true);
                    borrowImpossibility.SetActive(false);

                    borrowStateOn.SetActive(false);
                    borrowStateOff.SetActive(true);
                }
                else
                {
                    // 대여 불가상태 / 대출 상태 Off
                    //
                    borrowPossibility.SetActive(false);
                    borrowImpossibility.SetActive(true);

                    borrowStateOn.SetActive(false);
                    borrowStateOff.SetActive(true);
                }
            }
        }

        private void LoadBookImage()
        {
            string extension = ".png";
            bookTexture2D = new List<Texture2D>();

            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                string BookName = BookDataManager.Instance.BooksInformation[i].BookName;
                string imagePath = Path.Combine(Application.dataPath, ReceiveImageFolderName, BookName + extension);

                byte[] byteTexture = File.ReadAllBytes(imagePath);
                Texture2D texture = null;
                if (byteTexture.Length > 0)
                {
                    texture = new Texture2D(0, 0);
                    texture.LoadImage(byteTexture);
                }

                bookTexture2D.Add(texture);
            }
        }

        #endregion Private




    }
}
