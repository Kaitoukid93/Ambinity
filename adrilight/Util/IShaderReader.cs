using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace adrilight.Util
{
    public interface IShaderReader
    {
        bool IsRunning { get; }

        void Run(CancellationToken token);
        void Stop();
        void RefreshReadingState();

    }
}
