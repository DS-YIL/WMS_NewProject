/*
    Name of File : <<PrintController>>  Author :<<Prasanna>>  
    Date of Creation <<09-09-2020>>
    Purpose : <<This is used to write print methods>>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 <change version only if there is major change - new release etc>
    Sourcecode Copyright : Yokogawa India Limited
*/
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Interfaces;
using WMS.Models;

namespace WMS.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PrintController : ControllerBase
	{
        private IPodataService<OpenPoModel> _poService; 
        public PrintController(IPodataService<OpenPoModel> poService)
        {
            _poService = poService;
        }

        [HttpPost("printBarcode")]
        public string printBarcode(PrintHistoryModel model)
        {
            string path = Environment.CurrentDirectory + @"\PRNFiles\";
            bool result = false;
            string printResult = null;
            path = path + model.po_invoice + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
            FileMode fileType = FileMode.OpenOrCreate;
            //for (int i = 0; i < printQty; i++)
            //{
           // if (File.Exists(path))
            if (Directory.Exists(path))
            {
                fileType = FileMode.Append;
            }

            using (FileStream fs = new FileStream(path, fileType))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 30 mm, 30 mm");
                    tw.WriteLine("GAP 3 mm, 0 mm");
                    tw.WriteLine("SET RIBBON ON");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='33.0 mm'></xpml>SET TEAR ON");
                    tw.WriteLine("ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("QRCODE 90,80,L,3,A,0,M2,S7,\"" + model.po_invoice + "\"");

                    tw.WriteLine("PRINT 1,1");
                    tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");
                }


            }
            //}
            try
            {
                //Convert.ToString(ConfigurationManager.AppSettings["PrinterName"].ToString());
                //string printerName = ConfigurationManager.AppSettings["CTMajor_AdminPrinter"].ToString();
                //string printerName = "10.29.11.25";
                string printerName = "10.29.2.48";
                PrintUtilities objIdentification = new PrintUtilities();
                printResult = "success";
                //printResult = objIdentification.PrintIdentificationTag(path, printerName);
                // path =  @"D:\Transmitter\ECheckSheetAPI\ECheckSheetAPI\print\";
                //result = RawPrinterHelper.SendFileToPrinter(printerName, path);



            }

            catch (Exception ex)
            {
                throw ex;
            }

            if (printResult == "success")
            {
                //update count wms_reprinthistory table            
                this._poService.updateSecurityPrintHistory(model);
                return "success";
            }
            else
            {
                return "Error Occured";
            }
        }
    }
}