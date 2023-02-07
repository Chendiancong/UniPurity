using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniPurity;

public class LoadDll : MonoBehaviour
{
    private bool inited = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Doit()
    {
        StartCoroutine(Doit2());
    }

    private IEnumerator Doit2()
    {
        if (!inited)
        {
            using (var prepare = new UniPurityPrepare())
            {
                prepare.OnMsg += (msg) => Debug.Log($"message:{msg}");
                prepare.OnError += e => Debug.Log($"error:{e.Status},{e.FileName}");
                prepare.OnProgress += (ref UniPurityPrepare.ProgressInfo pi) =>
                    Debug.Log($"{pi.fileName}, {pi.cur}/{pi.groupTotal}");
                yield return StartCoroutine(prepare.PrepareDlls());
            }
            inited = true;
        }

        Text uiText = transform.GetChild(0).GetComponent<Text>();
        Assembly ass = AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "cdc.UniPurityTest.HotUpdate");
        Type type = ass.GetType("HotUpdate.HotUpdateMain");
        Func<string> action = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), null, type.GetMethod("Message"));
        uiText.text = action();

        type = ass.GetType("HotUpdate.HotUpdateBehaviour");
        GameObject go = new GameObject("New");
        go.AddComponent(type);
    }
}
