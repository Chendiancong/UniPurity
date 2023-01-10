using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotUpdateMain
{
    public void Show()
    {
#if UNITY_EDITOR
#endif
        Debug.Log("Hello World!");
    }
}
