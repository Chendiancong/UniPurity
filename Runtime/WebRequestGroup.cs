using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UniPurity
{
    internal class WebRequestGroup : CustomYieldInstruction, IDisposable
    {
        private HashSet<string> mDoings;
        private Action<UnityWebRequest> mOnComplete;

        public override bool keepWaiting => mDoings.Count > 0;
        public event Action<UnityWebRequest> OnComplete
        {
            add => mOnComplete += value;
            remove => mOnComplete -= value;
        }

        public WebRequestGroup()
        {
            mDoings = new HashSet<string>();
        }

        public UnityWebRequest Request(string urlOrPath)
        {
            var request = UnityWebRequest.Get(urlOrPath);
            mDoings.Add(request.url);
            var op = request.SendWebRequest();
            op.completed += OnRequestComplete;
            return request;
        }

        public void Dispose()
        {

        }

        private void OnRequestComplete(AsyncOperation op)
        {
            var _op = (UnityWebRequestAsyncOperation)op;
            var request = _op.webRequest;
            mDoings.Remove(request.url);
            mOnComplete(request);
            request.Dispose();
        }
    }
}