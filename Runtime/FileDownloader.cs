using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Collections.Concurrent;

namespace UniPurity
{
    public class FileDownloader : IDisposable
    {
        private int mThreadCount;
        private WorkerThread[] mWorkerThreads;
        private ConcurrentQueue<string> mUrls;
        private ConcurrentQueue<(string url, byte[] datas)> mResults;

        public FileDownloader(int threadCount)
        {
            mThreadCount = threadCount;
            mWorkerThreads = new WorkerThread[threadCount];
            mUrls = new ConcurrentQueue<string>();
            mResults = new ConcurrentQueue<(string url, byte[] datas)>();
            for (int i = 0; i < threadCount; ++i)
                mWorkerThreads[i] = new WorkerThread(this);
        }

        public void Dispose()
        {
            mUrls.Clear();
            for (int i = 0, len = mThreadCount; i < len; ++i)
                Interlocked.Exchange(ref mWorkerThreads[i].isRunning, 0);
        }

        public void Deconstruct(out ConcurrentQueue<string> urls, out ConcurrentQueue<(string url, byte[] datas)> results)
        {
            urls = mUrls;
            results = mResults;
        }

        private class WorkerThread
        {
            public Thread thread;
            public WebClient client;
            public FileDownloader threadState;
            public int isRunning;

            public WorkerThread(FileDownloader threadState)
            {
                thread = new Thread(ThreadJob);
                client = new WebClient();
                this.threadState = threadState;
                Interlocked.Exchange(ref isRunning, 1);
                thread.Start(threadState);
            }

            private void ThreadJob(object state)
            {
                var (urls, results) = (state as FileDownloader);
                while (isRunning == 1)
                {
                    string url;
                    if (!urls.TryDequeue(out url))
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    byte[] datas = client.DownloadData(url);
                    results.Enqueue((url, datas));
                }
            }
        }
    }
}