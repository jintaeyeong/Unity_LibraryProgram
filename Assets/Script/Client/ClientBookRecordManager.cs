using Frontis.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Frontis.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientBookRecordManager : MonoBehaviour
    {

        [SerializeField]
        private Transform rightPanelScrollViewContent = null;

        [SerializeField]
        private Transform unUsedClientBookRecordItemObject = null;

        private Queue<ClientBookRecordItem> useClientBookRecordItem = new Queue<ClientBookRecordItem>();
        private Queue<ClientBookRecordItem> unUsedClientBookRecordItem = new Queue<ClientBookRecordItem>();

        private readonly string ClientBookRecordPrefabPath = "Prefab/Client Book Record";

        /// <summary>
        /// 나의 대출 기록 UI에 표시되는 내용을 갱신합니다.
        /// </summary>
        public void UpdateClientBookRecord()
        {
            //Debug.Log($"useClientBookRecordItem.Count {useClientBookRecordItem.Count}");

            while (useClientBookRecordItem.Count > 0)
            {
                ClientBookRecordItem item = useClientBookRecordItem.Dequeue();

                item.SetActive(false);
                item.SetParent(unUsedClientBookRecordItemObject);

                unUsedClientBookRecordItem.Enqueue(item);
            }

            //Debug.Log($"BookDataManager.Instance.BooksInformation.Count {BookDataManager.Instance.BooksInformation.Count}");
            for (int i = 0; i < BookDataManager.Instance.BooksInformation.Count; i++)
            {
                for (int j = 0; j < BookDataManager.Instance.BooksInformation[i].RentDatas.Count; j++)
                {
                    ClientBookRecordItem item;

                    if (unUsedClientBookRecordItem.Count > 0)
                    {
                        item = unUsedClientBookRecordItem.Dequeue();
                    }
                    else
                    {
                        item = CreateClientBookRecordItem();
                    }

                    item.SetParent(rightPanelScrollViewContent);
                    item.Initialize(
                        BookDataManager.Instance.BooksInformation[i].BookName,
                        BookDataManager.Instance.BooksInformation[i].RentDatas[j].RentDay,
                        BookDataManager.Instance.BooksInformation[i].RentDatas[j].ReturnDay);

                    useClientBookRecordItem.Enqueue(item);
                }
            }
        }

        private ClientBookRecordItem CreateClientBookRecordItem()
        {
            GameObject prefab = Resources.Load(ClientBookRecordPrefabPath) as GameObject;
            GameObject itemObject = Instantiate(prefab, rightPanelScrollViewContent);

            return itemObject.GetComponent<ClientBookRecordItem>();
        }

    }

}
