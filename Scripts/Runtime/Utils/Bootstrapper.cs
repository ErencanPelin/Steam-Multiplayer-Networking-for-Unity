using UnityEngine;

namespace Multiplayer.Runtime.Utils
{
    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BootLoad()
        {
            Object.Instantiate(Resources.Load<GameObject>("NetworkManager"), Vector3.zero, Quaternion.identity);
        }
    }
}