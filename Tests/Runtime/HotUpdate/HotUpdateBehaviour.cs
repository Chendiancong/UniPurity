using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    public class HotUpdateBehaviour : MonoBehaviour
    {
        void Start()
        {
            List<string> list = new List<string>()
            {
                "Hello",
                " ",
                "world!",
            };
            string str = "";
            foreach (string elem in list)
                str += elem;
            Debug.Log(str);
        }
    }
}
