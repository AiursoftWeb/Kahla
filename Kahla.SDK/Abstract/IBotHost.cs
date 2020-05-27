using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kahla.SDK.Abstract
{
    public interface IBotHost
    {
        SemaphoreSlim ConnectingLock { get; }
        BotBase BuildBot { get; }
    }
}
