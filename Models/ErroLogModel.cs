using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Models
{
    public class ErroLogModel
    {
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string ExceptionReason { get; set; }
    }
}
