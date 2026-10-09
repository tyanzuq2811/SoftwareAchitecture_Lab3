using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDA.Events
{
    public interface IEvent
    {
        DateTime Timestamp { get; }
    }
}

