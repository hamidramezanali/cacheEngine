using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.Caching;

// Improve the performance of the following application !

// Profiling has shown that the application spends pratically all cpu time in the Worker constructor.
// The assignment is to implement a CachingFactory class, that caches workers so they do not have to be
// constructed for every call. No existing code may be changed except to construct a CachingFactory instead of a Factory.
// Keep the existing Factory class so that it is easy to compare the performance of both solutions.

// Assume that workers can be reused safely by multiple threads concurrently. The correct instance for a given key must always be called.

namespace Assignment1 {
     
    // The IWorker interface may NOT be changed
    public interface IWorker {
        // Must be thread safe
        void PerformWork(int[] input);
    }
    // The IFactory interface may NOT be changed
    public interface IFactory {
        // Must be thread safe
        IWorker CreateWorker(int key);
    }
    // The Worker class may NOT be changed. The constructor can be assumed to be thread safe and side-effect free.
    class Worker : IWorker {
        static RSA rsa = new RSACryptoServiceProvider(2048, new CspParameters { Flags = CspProviderFlags.CreateEphemeralKey });
        public Worker(int key) {
            // Totally bogus, only here to simulate CPU load when creating the worker. It is thread safe and side-effect free.
            rsa.SignData(new byte[1024], HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            this.key = key;
        }
        public void PerformWork(int[] input) {
            // Actual work once we have the worker is very light, and thread safe
            Interlocked.Increment(ref input[key]);
        }
        int key;
    }

    class CachingFactory : IFactory
    {



        public IWorker CreateWorker(int key)
        {
            // if worker not exist in the cache
            const string cacheKey = "workers";
            Worker workers = CacheEngine.Get<Worker>(cacheKey);

            if (workers == null)
            {
                Worker w = new Worker(key);

                CacheEngine.Add(cacheKey, w);

                return w;
            }
            else
            {
                return workers;
            }

        }


    }
    // Change this class to implement a worker cache.
    class Factory : IFactory {
        public IWorker CreateWorker(int key) {
            return new Worker(key);
        }

    
    }
    

    // Simple load generator, do NOT change this class
    class Program {

        public const int keys = 1000;
        public const int workers = 10;
        const int work = 1000;

        static void Main(string[] args) {
            Random random = new Random(12345);
            IFactory factory = new CachingFactory();

            Thread[] threads = new Thread[workers];
            int[] stats = new int[keys];

            for (int t = 0; t < workers; ++t) {
                threads[t] = new Thread(() => {
                    for (int i = 0; i < work; ++i) {
                        int key = random.Next(keys);
                        factory.CreateWorker(key).PerformWork(stats);
                    }
                });
            }

            Stopwatch watch = Stopwatch.StartNew();

            for (int t = 0; t < workers; ++t)
                threads[t].Start();

            for (int t = 0; t < workers; ++t)
                threads[t].Join();

            watch.Stop();

            int tot = stats.Sum();

            Console.WriteLine(tot + " work items done in " + watch.ElapsedMilliseconds / 1000.0 + "s, " + tot * 1000.0 / watch.ElapsedMilliseconds + " aggregate calls/s");
        }
    }
}
