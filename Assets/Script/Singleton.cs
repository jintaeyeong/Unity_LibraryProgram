using UnityEngine;


namespace Frontis.Global
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        protected static T instance = null;

        private static bool applicationIsQuitting = false;

        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' 프로그램이 종료되어 이미 삭제됨" + " 다시 생성하지 않음 - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType(typeof(T)) as T;

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError("[Singleton] 문제가 있습니다. " + " - 1개 이상의 싱글톤이 있습니다." + " 씬을 다시 시작하면 해결될 수 있습니다.");

                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject _singleton = new GameObject();   // typeof(T).ToString()

                            instance = _singleton.AddComponent<T>();
                            instance.name = typeof(T).Name;

                            DontDestroyOnLoad(_singleton);
                        }
                        else
                        {
                            Debug.Log("[Singleton] 싱글톤이 이미 생성되었습니다: " + instance.gameObject.name);
                        }
                    }
                }

                return instance;
            }
        }


        protected virtual void Awake()
        {
            if (instance == null)
            {
                applicationIsQuitting = false;
            }
        }


        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }


    }


}
