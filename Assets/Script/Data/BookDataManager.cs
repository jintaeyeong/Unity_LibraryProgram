using Frontis.Global;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Frontis.Data
{
    #region Json 데이터
    /// <summary>
    /// Json 데이터 배열입니다.
    /// </summary>
    [Serializable]
    public class BookJsonArray
    {
        public BookJson[] Book;
    }

    /// <summary>
    /// Json 데이터 형식입니다.
    /// </summary>
    [Serializable]
    public struct BookJson
    {
        public string name;

        [TextArea]
        public string contents;
    }

    #endregion Json 데이터

    public struct BookDataTypeName
    {
        public static string[] Name = new string[]
        {
            "BookName",
            "TotalCount"
        };
    }

    public enum BookDataType
    {
        None = -1,
        BookName,
        TotalCount,
        Max
    }

    [Serializable]
    public class UserRentData
    {
        public string BookName;
        public string RentDay;
        public string ReturnDay;
    }

    public class BookInformation
    {
        public string BookName { get; set; }
        public Sprite BookSprite { get; set; }
        public int    BookCount { get; set; }

        public List<RentData> RentDatas = new List<RentData>();
    }

    [Serializable]
    public class RentData
    {
        public string ClientName;
        public string RentDay;
        public string ReturnDay;
    }

    public class BookDataManager : Singleton<BookDataManager>
    {
        private string jsonFolderPath = string.Empty;
        private string imageFolderPath = string.Empty;

        private const string JsonResourcePath  = "Data/jsonData/";
        private const string ImageResourcePath = "Data/bookImage/";
        private const string JsonName          = "BookData";
        private const string ExcelFolderName   = "도서Json데이터";
        private const string ImageFolderName   = "도서이미지데이터";

        public List<BookInformation> BooksInformation = new List<BookInformation>();
        public string JsonFolderPath { get { return jsonFolderPath; } }
        public string ImageFolderPath { get { return imageFolderPath; } }
        public string JsonFileName { get { return JsonName; } }


        #region Public

        /// <summary>
        /// 도서 데이터들을 초기화합니다.
        /// </summary>
        public void InitializeBookData()
        {
            InitializePath();
            JsonFileSave();
            ImageFileSave();
        }

        /// <summary>
        /// 데이터를 읽어와서 BookInformation 데이터에 추가
        /// </summary>
        /// <param name="jsonFolderPath"></param>
        public void LoadData(string jsonFolderPath)
        {
            string[] delimiterChars = { ",," };

            string readFilePath = Path.Combine(jsonFolderPath, JsonName + ".json");

            string[] jsonString = File.ReadAllText(readFilePath).Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < jsonString.Length; i++)
            {
                BooksInformation.Add(LoadJsonData(jsonString[i]));
            }
        }

        #endregion Public

        #region Private

        /// <summary>
        /// Json 폴더와 Image 폴더의 경로를 초기화합니다.
        /// </summary>
        private void InitializePath()
        {
            string rootPath = Path.Combine(Application.dataPath, Application.productName);

            jsonFolderPath = Path.Combine(rootPath, ExcelFolderName);
            imageFolderPath = Path.Combine(rootPath, ImageFolderName);
        }

        /// <summary>
        /// Json 파일을 저장합니다.
        /// </summary>
        private void JsonFileSave()
        {
            if (!Directory.Exists(jsonFolderPath))
            {
                Directory.CreateDirectory(jsonFolderPath);

                string textAssetPath = JsonResourcePath + JsonName;
                TextAsset textAsset = Resources.Load<TextAsset>(textAssetPath);
                string writeFilePath = Path.Combine(jsonFolderPath, JsonName + ".json");

                if (textAsset != null)
                {
                    File.WriteAllText(writeFilePath, textAsset.text);
                }

            }
        }

        /// <summary>
        /// Image 파일을 저장합니다.
        /// </summary>
        private void ImageFileSave()
        {
            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);

                string extension = ".png";

                Sprite[] spriteArray = Resources.LoadAll<Sprite>(ImageResourcePath);

                //Debug.LogFormat($"spriteArray {spriteArray.Length}");

                if (spriteArray != null)
                {
                    for (int i = 0; i < spriteArray.Length; i++)
                    {
                        byte[] textureBytes = spriteArray[i].texture.EncodeToPNG();
                        string path = Path.Combine(imageFolderPath, spriteArray[i].name + extension);
                        //Debug.LogFormat($"path : {path}");

                        File.WriteAllBytes(path, textureBytes);
                    }
                }
            }
        }

        /// <summary>
        /// Json 데이터 변환
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        private BookInformation LoadJsonData(string jsonText)
        {
            BookJsonArray jsonData = ConvertJsonData(jsonText);

            return ConvertJsonToList(jsonData);
        }

        private BookInformation ConvertJsonToList(BookJsonArray jsonData)
        {
            string bookName = string.Empty;
            int totalCount  = 0;

            for (int i = 0; i < jsonData.Book.Length; i++)
            {
                if (GetDataType(jsonData.Book[i].name) == BookDataType.BookName)
                {
                    bookName = jsonData.Book[i].contents;
                }
                else if (GetDataType(jsonData.Book[i].name) == BookDataType.TotalCount)
                {
                    totalCount = int.Parse(jsonData.Book[i].contents);
                }
            }

            BookInformation bookInformation = new BookInformation();

            bookInformation.BookName  = bookName;
            bookInformation.BookCount = totalCount;

            return bookInformation;
        }

        public static BookJsonArray ConvertJsonData(string jsonText)
        {
            BookJsonArray BookJsonData = JsonUtility.FromJson<BookJsonArray>(jsonText);

            return BookJsonData;
        }

        private BookDataType GetDataType(string dataName)
        {
            BookDataType dataType = BookDataType.None;

            for (int i = 0; i < BookDataTypeName.Name.Length; i++)
            {
                if (BookDataTypeName.Name[i] == dataName)
                {
                    dataType = (BookDataType)Enum.Parse(typeof(BookDataType), i.ToString());
                }
            }

            return dataType;
        }

        #endregion Private

    }

}
