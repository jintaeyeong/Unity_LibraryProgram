using UnityEngine;
using UnityEngine.UI;


namespace Frontis.Server
{
    /// <summary>
    /// 도서관PC(서버)에 표시되는 도서 대여 정보의 개별 아이템입니다.
    /// </summary>
    public class BookUseLog : MonoBehaviour
    {
        [SerializeField]
        private Text userName   = null;

        [SerializeField]
        private Text rentDate   = null;

        [SerializeField]
        private Text returnDate = null;

        public void Initialize(string userName, string rentDate, string returnDate)
        {
            this.userName.text   = userName;
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
