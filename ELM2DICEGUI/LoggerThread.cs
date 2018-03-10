using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ELM2DICEGUI
{
    class LoggerThread : Thread
    {
        ELM327 elm;
        int period;

        public LoggerThread(ELM327 elm, int period)
        {
            this.elm = elm;
            this.period = period;
        }

    }
}
