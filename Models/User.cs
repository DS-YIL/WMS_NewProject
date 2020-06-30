using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Models
{
    public class User
    {
		public string employeeno { get; set; }
		public int deptId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string name { get; set; }
        public string pwd { get; set; }
        public string domainid { get; set; }
       
    }
}
