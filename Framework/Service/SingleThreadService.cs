using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.Framework.Service {
    public class SingleThreadServiceOptions : MultiThreadServiceOptions
    {
        public SingleThreadServiceOptions(String name)
            : base(name)
        {
        }

        public sealed override int MaxWorkerThreads
        {
            get
            {
                return 1;
            }
        }
    }

    public abstract class SingleThreadService : MultiThreadService
    {
        protected sealed override MultiThreadServiceOptions InitMultiThreadServiceOptions()
        {
            return InitSingleThreadServiceOptions();
        }

        protected abstract SingleThreadServiceOptions InitSingleThreadServiceOptions();
    }
}
