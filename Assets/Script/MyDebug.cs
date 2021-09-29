
using UnityEngine;

namespace Frontis.Global
{
    public static class MyDebug
    {
        public static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#endif
        }
    }

}
