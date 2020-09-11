/*
    Name of File : <<PrintUtilities>>  Author :<<Prasanna>>  
    Date of Creation <<09-09-2020>>
    Purpose : <<This is used to write dependancy mehods of print >>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 <change version only if there is major change - new release etc>
    Sourcecode Copyright : Yokogawa India Limited
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WMS.Common
{
	public class PrintUtilities
	{
        public string PrintIdentificationTag(string path, string printerName)
        {
            try
            {

                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.NoDelay = true;

                IPAddress ip = IPAddress.Parse(printerName);
                //int port = Convert.ToInt32("IP_10.29.2.37");

                IPEndPoint ipep = new IPEndPoint(ip, 9100);
                clientSocket.Connect(ipep);

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);

                clientSocket.Send(fileBytes);
                clientSocket.Close();

                return "success";
            }
            catch (Exception ex)
            {
                return "Error";
            }


        }
    }
}
