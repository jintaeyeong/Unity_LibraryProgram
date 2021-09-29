using UnityEngine;

namespace Frontis.Client
{
    /// <summary>
    /// BookClientPanel 메인 스크립트 입니다
    /// </summary>
    partial class BookClientPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject firstCanvas = null;

        [SerializeField]
        private GameObject mainCanvas = null;

        private string bookName = string.Empty;

        #region Unity Method

        void Start()
        {
            ClientDataManager.Instance.InitializeClientData();

            InitializeControl();

            InitializeView();
        }

        #endregion Unity Method


    }



}
