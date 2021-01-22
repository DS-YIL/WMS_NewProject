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

        //Printing label at security - Updated by -> Gayathri

        [HttpPost("printBarcode")]
        public string printBarcode(PrintHistoryModel model)
        {
            string path = Environment.CurrentDirectory + @"\PRNFiles\";
            if(!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            bool result = false;
            string printResult = null;
            path = path + model.inwmasterid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
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



                    //tw.WriteLine("SIZE 94.10 mm, 38 mm");
                    //tw.WriteLine("GAP 3 mm, 0 mm");
                    //tw.WriteLine("DIRECTION 0,0");
                    //tw.WriteLine("REFERENCE 0,0");
                    //tw.WriteLine("OFFSET 0 mm");
                    //tw.WriteLine("SET PEEL OFF");
                    //tw.WriteLine("SET CUTTER OFF");
                    //tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    //tw.WriteLine("SET TEAR ON");
                    //tw.WriteLine("CLS");
                    //tw.WriteLine("QRCODE 359,164,L,5,A,180,M2,S7,\"" + model.po_invoice + "\"");
                    //tw.WriteLine("CODEPAGE 1252");
                    //tw.WriteLine("TEXT 449,49,\"" + 0 + "\",180,18,6,\"" + model.po_invoice + "\"");
                    //tw.WriteLine("PRINT 1,1");



                    tw.WriteLine("SIZE 97.5 mm, 38 mm");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("SET TEAR ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("BOX 10,7,769,296,3");
                    tw.WriteLine("BAR 11,63, 757, 3");
                    tw.WriteLine("BAR 164,64, 3, 224");
                    tw.WriteLine("BAR 165,109, 603, 3");
                    tw.WriteLine("BAR 165,153, 603, 3");
                    tw.WriteLine("BAR 165,245, 603, 3");
                    tw.WriteLine("BAR 572,6, 3, 287");
                    tw.WriteLine("CODEPAGE 1252");
                    tw.WriteLine("TEXT 762,271,\"ROMAN.TTF\",180,1,8,\"Gate Entry No.\"");
                    tw.WriteLine("TEXT 766,213,\"ROMAN.TTF\",180,1,8,\"PO No.\"");
                    tw.WriteLine("TEXT 762,142,\"ROMAN.TTF\",180,1,8,\"Gate Entry Time\"");
                    tw.WriteLine("TEXT 762,98,\"ROMAN.TTF\",180,1,8,\"Vehicle No.\"");
                    tw.WriteLine("TEXT 762,46,\"ROMAN.TTF\",180,1,8,\"Transporter Details\"");
                    tw.WriteLine("TEXT 567,281,\"ROMAN.TTF\",180,1,8,\""+model.inwmasterid+"\"");
                    tw.WriteLine("TEXT 567,240,\"ROMAN.TTF\",180,1,8,\""+model.pono+"\"");
                    tw.WriteLine("TEXT 567,144,\"ROMAN.TTF\",180,1,8,\""+model.gateentrytime+"\"");
                    tw.WriteLine("TEXT 567,98,\"ROMAN.TTF\",180,1,8,\""+model.vehicleno+"\"");
                    tw.WriteLine("TEXT 567,59,\"ROMAN.TTF\",180,1,8,\""+model.transporterdetails+"\"");
                    tw.WriteLine("QRCODE 117,239,L,3,A,180,M2,S7,\""+model.inwmasterid+"\"");
                    tw.WriteLine("TEXT 158,158,\"ROMAN.TTF\",180,1,7,\""+ model.inwmasterid + "\"");
                    tw.WriteLine("PRINT 1,1");

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
                printResult = objIdentification.PrintQRCode(path, printerName);
                // path =  @"D:\Transmitter\ECheckSheetAPI\ECheckSheetAPI\print\";
               // result = RawPrinterHelper.SendFileToPrinter(printerName, path);



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

        [HttpPost("printBinLabel")]
        public string printBinLabel(locationBarcode model)
        {
            string path = Environment.CurrentDirectory + @"\PRNFiles\";
            if (!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            bool result = false;
            string printResult = null;
            path = path + model.rackid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
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


                    tw.WriteLine("SIZE 17.5 mm, 20 mm");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("SET TEAR ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("QRCODE 119,143,L,4,A,180,M2,S7,\"" + model.locatorid +"-"+model.rackid+"-"+model.binid+ "\"");
                    tw.WriteLine("CODEPAGE 1252");
                    tw.WriteLine("TEXT 119,37,\"ROMAN.TTF\",180,1,6,\"" + model.binid + "\"");
                    tw.WriteLine("PRINT 1,1");

                }


            }
            
            try
            {
                string printerName = "10.29.2.48";
                PrintUtilities objIdentification = new PrintUtilities();
                printResult = "success";
                printResult = objIdentification.PrintQRCode(path, printerName);



            }

            catch (Exception ex)
            {
                throw ex;
            }

            if (printResult == "success")
            {
                //update count wms_reprinthistory table            
               // this._poService.updateSecurityPrintHistory(model);
                return "success";
            }
            else
            {
                return "Error Occured";
            }
        }


        [HttpPost("printLabel")]
        public string printLabel(printMaterial printMat)
        {
            string path = Environment.CurrentDirectory + @"\PRNFiles\";
            if (!Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            bool result = false;
            string printResult = null;
            path = path + printMat.materialid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
            FileMode fileType = FileMode.OpenOrCreate;
            //for (int i = 0; i < printQty; i++)
            //{
            // if (File.Exists(path))

            for(int i=1;i<=printMat.noofprint;i++)
            {
                if (System.IO.File.Exists(path))
                {
                    fileType = FileMode.Append;
                }


                using (FileStream fs = new FileStream(path, fileType))
                {
                    using (TextWriter tw = new StreamWriter(fs))
                    {
                        if (printMat.grnno != null)
                        {
                            tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
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
                            tw.WriteLine("BOX 9,15,741,289,3");
                            tw.WriteLine("BAR 492,15, 3, 272");
                            tw.WriteLine("BAR 182,15, 3, 272");
                            tw.WriteLine("BAR 183,222, 557, 3");
                            tw.WriteLine("BAR 9,151, 731, 3");
                            tw.WriteLine("BAR 183,86, 557, 3");
                            tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
                            tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 731,268,\"0\",180,9,9,\"Material Code: \"");
                            tw.WriteLine("TEXT 731,195,\"0\",180,8,9,\"Received Date: \"");
                            tw.WriteLine("TEXT 732,124,\"0\",180,6,6,\"WMS GRN No. - Material Code: \"");
                            tw.WriteLine("TEXT 704,56,\"0\",180,9,9,\"Quantity\"");
                            tw.WriteLine("TEXT 482,265,\"0\",180,14,9,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 484,124,\"0\",180,9,6,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 486,59,\"0\",180,13,9,\"" + i + "/" + printMat.noofprint + "\"");
                            tw.WriteLine("TEXT 485,199,\"0\",180,13,11,\"" + printMat.receiveddate + "\"");

                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");





                        }
                        else
                        {
                            tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
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
                            tw.WriteLine("BOX 9,15,741,289,3");
                            tw.WriteLine("BAR 492,15, 3, 272");
                            tw.WriteLine("BAR 172,15, 3, 272");
                            tw.WriteLine("BAR 172,217, 568, 3");
                            tw.WriteLine("BAR 172,75, 568, 3");
                            //tw.WriteLine("BAR 183,86, 557, 3");
                            tw.WriteLine("QRCODE 144,192,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 731,264,\"0\",180,9,9,\"Material Code: \"");
                            tw.WriteLine("TEXT 731,188,\"0\",180,8,9,\"Received Date: \"");
                            tw.WriteLine("TEXT 704,60,\"0\",180,9,9,\"Quantity\"");
                            tw.WriteLine("TEXT 482,259,\"0\",180,14,9,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 486,60,\"0\",180,13,9,\"" + i+ "/" + printMat.noofprint + "\"");
                            tw.WriteLine("TEXT 485,189,\"0\",180,13,11,\"" + printMat.receiveddate + "\"");
                            tw.WriteLine("BAR 172,143, 568, 3");
                            tw.WriteLine("TEXT 731,116,\"ROMAN.TTF\",180,1,8,\"PO No. - Invoice No.\"");
                            tw.WriteLine("TEXT 481,116,\"0\",180,11,7,\""+printMat.pono+"-"+printMat.invoiceno+"\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }
                    }


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
                printResult = objIdentification.PrintQRCode(path, printerName);
                // path =  @"D:\Transmitter\ECheckSheetAPI\ECheckSheetAPI\print\";
                // result = RawPrinterHelper.SendFileToPrinter(printerName, path);



            }

            catch (Exception ex)
            {
                throw ex;
            }

            if (printResult == "success")
            {
                //update count wms_reprinthistory table            
                this._poService.updateQRcodePrintHistory(printMat);
                return "success";
            }
            else
            {
                return "Error Occured";
            }
        }
    }
}
