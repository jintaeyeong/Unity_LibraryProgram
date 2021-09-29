using UnityEngine;
using UnityEngine.UI;


namespace Frontis.Client
{
    /// <summary>
    /// 사용자 앱(클라이언트)에 표시되는 대출 기록의 개별 아이템입니다.
    /// </summary>
    public class ClientBookRecordItem : MonoBehaviour
    {
        [SerializeField]
        private Text bookName = null;

        [SerializeField]
        private Text rentDate = null;

        [SerializeField]
        private Text returnDate = null;

        public void Initialize(string bookName, string rentDate, string returnDate)
        {
            this.bookName.text   = bookName;
            this.rentDate.text   = rentDate;
            this.returnDate.text = returnDate;

            SetActive(true);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }



    }

    


}
