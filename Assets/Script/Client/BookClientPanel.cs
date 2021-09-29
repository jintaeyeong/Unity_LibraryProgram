using UnityEngine;

namespace Frontis.Client
{
    /// <summary>
    /// BookClientPanel ���� ��ũ��Ʈ �Դϴ�
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
