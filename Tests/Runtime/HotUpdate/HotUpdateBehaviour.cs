using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class HotUpdateBehaviour : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log(Message());
        }

        public string Message()
        {
            return "Hello World!";
        }
    }
}
