using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Interfaces
{
    public interface IErrorLog<T>
    {
        Task<IEnumerable<T>> GetLogData();
    }
}
