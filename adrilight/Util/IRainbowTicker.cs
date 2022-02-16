
using System.Threading;

namespace adrilight
    {
        public interface IRainbowTicker
        {
        bool IsRunning { get; }

        void Run(CancellationToken token);
        double StartIndex { get; }
    }


    }


