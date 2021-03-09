/*
    Name of File : <<PrintController>>  Author :<<Prasanna>>  
    Date of Creation <<09-09-2020>>
    Purpose : <<This is used to write print methods>>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 <change version only if there is major change - new release etc>
    Sourcecode Copyright : Yokogawa India Limited
*/
using Microsoft.AspNetCore.Http;
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
        Configurations config = new Configurations();
        ErrorLogTrace log = new ErrorLogTrace();
        private IPodataService<OpenPoModel> _poService;
        string url = "";
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PrintController(IPodataService<OpenPoModel> poService)
        {
            _poService = poService;
            //this._httpContextAccessor = _httpContextAccessor;
            //url = _httpContextAccessor.HttpContext.Request.Host + _httpContextAccessor.HttpContext.Request.Path;
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


                    //Prn code for TSC TE310 printer
                    string formateddate = model.gateentrytime.ToString("dd-MM-yyyy");
                    tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("BOX 21,7,1127,383,4");
                    tw.WriteLine("BAR 23,73, 1103, 4");
                    tw.WriteLine("BAR 244,73, 4, 309");
                    tw.WriteLine("BAR 245,141, 881, 4");
                    tw.WriteLine("BAR 245,206, 881, 4");
                    tw.WriteLine("BAR 245,308, 881, 4");
                    tw.WriteLine("BAR 846,9, 4, 373");
                    tw.WriteLine("CODEPAGE 1252");
                    tw.WriteLine("TEXT 1118,365,\"0\",180,8,8,\"Gate Entry No.\"");
                    tw.WriteLine("TEXT 1118,275,\"0\",180,8,8,\"PO No.\"");
                    tw.WriteLine("TEXT 1118,189,\"0\",180,8,8,\"Gate Entry Time\"");
                    tw.WriteLine("TEXT 1118,125,\"0\",180,8,8,\"Vehicle No.\"");
                    tw.WriteLine("TEXT 1118,53,\"0\",180,8,8,\"Transporter Details\"");
                    tw.WriteLine("TEXT 839,367,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 839,193,\"0\",180,8,8,\"" + formateddate + "\"");
                    tw.WriteLine("TEXT 839,125,\"0\",180,8,8,\"" + model.vehicleno + "\"");
                    tw.WriteLine("TEXT 839,53,\"0\",180,8,8,\"" + model.transporterdetails + "\"");
                    //tw.WriteLine("TEXT 839,48,\"0\",180,8,8,\"" + model.transporterdetails+"\"");
                    tw.WriteLine("QRCODE 175,310,L,4,A,180,M2,S7,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 235,199,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 839,295,\"0\",180,8,8,\"" + model.pono + "\"");
                    //tw.WriteLine("TEXT 839,354,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                    //tw.WriteLine("TEXT 1235,243,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("PRINT 1,1");
                    tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");




                   // //Prn code for TSC TE210 printer

                   // string formateddate = model.gateentrytime.ToString("dd-MM-yyyy");

                   // tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                   //// tw.WriteLine("GAP 3 mm, 0 mm");
                   // tw.WriteLine("DIRECTION 0,0");
                   // tw.WriteLine("REFERENCE 0,0");
                   // tw.WriteLine("OFFSET 0 mm");
                   // tw.WriteLine("SET PEEL OFF");
                   // tw.WriteLine("SET CUTTER OFF");
                   // tw.WriteLine("SET PARTIAL_CUTTER OFF");
                   // tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");
                   // tw.WriteLine("CLS");
                   // tw.WriteLine("BOX 15,5,762,260,3");
                   // tw.WriteLine("BAR 15,49, 746, 3");
                   // tw.WriteLine("BAR 164,49, 3, 209");
                   // tw.WriteLine("BAR 165,95, 596, 3");
                   // tw.WriteLine("BAR 165,139, 596, 3");
                   // tw.WriteLine("BAR 165,208, 596, 3");
                   // tw.WriteLine("BAR 572,6, 3, 252");
                   // tw.WriteLine("CODEPAGE 1252");
                   // tw.WriteLine("TEXT 756,247,\"0\",180,8,8,\"Gate Entry No.\"");
                   // tw.WriteLine("TEXT 756,186,\"0\",180,8,8,\"PO No.\"");
                   // tw.WriteLine("TEXT 756,128,\"0\",180,8,8,\"Gate Entry Time\"");
                   // tw.WriteLine("TEXT 756,84,\"0\",180,8,8,\"Vehicle No.\"");
                   // tw.WriteLine("TEXT 756,35,\"0\",180,8,8,\"Transporter Details\"");
                   // tw.WriteLine("TEXT 567,248,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                   // tw.WriteLine("TEXT 567,130,\"0\",180,8,8,\"" + formateddate + "\"");
                   // tw.WriteLine("TEXT 567,84,\"0\",180,8,8,\"" + model.vehicleno + "\"");
                   // tw.WriteLine("TEXT 567,35,\"0\",180,8,8,\"" + model.transporterdetails + "\"");
                   // //tw.WriteLine("TEXT 839,48,\"0\",180,8,8,\"" + model.transporterdetails+"\"");
                   // tw.WriteLine("QRCODE 117,209,L,3,A,180,M2,S7,\"" + model.inwmasterid + "\"");
                   // tw.WriteLine("TEXT 158,128,\"0\",180,8,7,\"" + model.inwmasterid + "\"");
                   // tw.WriteLine("TEXT 567,199,\"0\",180,8,8,\"" + model.pono + "\"");
                   // //tw.WriteLine("TEXT 839,354,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                   // //tw.WriteLine("TEXT 1235,243,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
                   // tw.WriteLine("PRINT 1,1");
                   // tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                }


            }
            //}
            try
            {
                //Convert.ToString(ConfigurationManager.AppSettings["PrinterName"].ToString());
                //string printerName = ConfigurationManager.AppSettings["CTMajor_AdminPrinter"].ToString();
                //string printerName = "10.29.11.25";

                string printerName = "10.29.2.48";
                printerName = config._GateEntryPrintIP;
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


                    //tw.WriteLine("SIZE 17.5 mm, 20 mm");
                    //tw.WriteLine("DIRECTION 0,0");
                    //tw.WriteLine("REFERENCE 0,0");
                    //tw.WriteLine("OFFSET 0 mm");
                    //tw.WriteLine("SET PEEL OFF");
                    //tw.WriteLine("SET CUTTER OFF");
                    //tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    //tw.WriteLine("SET TEAR ON");
                    //tw.WriteLine("CLS");
                    //tw.WriteLine("QRCODE 119,143,L,4,A,180,M2,S7,\"" + model.locatorid +"-"+model.rackid+"-"+model.binid+ "\"");
                    //tw.WriteLine("CODEPAGE 1252");
                    //tw.WriteLine("TEXT 119,37,\"ROMAN.TTF\",180,1,6,\"" + model.binid + "\"");
                    //tw.WriteLine("PRINT 1,1");



                    tw.WriteLine("<xpml><page quantity='0' pitch='28.0 mm'></xpml>SIZE 77.5 mm, 28 mm");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='28.0 mm'></xpml>SET TEAR ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("BITMAP 42,125,4,144,1,ÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿÿü?ÿ   €  €  À  À  à  à  ð  ð  ø  ø  ?ü  ?ü  þ  þ  ÿÿ  ÿÿ ÿÿ€ÿÿ€ÿÿÀÿÿÀÿÿàÿÿàÿÿðÿÿðÿÿøÿÿø?ÿÿü?ÿÿüÿÿþÿÿþÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿÿ");
                    tw.WriteLine("BARCODE 891,263,\"128M\",123,0,180,2,4,\"!General PCI Impo!100 - FRG\"");
                    tw.WriteLine("CODEPAGE 1252");
                    tw.WriteLine("TEXT 730,132,\"0\",180,5,5,\"General PCI Impo-FRG\"");
                    tw.WriteLine("TEXT 636,81,\"0\",180,8,8,\"Location - Rack\"");
                    tw.WriteLine("PRINT 1,1");
                    tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");


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
            string currentdate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            //Check the length of soline item number and append Zero

            if(printMat.solineitemno!=null)
            {
                if (printMat.solineitemno.Length <= 5)
                {
                    int length = printMat.solineitemno.Length;
                    int countlength = 6 - length;
                    printMat.solineitemno = printMat.solineitemno.PadLeft(6, '0');
                }
            }
           
            //for(int i=1;i<=printMat.noofprint;i++)
            //{
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
                            //tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
                            //tw.WriteLine("GAP 3 mm, 0 mm");
                            //tw.WriteLine("SET RIBBON ON");
                            //tw.WriteLine("DIRECTION 0,0");
                            //tw.WriteLine("REFERENCE 0,0");
                            //tw.WriteLine("OFFSET 0 mm");
                            //tw.WriteLine("SET PEEL OFF");
                            //tw.WriteLine("SET CUTTER OFF");
                            //tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            //tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='33.0 mm'></xpml>SET TEAR ON");
                            //tw.WriteLine("ON");
                            //tw.WriteLine("CLS");
                            //tw.WriteLine("BOX 9,15,741,289,3");
                            //tw.WriteLine("BAR 492,15, 3, 272");
                            //tw.WriteLine("BAR 182,15, 3, 272");
                            //tw.WriteLine("BAR 183,222, 557, 3");
                            //tw.WriteLine("BAR 9,151, 731, 3");
                            //tw.WriteLine("BAR 183,86, 557, 3");
                            //tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
                            //tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
                            //tw.WriteLine("CODEPAGE 1252");
                            //tw.WriteLine("TEXT 731,268,\"0\",180,9,9,\"Material Code: \"");
                            //tw.WriteLine("TEXT 731,195,\"0\",180,8,9,\"Received Date: \"");
                            //tw.WriteLine("TEXT 732,124,\"0\",180,6,6,\"WMS GRN No. - Material Code: \"");
                            //tw.WriteLine("TEXT 704,56,\"0\",180,9,9,\"Quantity\"");
                            //tw.WriteLine("TEXT 482,265,\"0\",180,14,9,\"" + printMat.materialid + "\"");
                            //tw.WriteLine("TEXT 484,124,\"0\",180,9,6,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
                            //tw.WriteLine("TEXT 486,59,\"0\",180,13,9,\"" + i + "/" + printMat.noofprint + "\"");
                            //tw.WriteLine("TEXT 485,199,\"0\",180,13,11,\"" + printMat.receiveddate + "\"");

                            //tw.WriteLine("PRINT 1,1");
                            //tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");


                          if(printMat.codetype=="M")
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 2 mm, 0 mm");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 21,3906, 2261, 8");
                                tw.WriteLine("BAR 27,3667, 2261, 8");
                                tw.WriteLine("BAR 21,3386, 2261, 8");
                                tw.WriteLine("BAR 21,3146, 2261, 8");
                                tw.WriteLine("BAR 21,2989, 2261, 8");
                                tw.WriteLine("BAR 26,2833, 2261, 8");
                                tw.WriteLine("BAR 27,2295, 2261, 8");
                                tw.WriteLine("BOX 22,196,2282,1903,8");
                                tw.WriteLine("BAR 26,1696, 2253, 8");
                                tw.WriteLine("BAR 26,1506, 2253, 8");
                                tw.WriteLine("BAR 26,1296, 2253, 8");
                                tw.WriteLine("BAR 26,1134, 2253, 8");
                                tw.WriteLine("BAR 26,969, 2253, 8");
                                tw.WriteLine("BAR 26,783, 2253, 8");
                                tw.WriteLine("BAR 26,587, 2253, 8");
                                tw.WriteLine("BAR 26,411, 2253, 8");
                                tw.WriteLine("CODEPAGE 1252");
                                tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono+" - "+ printMat.itemno +"\"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                                tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                                tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                                tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                                tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                                tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                                tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                                tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" "+printMat.serialno+" \"");
                                if(printMat.materialid!=null)
                                {
                                    tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                                }
                                
                                tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\""+ printMat.materialid + "\"");
                                tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1697,3492,\"0\",180,8,8,\""+printMat.materialdescription+"\"");
                                tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105"+printMat.saleorderno+ "!100 - !099" + printMat.solineitemno+"\"");
                                tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\""+printMat.saleorderno+"-"+printMat.solineitemno+" \"");
                                tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\""+printMat.noofpieces+"/"+printMat.receivedqty+" ST "+ printMat.boxno+" OF "+printMat.totalboxes+" BOXES\"");
                                tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\""+printMat.saleordertype+"\"");
                                tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                                tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\""+printMat.customercode+" "+printMat.customername+"\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : "+printMat.ygsgr+"\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\""+printMat.plant+"\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                                
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2205,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\""+printMat.shippingpoint+" \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 721,220, 8, 143");
                                tw.WriteLine("BAR 713,220, 4, 143");
                                tw.WriteLine("BAR 693,220, 12, 143");
                                tw.WriteLine("BAR 681,220, 4, 143");
                                tw.WriteLine("BAR 669,220, 8, 143");
                                tw.WriteLine("BAR 649,220, 12, 143");
                                tw.WriteLine("BAR 637,220, 4, 143");
                                tw.WriteLine("BAR 621,220, 4, 143");
                                tw.WriteLine("BAR 609,220, 8, 143");
                                tw.WriteLine("BAR 585,220, 12, 143");
                                tw.WriteLine("BAR 569,220, 4, 143");
                                tw.WriteLine("BAR 557,220, 8, 143");
                                tw.WriteLine("BAR 545,220, 8, 143");
                                tw.WriteLine("BAR 525,220, 4, 143");
                                tw.WriteLine("BAR 517,220, 4, 143");
                                tw.WriteLine("BAR 505,220, 4, 143");
                                tw.WriteLine("BAR 493,220, 8, 143");
                                tw.WriteLine("BAR 473,220, 12, 143");
                                tw.WriteLine("BAR 461,220, 4, 143");
                                tw.WriteLine("BAR 445,220, 4, 143");
                                tw.WriteLine("BAR 433,220, 8, 143");
                                tw.WriteLine("BAR 409,220, 12, 143");
                                tw.WriteLine("BAR 393,220, 4, 143");
                                tw.WriteLine("BAR 381,220, 8, 143");
                                tw.WriteLine("BAR 369,220, 8, 143");
                                tw.WriteLine("BAR 349,220, 4, 143");
                                tw.WriteLine("BAR 341,220, 4, 143");
                                tw.WriteLine("BAR 325,220, 8, 143");
                                tw.WriteLine("BAR 305,220, 16, 143");
                                tw.WriteLine("BAR 293,220, 8, 143");
                                tw.WriteLine("BAR 285,220, 4, 143");
                                tw.WriteLine("BAR 277,220, 4, 143");
                                tw.WriteLine("BAR 257,220, 16, 143");
                                tw.WriteLine("BAR 241,220, 4, 143");
                                tw.WriteLine("BAR 233,220, 4, 143");
                                tw.WriteLine("BAR 213,220, 16, 143");
                                tw.WriteLine("BAR 193,220, 8, 143");
                                tw.WriteLine("BAR 177,220, 12, 143");
                                tw.WriteLine("BAR 161,220, 4, 143");
                                tw.WriteLine("BAR 149,220, 8, 143");
                                tw.WriteLine("BAR 125,220, 12, 143");
                                tw.WriteLine("BAR 117,220, 4, 143");
                                tw.WriteLine("BAR 105,220, 8, 143");
                                tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\""+printMat.linkageno+"\"");
                                tw.WriteLine("BAR 21,2660, 2261, 8");
                                tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                                tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                                tw.WriteLine("BAR 33,2500, 2255, 8");
                                tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                                tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                                tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if(printMat.customerpono !=null || printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            else
                            {
                                tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\" - \"");
                            }
                               
                                tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                                if(printMat.storagelocation!=null)
                                {
                                    tw.WriteLine("BARCODE 596,1483,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                                }
                                tw.WriteLine("TEXT 564,1373,\"0\",180,8,8,\""+printMat.storagelocation+"\"");
                                tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\""+ currentdate + "\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else if (printMat.codetype == "N")
                            {
                            tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                            tw.WriteLine("GAP 2 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 16,1926,2285,4060,8");
                            tw.WriteLine("BAR 21,3906, 2261, 8");
                            tw.WriteLine("BAR 27,3667, 2261, 8");
                            tw.WriteLine("BAR 21,3386, 2261, 8");
                            tw.WriteLine("BAR 21,3146, 2261, 8");
                            tw.WriteLine("BAR 21,2989, 2261, 8");
                            tw.WriteLine("BAR 26,2833, 2261, 8");
                            tw.WriteLine("BAR 27,2295, 2261, 8");
                            tw.WriteLine("BOX 20,185,2281,1774,8");
                            tw.WriteLine("BAR 24,1581, 2253, 8");
                            tw.WriteLine("BAR 26,1414, 2253, 8");
                            tw.WriteLine("BAR 24,1177, 2253, 8");
                            tw.WriteLine("BAR 26,1045, 2253, 8");
                            tw.WriteLine("BAR 26,883, 2253, 8");
                            tw.WriteLine("BAR 26,735, 2253, 8");
                            tw.WriteLine("BAR 26,551, 2253, 8");
                            tw.WriteLine("BAR 26,404, 2253, 8");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                            tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"" + printMat.pono + " - " + printMat.itemno + "\"");
                            tw.WriteLine("BAR 1739,1930, 8, 2127");
                            tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                            tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                            tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                            tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                            tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                            tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                            tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                            tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                            tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" " + printMat.serialno + " \"");
                            if(printMat.materialid!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                            }
                            
                            tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\"" + printMat.mscode + " \"");
                            tw.WriteLine("TEXT 1698,3492,\"0\",180,8,8,\"" + printMat.materialdescription + "\"");
                            tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105" + printMat.saleorderno + "!100 -  !099" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\"" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\"" + printMat.noofpieces + "/" + printMat.receivedqty + " ST " + printMat.boxno + " OF " + printMat.totalboxes + " BOXES\"");
                            tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\" Y103 \"");
                            tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                            tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\"" + printMat.customercode + " " + printMat.customername + "\"");
                            //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                            tw.WriteLine("TEXT 1785,1880,\"0\",180,10,9,\"Additional Work Instruction\"");
                            tw.WriteLine("TEXT 2255,1689,\"0\",180,8,8,\"Plant\"");
                            tw.WriteLine("BAR 2091,1579, 8, 192");
                            tw.WriteLine("BAR 2091,554, 8, 1029");
                            tw.WriteLine("BAR 667,885, 8, 698");
                            tw.WriteLine("BAR 1502,200, 8, 354");
                            tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                            tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : " + printMat.ygsgr + "\"");
                            tw.WriteLine("TEXT 1977,1703,\"0\",180,8,8,\"" + printMat.plant + "\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1740,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                            tw.WriteLine("TEXT 2235,1521,\"0\",180,8,8,\"#\"");
                            tw.WriteLine("TEXT 1785,1524,\"0\",180,8,8,\"Carry -in -place\"");
                            tw.WriteLine("TEXT 438,1524,\"0\",180,8,8,\"S_Loc\"");
                            tw.WriteLine("TEXT 2211,1152,\"0\",180,8,8,\"1\"");
                            tw.WriteLine("TEXT 2205,999,\"0\",180,8,8,\"2\"");
                            tw.WriteLine("TEXT 2195,851,\"0\",180,8,8,\"3\"");
                            tw.WriteLine("TEXT 2225,693,\"0\",180,8,8,\"SP\"");
                            tw.WriteLine("TEXT 2016,693,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2228,1338,\"0\",180,8,8,\"GR\"");
                            tw.WriteLine("TEXT 2226,507,\"0\",180,8,8,\"Loading Date\"");
                            tw.WriteLine("TEXT 1421,519,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                            tw.WriteLine("BAR 721,220, 8, 143");
                            tw.WriteLine("BAR 713,220, 4, 143");
                            tw.WriteLine("BAR 693,220, 12, 143");
                            tw.WriteLine("BAR 681,220, 4, 143");
                            tw.WriteLine("BAR 669,220, 8, 143");
                            tw.WriteLine("BAR 649,220, 12, 143");
                            tw.WriteLine("BAR 637,220, 4, 143");
                            tw.WriteLine("BAR 621,220, 4, 143");
                            tw.WriteLine("BAR 609,220, 8, 143");
                            tw.WriteLine("BAR 585,220, 12, 143");
                            tw.WriteLine("BAR 569,220, 4, 143");
                            tw.WriteLine("BAR 557,220, 8, 143");
                            tw.WriteLine("BAR 545,220, 8, 143");
                            tw.WriteLine("BAR 525,220, 4, 143");
                            tw.WriteLine("BAR 517,220, 4, 143");
                            tw.WriteLine("BAR 505,220, 4, 143");
                            tw.WriteLine("BAR 493,220, 8, 143");
                            tw.WriteLine("BAR 473,220, 12, 143");
                            tw.WriteLine("BAR 461,220, 4, 143");
                            tw.WriteLine("BAR 445,220, 4, 143");
                            tw.WriteLine("BAR 433,220, 8, 143");
                            tw.WriteLine("BAR 409,220, 12, 143");
                            tw.WriteLine("BAR 393,220, 4, 143");
                            tw.WriteLine("BAR 381,220, 8, 143");
                            tw.WriteLine("BAR 369,220, 8, 143");
                            tw.WriteLine("BAR 349,220, 4, 143");
                            tw.WriteLine("BAR 341,220, 4, 143");
                            tw.WriteLine("BAR 325,220, 8, 143");
                            tw.WriteLine("BAR 305,220, 16, 143");
                            tw.WriteLine("BAR 293,220, 8, 143");
                            tw.WriteLine("BAR 285,220, 4, 143");
                            tw.WriteLine("BAR 277,220, 4, 143");
                            tw.WriteLine("BAR 257,220, 16, 143");
                            tw.WriteLine("BAR 241,220, 4, 143");
                            tw.WriteLine("BAR 233,220, 4, 143");
                            tw.WriteLine("BAR 213,220, 16, 143");
                            tw.WriteLine("BAR 193,220, 8, 143");
                            tw.WriteLine("BAR 177,220, 12, 143");
                            tw.WriteLine("BAR 161,220, 4, 143");
                            tw.WriteLine("BAR 149,220, 8, 143");
                            tw.WriteLine("BAR 125,220, 12, 143");
                            tw.WriteLine("BAR 117,220, 4, 143");
                            tw.WriteLine("BAR 105,220, 8, 143");
                            tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\"" + printMat.linkageno + "\"");
                            tw.WriteLine("BAR 21,2660, 2261, 8");
                            tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            tw.WriteLine("BAR 33,2500, 2255, 8");
                            tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                            tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                            tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if(printMat.customerpono!=null && printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 2007,1338,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            
                            //tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            if (printMat.storagelocation!=null)
                            {
                                tw.WriteLine("BARCODE 596,1392,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                            }
                            
                            tw.WriteLine("TEXT 564,1282,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\"" + currentdate + "\"");
                            tw.WriteLine("BAR 27,2154, 2255, 8");
                            tw.WriteLine("TEXT 2235,2130,\"0\",180,8,8,\"Project ID &\"");
                            tw.WriteLine("TEXT 2235,2051,\"0\",180,8,8,\"Definition\"");
                            tw.WriteLine("TEXT 1655,2088,\"0\",180,8,8,\"" + printMat.projectiddef + "\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }
                        else if (printMat.codetype == "A")
                            {
                            tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                            tw.WriteLine("GAP 2 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 16,1926,2285,4060,8");
                            tw.WriteLine("BAR 21,3906, 2261, 8");
                            tw.WriteLine("BAR 27,3667, 2261, 8");
                            tw.WriteLine("BAR 21,3386, 2261, 8");
                            tw.WriteLine("BAR 21,3146, 2261, 8");
                            tw.WriteLine("BAR 21,2989, 2261, 8");
                            tw.WriteLine("BAR 26,2833, 2261, 8");
                            tw.WriteLine("BAR 27,2295, 2261, 8");
                            tw.WriteLine("BOX 20,185,2281,1774,8");
                            tw.WriteLine("BAR 24,1581, 2253, 8");
                            tw.WriteLine("BAR 26,1414, 2253, 8");
                            tw.WriteLine("BAR 24,1177, 2253, 8");
                            tw.WriteLine("BAR 26,1045, 2253, 8");
                            tw.WriteLine("BAR 26,883, 2253, 8");
                            tw.WriteLine("BAR 26,735, 2253, 8");
                            tw.WriteLine("BAR 26,551, 2253, 8");
                            tw.WriteLine("BAR 26,404, 2253, 8");
                            //tw.WriteLine("BAR 26,404, 2253, 8");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                            tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"" + printMat.pono + " - " + printMat.itemno + "\"");
                            tw.WriteLine("BAR 1739,1930, 8, 2127");
                            tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                            tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                            tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                            tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                            tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                            tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                            tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                            tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                            tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" " + printMat.serialno + " \"");
                            if(printMat.materialid!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                            }
                           
                            tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\"" + printMat.mscode + " \"");
                            tw.WriteLine("TEXT 1697,3492,\"0\",180,8,8,\"" + printMat.materialdescription + "\"");
                            tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\"" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\"" + printMat.noofpieces + "/" + printMat.receivedqty + " ST " + printMat.boxno + " OF " + printMat.totalboxes + " BOXES\"");
                            tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\" Y103 \"");
                            tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                            tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\"" + printMat.customercode + " " + printMat.customername + "\"");
                            //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                            tw.WriteLine("TEXT 1785,1880,\"0\",180,10,9,\"Additional Work Instruction\"");
                            tw.WriteLine("TEXT 2255,1689,\"0\",180,8,8,\"Plant\"");
                            tw.WriteLine("BAR 2091,1579, 8, 192");
                            tw.WriteLine("BAR 2091,554, 8, 1029");
                            tw.WriteLine("BAR 667,885, 8, 698");
                            tw.WriteLine("BAR 1502,200, 8, 354");
                            tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                            tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : " + printMat.ygsgr + "\"");
                            tw.WriteLine("TEXT 1977,1703,\"0\",180,8,8,\"" + printMat.plant + "\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1740,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                           
                            tw.WriteLine("TEXT 2235,1521,\"0\",180,8,8,\"#\"");
                            tw.WriteLine("TEXT 1785,1524,\"0\",180,8,8,\"Carry -in -place\"");
                            tw.WriteLine("TEXT 438,1524,\"0\",180,8,8,\"S_Loc\"");
                            tw.WriteLine("TEXT 2211,1152,\"0\",180,8,8,\"1\"");
                            tw.WriteLine("TEXT 2205,999,\"0\",180,8,8,\"2\"");
                            tw.WriteLine("TEXT 2195,851,\"0\",180,8,8,\"3\"");
                            tw.WriteLine("TEXT 2225,693,\"0\",180,8,8,\"SP\"");
                            tw.WriteLine("TEXT 2016,693,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2228,1338,\"0\",180,8,8,\"GR\"");
                            tw.WriteLine("TEXT 2226,507,\"0\",180,8,8,\"Loading Date\"");
                            tw.WriteLine("TEXT 1421,519,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                            tw.WriteLine("BAR 721,220, 8, 143");
                            tw.WriteLine("BAR 713,220, 4, 143");
                            tw.WriteLine("BAR 693,220, 12, 143");
                            tw.WriteLine("BAR 681,220, 4, 143");
                            tw.WriteLine("BAR 669,220, 8, 143");
                            tw.WriteLine("BAR 649,220, 12, 143");
                            tw.WriteLine("BAR 637,220, 4, 143");
                            tw.WriteLine("BAR 621,220, 4, 143");
                            tw.WriteLine("BAR 609,220, 8, 143");
                            tw.WriteLine("BAR 585,220, 12, 143");
                            tw.WriteLine("BAR 569,220, 4, 143");
                            tw.WriteLine("BAR 557,220, 8, 143");
                            tw.WriteLine("BAR 545,220, 8, 143");
                            tw.WriteLine("BAR 525,220, 4, 143");
                            tw.WriteLine("BAR 517,220, 4, 143");
                            tw.WriteLine("BAR 505,220, 4, 143");
                            tw.WriteLine("BAR 493,220, 8, 143");
                            tw.WriteLine("BAR 473,220, 12, 143");
                            tw.WriteLine("BAR 461,220, 4, 143");
                            tw.WriteLine("BAR 445,220, 4, 143");
                            tw.WriteLine("BAR 433,220, 8, 143");
                            tw.WriteLine("BAR 409,220, 12, 143");
                            tw.WriteLine("BAR 393,220, 4, 143");
                            tw.WriteLine("BAR 381,220, 8, 143");
                            tw.WriteLine("BAR 369,220, 8, 143");
                            tw.WriteLine("BAR 349,220, 4, 143");
                            tw.WriteLine("BAR 341,220, 4, 143");
                            tw.WriteLine("BAR 325,220, 8, 143");
                            tw.WriteLine("BAR 305,220, 16, 143");
                            tw.WriteLine("BAR 293,220, 8, 143");
                            tw.WriteLine("BAR 285,220, 4, 143");
                            tw.WriteLine("BAR 277,220, 4, 143");
                            tw.WriteLine("BAR 257,220, 16, 143");
                            tw.WriteLine("BAR 241,220, 4, 143");
                            tw.WriteLine("BAR 233,220, 4, 143");
                            tw.WriteLine("BAR 213,220, 16, 143");
                            tw.WriteLine("BAR 193,220, 8, 143");
                            tw.WriteLine("BAR 177,220, 12, 143");
                            tw.WriteLine("BAR 161,220, 4, 143");
                            tw.WriteLine("BAR 149,220, 8, 143");
                            tw.WriteLine("BAR 125,220, 12, 143");
                            tw.WriteLine("BAR 117,220, 4, 143");
                            tw.WriteLine("BAR 105,220, 8, 143");
                            tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\"" + printMat.linkageno + "\"");
                            tw.WriteLine("BAR 21,2660, 2261, 8");
                            tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            tw.WriteLine("BAR 33,2500, 2255, 8");
                            tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                            tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                            tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if (printMat.customerpono!=null || printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 2007,1338,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            
                            //tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            if(printMat.storagelocation!=null)
                            {
                                tw.WriteLine("BARCODE 596,1392,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                            }
                            
                            tw.WriteLine("TEXT 564,1282,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\"" + currentdate + "\"");
                            tw.WriteLine("BAR 27,2149, 2255, 8");
                            tw.WriteLine("TEXT 2235,2117,\"0\",180,8,8,\"Asset &\"");
                            tw.WriteLine("TEXT 2235,2038,\"0\",180,8,8,\"SubNumber\"");
                            if (printMat.assetno!=null || printMat.assetsubno!=null)
                            {
                                tw.WriteLine("TEXT 1701,2077,\"0\",180,8,8,\"" + printMat.assetno + " & " + printMat.assetsubno + "\"");
                            }
                            
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");
                        }
                        else if (printMat.codetype == "F")
                            {
                            tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                            tw.WriteLine("GAP 2 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 22,2121,2291,4060,8");
                            tw.WriteLine("BAR 21,3906, 2261, 8");
                            tw.WriteLine("BAR 27,3667, 2261, 8");
                            tw.WriteLine("BAR 21,3386, 2261, 8");
                            tw.WriteLine("BAR 21,3146, 2261, 8");
                            tw.WriteLine("BAR 21,2989, 2261, 8");
                            tw.WriteLine("BAR 26,2833, 2261, 8");
                            tw.WriteLine("BAR 27,2295, 2261, 8");
                            tw.WriteLine("BOX 22,196,2282,1903,8");
                            tw.WriteLine("BAR 26,1696, 2253, 8");
                            tw.WriteLine("BAR 26,1506, 2253, 8");
                            tw.WriteLine("BAR 26,1296, 2253, 8");
                            tw.WriteLine("BAR 26,1134, 2253, 8");
                            tw.WriteLine("BAR 26,969, 2253, 8");
                            tw.WriteLine("BAR 26,783, 2253, 8");
                            tw.WriteLine("BAR 26,587, 2253, 8");
                            tw.WriteLine("BAR 26,411, 2253, 8");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                            tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"" + printMat.pono + " - " + printMat.itemno + "\"");
                            tw.WriteLine("BAR 1739,2125, 8, 1932");
                            tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                            tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                            tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                            tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                            tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                            tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                            tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                            tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                            tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" " + printMat.serialno + " \"");
                            if(printMat.materialid!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                            }
                            
                            tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\"" + printMat.mscode + " \"");
                            tw.WriteLine("TEXT 1697,3492,\"0\",180,8,8,\"" + printMat.materialdescription + "\"");
                            tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\"" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\"" + printMat.noofpieces + "/" + printMat.receivedqty + " ST " + printMat.boxno + " OF " + printMat.totalboxes + " BOXES\"");
                            tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                            tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                            tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\"" + printMat.customercode + " " + printMat.customername + "\"");
                            //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                            tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                            tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                            tw.WriteLine("BAR 2015,1698, 8, 202");
                            tw.WriteLine("BAR 2091,590, 8, 1109");
                            tw.WriteLine("BAR 667,971, 8, 728");
                            tw.WriteLine("BAR 1502,200, 8, 390");
                            tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                            tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : " + printMat.ygsgr + "\"");
                            tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                            tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                            tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                            tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                            tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                            tw.WriteLine("TEXT 2205,1086,\"0\",180,8,8,\"2\"");
                            tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                            tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                            tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\"" + printMat.shippingpoint + " \"");
                            tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                            tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                            tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                            tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                            tw.WriteLine("BAR 721,220, 8, 143");
                            tw.WriteLine("BAR 713,220, 4, 143");
                            tw.WriteLine("BAR 693,220, 12, 143");
                            tw.WriteLine("BAR 681,220, 4, 143");
                            tw.WriteLine("BAR 669,220, 8, 143");
                            tw.WriteLine("BAR 649,220, 12, 143");
                            tw.WriteLine("BAR 637,220, 4, 143");
                            tw.WriteLine("BAR 621,220, 4, 143");
                            tw.WriteLine("BAR 609,220, 8, 143");
                            tw.WriteLine("BAR 585,220, 12, 143");
                            tw.WriteLine("BAR 569,220, 4, 143");
                            tw.WriteLine("BAR 557,220, 8, 143");
                            tw.WriteLine("BAR 545,220, 8, 143");
                            tw.WriteLine("BAR 525,220, 4, 143");
                            tw.WriteLine("BAR 517,220, 4, 143");
                            tw.WriteLine("BAR 505,220, 4, 143");
                            tw.WriteLine("BAR 493,220, 8, 143");
                            tw.WriteLine("BAR 473,220, 12, 143");
                            tw.WriteLine("BAR 461,220, 4, 143");
                            tw.WriteLine("BAR 445,220, 4, 143");
                            tw.WriteLine("BAR 433,220, 8, 143");
                            tw.WriteLine("BAR 409,220, 12, 143");
                            tw.WriteLine("BAR 393,220, 4, 143");
                            tw.WriteLine("BAR 381,220, 8, 143");
                            tw.WriteLine("BAR 369,220, 8, 143");
                            tw.WriteLine("BAR 349,220, 4, 143");
                            tw.WriteLine("BAR 341,220, 4, 143");
                            tw.WriteLine("BAR 325,220, 8, 143");
                            tw.WriteLine("BAR 305,220, 16, 143");
                            tw.WriteLine("BAR 293,220, 8, 143");
                            tw.WriteLine("BAR 285,220, 4, 143");
                            tw.WriteLine("BAR 277,220, 4, 143");
                            tw.WriteLine("BAR 257,220, 16, 143");
                            tw.WriteLine("BAR 241,220, 4, 143");
                            tw.WriteLine("BAR 233,220, 4, 143");
                            tw.WriteLine("BAR 213,220, 16, 143");
                            tw.WriteLine("BAR 193,220, 8, 143");
                            tw.WriteLine("BAR 177,220, 12, 143");
                            tw.WriteLine("BAR 161,220, 4, 143");
                            tw.WriteLine("BAR 149,220, 8, 143");
                            tw.WriteLine("BAR 125,220, 12, 143");
                            tw.WriteLine("BAR 117,220, 4, 143");
                            tw.WriteLine("BAR 105,220, 8, 143");
                            tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\"" + printMat.linkageno + "\"");
                            tw.WriteLine("BAR 21,2660, 2261, 8");
                            tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            tw.WriteLine("BAR 33,2500, 2255, 8");
                            tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                            tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if(printMat.customerpono!=null && printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            if (printMat.storagelocation!=null)
                            {
                                tw.WriteLine("BARCODE 596,1483,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                            }
                            
                            tw.WriteLine("TEXT 564,1373,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\"" + currentdate + "\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }
                        else if (printMat.codetype == "Y")
                            {
                            tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                            tw.WriteLine("GAP 2 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 16,1926,2285,4060,8");
                            tw.WriteLine("BAR 21,3906, 2261, 8");
                            tw.WriteLine("BAR 27,3667, 2261, 8");
                            tw.WriteLine("BAR 21,3386, 2261, 8");
                            tw.WriteLine("BAR 21,3146, 2261, 8");
                            tw.WriteLine("BAR 21,2989, 2261, 8");
                            tw.WriteLine("BAR 26,2833, 2261, 8");
                            tw.WriteLine("BAR 27,2295, 2261, 8");
                            tw.WriteLine("BOX 20,185,2281,1774,8");
                            tw.WriteLine("BAR 24,1581, 2253, 8");
                            tw.WriteLine("BAR 26,1414, 2253, 8");
                            tw.WriteLine("BAR 24,1177, 2253, 8");
                            tw.WriteLine("BAR 26,1045, 2253, 8");
                            tw.WriteLine("BAR 26,883, 2253, 8");
                            tw.WriteLine("BAR 26,735, 2253, 8");
                            tw.WriteLine("BAR 26,551, 2253, 8");
                            tw.WriteLine("BAR 26,404, 2253, 8");
                            //tw.WriteLine("BAR 26,404, 2253, 8");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                            tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"" + printMat.pono + " - " + printMat.itemno + "\"");
                            tw.WriteLine("BAR 1739,1930, 8, 2127");
                            tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                            tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                            tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                            tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                            tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                            tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                            tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                            tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                            tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" " + printMat.serialno + " \"");
                            if(printMat.materialid!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                            }
                            
                            tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\"" + printMat.mscode + " \"");
                            tw.WriteLine("TEXT 1697,3492,\"0\",180,8,8,\"" + printMat.materialdescription + "\"");
                            if(printMat.saleorderno!=null && printMat.solineitemno!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            }
                            
                            tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\"" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\"" + printMat.noofpieces + "/" + printMat.receivedqty + " ST " + printMat.boxno + " OF " + printMat.totalboxes + " BOXES\"");
                            tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\" Y103 \"");
                            tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                            tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\"" + printMat.customercode + " " + printMat.customername + "\"");
                            //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                            tw.WriteLine("TEXT 1785,1880,\"0\",180,10,9,\"Additional Work Instruction\"");
                            tw.WriteLine("TEXT 2255,1689,\"0\",180,8,8,\"Plant\"");
                            tw.WriteLine("BAR 2091,1579, 8, 192");
                            tw.WriteLine("BAR 2091,554, 8, 1029");
                            tw.WriteLine("BAR 667,885, 8, 698");
                            tw.WriteLine("BAR 1502,200, 8, 354");
                            tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                            tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : " + printMat.ygsgr + "\"");
                            tw.WriteLine("TEXT 1977,1703,\"0\",180,8,8,\"" + printMat.plant + "\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1740,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                            
                            tw.WriteLine("TEXT 2235,1521,\"0\",180,8,8,\"#\"");
                            tw.WriteLine("TEXT 1785,1524,\"0\",180,8,8,\"Carry -in -place\"");
                            tw.WriteLine("TEXT 438,1524,\"0\",180,8,8,\"S_Loc\"");
                            tw.WriteLine("TEXT 2211,1152,\"0\",180,8,8,\"1\"");
                            tw.WriteLine("TEXT 2205,999,\"0\",180,8,8,\"2\"");
                            tw.WriteLine("TEXT 2195,851,\"0\",180,8,8,\"3\"");
                            tw.WriteLine("TEXT 2225,693,\"0\",180,8,8,\"SP\"");
                            tw.WriteLine("TEXT 2016,693,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2228,1338,\"0\",180,8,8,\"GR\"");
                            tw.WriteLine("TEXT 2226,507,\"0\",180,8,8,\"Loading Date\"");
                            tw.WriteLine("TEXT 1421,519,\"0\",180,8,8,\" - \"");
                            tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                            tw.WriteLine("BAR 721,220, 8, 143");
                            tw.WriteLine("BAR 713,220, 4, 143");
                            tw.WriteLine("BAR 693,220, 12, 143");
                            tw.WriteLine("BAR 681,220, 4, 143");
                            tw.WriteLine("BAR 669,220, 8, 143");
                            tw.WriteLine("BAR 649,220, 12, 143");
                            tw.WriteLine("BAR 637,220, 4, 143");
                            tw.WriteLine("BAR 621,220, 4, 143");
                            tw.WriteLine("BAR 609,220, 8, 143");
                            tw.WriteLine("BAR 585,220, 12, 143");
                            tw.WriteLine("BAR 569,220, 4, 143");
                            tw.WriteLine("BAR 557,220, 8, 143");
                            tw.WriteLine("BAR 545,220, 8, 143");
                            tw.WriteLine("BAR 525,220, 4, 143");
                            tw.WriteLine("BAR 517,220, 4, 143");
                            tw.WriteLine("BAR 505,220, 4, 143");
                            tw.WriteLine("BAR 493,220, 8, 143");
                            tw.WriteLine("BAR 473,220, 12, 143");
                            tw.WriteLine("BAR 461,220, 4, 143");
                            tw.WriteLine("BAR 445,220, 4, 143");
                            tw.WriteLine("BAR 433,220, 8, 143");
                            tw.WriteLine("BAR 409,220, 12, 143");
                            tw.WriteLine("BAR 393,220, 4, 143");
                            tw.WriteLine("BAR 381,220, 8, 143");
                            tw.WriteLine("BAR 369,220, 8, 143");
                            tw.WriteLine("BAR 349,220, 4, 143");
                            tw.WriteLine("BAR 341,220, 4, 143");
                            tw.WriteLine("BAR 325,220, 8, 143");
                            tw.WriteLine("BAR 305,220, 16, 143");
                            tw.WriteLine("BAR 293,220, 8, 143");
                            tw.WriteLine("BAR 285,220, 4, 143");
                            tw.WriteLine("BAR 277,220, 4, 143");
                            tw.WriteLine("BAR 257,220, 16, 143");
                            tw.WriteLine("BAR 241,220, 4, 143");
                            tw.WriteLine("BAR 233,220, 4, 143");
                            tw.WriteLine("BAR 213,220, 16, 143");
                            tw.WriteLine("BAR 193,220, 8, 143");
                            tw.WriteLine("BAR 177,220, 12, 143");
                            tw.WriteLine("BAR 161,220, 4, 143");
                            tw.WriteLine("BAR 149,220, 8, 143");
                            tw.WriteLine("BAR 125,220, 12, 143");
                            tw.WriteLine("BAR 117,220, 4, 143");
                            tw.WriteLine("BAR 105,220, 8, 143");
                            tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\"" + printMat.linkageno + "\"");
                            tw.WriteLine("BAR 21,2660, 2261, 8");
                            tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            tw.WriteLine("BAR 33,2500, 2255, 8");
                            tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                            tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                            tw.WriteLine("TEXT 1682,2440,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if(printMat.customerpono!=null || printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 2007,1338,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            
                            //tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            if(printMat.storagelocation!=null)
                            {
                                tw.WriteLine("BARCODE 596,1392,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                            }
                            
                            tw.WriteLine("TEXT 564,1282,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\"" + currentdate + "\"");
                            tw.WriteLine("BAR 27,2158, 2255, 8");
                            tw.WriteLine("TEXT 2255,2127,\"0\",180,8,8,\"Cost centre &\"");
                            tw.WriteLine("TEXT 2255,2048,\"0\",180,8,8,\"Cost centre Text\"");
                            if(printMat.costcenter!=null || printMat.costcentertext!=null)
                            {
                                tw.WriteLine("TEXT 1697,2088,\"0\",180,9,8,\"" + printMat.costcenter + " & " + printMat.costcentertext + "\"");
                            }
                           
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");
                        }
                        else
                            {
                            tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                            tw.WriteLine("GAP 2 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 22,2121,2291,4060,8");
                            tw.WriteLine("BAR 21,3906, 2261, 8");
                            tw.WriteLine("BAR 27,3667, 2261, 8");
                            tw.WriteLine("BAR 21,3386, 2261, 8");
                            tw.WriteLine("BAR 21,3146, 2261, 8");
                            tw.WriteLine("BAR 21,2989, 2261, 8");
                            tw.WriteLine("BAR 26,2833, 2261, 8");
                            tw.WriteLine("BAR 27,2295, 2261, 8");
                            tw.WriteLine("BOX 22,196,2282,1903,8");
                            tw.WriteLine("BAR 26,1696, 2253, 8");
                            tw.WriteLine("BAR 26,1506, 2253, 8");
                            tw.WriteLine("BAR 26,1296, 2253, 8");
                            tw.WriteLine("BAR 26,1134, 2253, 8");
                            tw.WriteLine("BAR 26,969, 2253, 8");
                            tw.WriteLine("BAR 26,783, 2253, 8");
                            tw.WriteLine("BAR 26,587, 2253, 8");
                            tw.WriteLine("BAR 26,411, 2253, 8");
                            tw.WriteLine("CODEPAGE 1252");
                            tw.WriteLine("TEXT 2274,4155,\"0\",180,8,8,\"Ref # - \"");
                            tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"" + printMat.pono + " - " + printMat.itemno + "\"");
                            tw.WriteLine("BAR 1739,2125, 8, 1932");
                            tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No.\"");
                            tw.WriteLine("TEXT 2235,3836,\"0\",180,7,8,\"Material\"");
                            tw.WriteLine("TEXT 2235,3576,\"0\",180,8,8,\"MS Code\"");
                            tw.WriteLine("TEXT 2235,3306,\"0\",180,9,8,\"Order\"");
                            tw.WriteLine("TEXT 2235,3098,\"0\",180,8,8,\"Qty\"");
                            tw.WriteLine("TEXT 2235,2940,\"0\",180,8,8,\"S/O Type\"");
                            tw.WriteLine("TEXT 2235,2777,\"0\",180,8,8,\"Insp Rec.\"");
                            tw.WriteLine("TEXT 2235,2256,\"0\",180,8,8,\"Ship-to:\"");
                            tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" " + printMat.serialno + " \"");
                            if(printMat.materialid!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3822,\"128M\",117,0,180,6,12,\"!104" + printMat.materialid + "\"");
                            }
                            
                            tw.WriteLine("TEXT 1592,3894,\"0\",180,8,8,\"" + printMat.materialid + "\"");
                            tw.WriteLine("TEXT 1697,3614,\"0\",180,8,8,\"" + printMat.mscode + " \"");
                            tw.WriteLine("TEXT 1697,3492,\"0\",180,8,8,\"" + printMat.materialdescription + "\"");
                            if(printMat.saleorderno!=null || printMat.solineitemno!=null)
                            {
                                tw.WriteLine("BARCODE 1697,3260,\"128M\",71,0,180,6,12,\"!105" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            }
                           
                            tw.WriteLine("TEXT 1592,3331,\"0\",180,8,8,\"" + printMat.saleorderno + "-" + printMat.solineitemno + " \"");
                            tw.WriteLine("TEXT 1697,3095,\"0\",180,8,8,\"" + printMat.noofpieces + "/" + printMat.receivedqty + " ST " + printMat.boxno + " OF " + printMat.totalboxes + " BOXES\"");
                            tw.WriteLine("TEXT 1697,2943,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                            tw.WriteLine("TEXT 1697,2777,\"0\",180,8,8,\"Not Required\"");
                            tw.WriteLine("TEXT 1697,2256,\"0\",180,8,8,\"" + printMat.customercode + " " + printMat.customername + "\"");
                            //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                            tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                            tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                            tw.WriteLine("BAR 2015,1698, 8, 202");
                            tw.WriteLine("BAR 2091,590, 8, 1109");
                            tw.WriteLine("BAR 667,971, 8, 728");
                            tw.WriteLine("BAR 1502,200, 8, 390");
                            tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                            tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : " + printMat.ygsgr + "\"");
                            tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                            if(printMat.plant!=null)
                            {
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105" + printMat.plant + "!1005\"");
                            }
                            
                            tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                            tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                            tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                            tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                            tw.WriteLine("TEXT 2205,1086,\"0\",180,8,8,\"2\"");
                            tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                            tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                            tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\"" + printMat.shippingpoint + " \"");
                            tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                            tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                            tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                            tw.WriteLine("TEXT 2205,329,\"0\",180,8,8,\"Linkage No.\"");
                            tw.WriteLine("BAR 721,220, 8, 143");
                            tw.WriteLine("BAR 713,220, 4, 143");
                            tw.WriteLine("BAR 693,220, 12, 143");
                            tw.WriteLine("BAR 681,220, 4, 143");
                            tw.WriteLine("BAR 669,220, 8, 143");
                            tw.WriteLine("BAR 649,220, 12, 143");
                            tw.WriteLine("BAR 637,220, 4, 143");
                            tw.WriteLine("BAR 621,220, 4, 143");
                            tw.WriteLine("BAR 609,220, 8, 143");
                            tw.WriteLine("BAR 585,220, 12, 143");
                            tw.WriteLine("BAR 569,220, 4, 143");
                            tw.WriteLine("BAR 557,220, 8, 143");
                            tw.WriteLine("BAR 545,220, 8, 143");
                            tw.WriteLine("BAR 525,220, 4, 143");
                            tw.WriteLine("BAR 517,220, 4, 143");
                            tw.WriteLine("BAR 505,220, 4, 143");
                            tw.WriteLine("BAR 493,220, 8, 143");
                            tw.WriteLine("BAR 473,220, 12, 143");
                            tw.WriteLine("BAR 461,220, 4, 143");
                            tw.WriteLine("BAR 445,220, 4, 143");
                            tw.WriteLine("BAR 433,220, 8, 143");
                            tw.WriteLine("BAR 409,220, 12, 143");
                            tw.WriteLine("BAR 393,220, 4, 143");
                            tw.WriteLine("BAR 381,220, 8, 143");
                            tw.WriteLine("BAR 369,220, 8, 143");
                            tw.WriteLine("BAR 349,220, 4, 143");
                            tw.WriteLine("BAR 341,220, 4, 143");
                            tw.WriteLine("BAR 325,220, 8, 143");
                            tw.WriteLine("BAR 305,220, 16, 143");
                            tw.WriteLine("BAR 293,220, 8, 143");
                            tw.WriteLine("BAR 285,220, 4, 143");
                            tw.WriteLine("BAR 277,220, 4, 143");
                            tw.WriteLine("BAR 257,220, 16, 143");
                            tw.WriteLine("BAR 241,220, 4, 143");
                            tw.WriteLine("BAR 233,220, 4, 143");
                            tw.WriteLine("BAR 213,220, 16, 143");
                            tw.WriteLine("BAR 193,220, 8, 143");
                            tw.WriteLine("BAR 177,220, 12, 143");
                            tw.WriteLine("BAR 161,220, 4, 143");
                            tw.WriteLine("BAR 149,220, 8, 143");
                            tw.WriteLine("BAR 125,220, 12, 143");
                            tw.WriteLine("BAR 117,220, 4, 143");
                            tw.WriteLine("BAR 105,220, 8, 143");
                            tw.WriteLine("TEXT 1472,323,\"0\",180,8,8,\"" + printMat.linkageno + "\"");
                            tw.WriteLine("BAR 21,2660, 2261, 8");
                            tw.WriteLine("TEXT 2252,2631,\"0\",180,8,9,\"Customer\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            tw.WriteLine("BAR 33,2500, 2255, 8");
                            tw.WriteLine("TEXT 2273,2471,\"0\",180,8,8,\"Customer PO No\"");
                            tw.WriteLine("TEXT 2273,2392,\"0\",180,8,8,\"& Line Item No\"");
                            tw.WriteLine("TEXT 1681,2614,\"0\",180,8,8,\"" + printMat.customername + "\"");
                            if(printMat.customerpono!=null || printMat.custpolineitem!=null)
                            {
                                tw.WriteLine("TEXT 1681,2440,\"0\",180,8,8,\"" + printMat.customerpono + " & " + printMat.custpolineitem + "\"");
                            }
                            
                            tw.WriteLine("TEXT 2007,1450,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            if(printMat.storagelocation!=null)
                            {
                                tw.WriteLine("BARCODE 596,1483,\"128M\",93,0,180,4,8,\"!105" + printMat.storagelocation + "!1004\"");
                            }
                            
                            tw.WriteLine("TEXT 564,1373,\"0\",180,8,8,\"" + printMat.storagelocation + "\"");
                            tw.WriteLine("TEXT 632,4150,\"0\",180,8,8,\"" + currentdate + "\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }


                    }
                        //else
                        //{
                        //    tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
                        //    tw.WriteLine("GAP 3 mm, 0 mm");
                        //    tw.WriteLine("SET RIBBON ON");
                        //    tw.WriteLine("DIRECTION 0,0");
                        //    tw.WriteLine("REFERENCE 0,0");
                        //    tw.WriteLine("OFFSET 0 mm");
                        //    tw.WriteLine("SET PEEL OFF");
                        //    tw.WriteLine("SET CUTTER OFF");
                        //    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                        //    tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='33.0 mm'></xpml>SET TEAR ON");
                        //    tw.WriteLine("ON");
                        //    tw.WriteLine("CLS");
                        //    tw.WriteLine("BOX 9,15,741,289,3");
                        //    tw.WriteLine("BAR 492,15, 3, 272");
                        //    tw.WriteLine("BAR 172,15, 3, 272");
                        //    tw.WriteLine("BAR 172,217, 568, 3");
                        //    tw.WriteLine("BAR 172,75, 568, 3");
                        //    //tw.WriteLine("BAR 183,86, 557, 3");
                        //    tw.WriteLine("QRCODE 144,192,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
                        //    tw.WriteLine("CODEPAGE 1252");
                        //    tw.WriteLine("TEXT 731,264,\"0\",180,9,9,\"Material Code: \"");
                        //    tw.WriteLine("TEXT 731,188,\"0\",180,8,9,\"Received Date: \"");
                        //    tw.WriteLine("TEXT 704,60,\"0\",180,9,9,\"Quantity\"");
                        //    tw.WriteLine("TEXT 482,259,\"0\",180,14,9,\"" + printMat.materialid + "\"");
                        //    tw.WriteLine("TEXT 486,60,\"0\",180,13,9,\"" + i+ "/" + printMat.noofprint + "\"");
                        //    tw.WriteLine("TEXT 485,189,\"0\",180,13,11,\"" + printMat.receiveddate + "\"");
                        //    tw.WriteLine("BAR 172,143, 568, 3");
                        //    tw.WriteLine("TEXT 731,116,\"ROMAN.TTF\",180,1,8,\"PO No. - Invoice No.\"");
                        //    tw.WriteLine("TEXT 481,116,\"0\",180,11,7,\""+printMat.pono+"-"+printMat.invoiceno+"\"");
                        //    tw.WriteLine("PRINT 1,1");
                        //    tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        //}
                    }


                }
            //}
            
            //}
            try
            {
                //Convert.ToString(ConfigurationManager.AppSettings["PrinterName"].ToString());
                //string printerName = ConfigurationManager.AppSettings["CTMajor_AdminPrinter"].ToString();
                //string printerName = "10.29.11.25";
                string printerName = "10.29.11.126";
                printerName = config._MaterialLabelPrintIP;
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

        [HttpPost("printonholdmaterials")]
        public string printonholdmaterials(printonholdGR onholdprintdata)
        {
            string result = "Error";
            try
            {
                string path = Environment.CurrentDirectory + @"\PRNFiles\";
                if (!Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                string printResult = null;
                path = path + onholdprintdata.gateentryid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
                FileMode fileType = FileMode.OpenOrCreate;
                for (int i = 1; i <= onholdprintdata.noofprint; i++)
                {
                    // if (File.Exists(path))
                    if (Directory.Exists(path))
                {
                    fileType = FileMode.Append;
                }

                    using (FileStream fs = new FileStream(path, fileType))
                    {
                        using (TextWriter tw = new StreamWriter(fs))
                        {

                            //TSC TE310 printer
                            //tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                            ////tw.WriteLine("GAP 3 mm, 0 mm");
                            //tw.WriteLine("DIRECTION 0,0");
                            //tw.WriteLine("SET RIBBON ON");

                            //tw.WriteLine("REFERENCE 0,0");
                            //tw.WriteLine("OFFSET 0 mm");
                            //tw.WriteLine("SET PEEL OFF");
                            //tw.WriteLine("SET CUTTER OFF");
                            //tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            //tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");

                            //tw.WriteLine("CLS");
                            //tw.WriteLine("BOX 15,9,1137,436,4");
                            //tw.WriteLine("BAR 244,11, 4, 424");
                            //tw.WriteLine("BAR 17,226, 1119, 4");
                            //tw.WriteLine("BAR 245,293, 891, 4");
                            //tw.WriteLine("BAR 245,362, 891, 4");
                            //tw.WriteLine("BAR 846,9, 4, 424");
                            //tw.WriteLine("CODEPAGE 1252");
                            ////tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            ////tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");

                            //tw.WriteLine("TEXT 1128,416,\"0\",180,8,8,\"Material Code: \"");
                            //tw.WriteLine("TEXT 1128,346,\"0\",180,8,8,\"Received Date: \"");
                            //tw.WriteLine("TEXT 1128,273,\"0\",180,8,8,\"Gate Entry ID: \"");
                            //tw.WriteLine("TEXT 1128,169,\"0\",180,8,8,\"PO No. - InvoiceNo.\"");
                            //tw.WriteLine("TEXT 1128,68,\"0\",180,8,8,\"Quantity\"");
                            //tw.WriteLine("TEXT 839,416,\"0\",180,8,8,\"" + onholdprintdata.materialid + "\"");
                            //tw.WriteLine("TEXT 839,273,\"0\",180,8,8,\"" + onholdprintdata.gateentryid + "\"");
                            //tw.WriteLine("TEXT 839,207,\"0\",180,8,8,\"" + onholdprintdata.pono +"-"+onholdprintdata.invoiceno+ "\"");
                            //tw.WriteLine("QRCODE 175,410,L,4,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            //tw.WriteLine("TEXT 241,299,\"0\",180,7,7,\"" + onholdprintdata.materialid + "\"");
                            //tw.WriteLine("TEXT 839,346,\"0\",180,8,8,\"" + onholdprintdata.receiveddate + "\"");
                            //tw.WriteLine("TEXT 839,68,\"0\",180,8,8,\"" +i+"/"+ onholdprintdata.noofprint + "\"");
                            //tw.WriteLine("BAR 600,78, 536, 4");
                            //tw.WriteLine("BAR 245,78, 357, 4");
                            //tw.WriteLine("QRCODE 183,216,L,5,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");
                            //tw.WriteLine("TEXT 235,85,\"0\",180,8,8,\"" +onholdprintdata.gateentryid + "\"");
                            //tw.WriteLine("PRINT 1,1");
                            //tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            //TSC TE210 model
                            tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");

                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 18,6,769,264,3");
                            tw.WriteLine("BAR 164,8, 3, 255");
                            tw.WriteLine("BAR 19,141, 749, 3");
                            tw.WriteLine("BAR 165,182, 603, 3");
                            tw.WriteLine("BAR 165,221, 603, 3");
                            tw.WriteLine("BAR 572,6, 3, 257");
                            tw.WriteLine("CODEPAGE 1252");
                            //tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            //tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");

                            tw.WriteLine("TEXT 762,255,\"0\",180,8,8,\"Material Code \"");
                            tw.WriteLine("TEXT 762,219,\"0\",180,8,8,\"Received Date \"");
                            tw.WriteLine("TEXT 762,176,\"0\",180,8,8,\"Gate Entry ID \"");
                            tw.WriteLine("TEXT 762,102,\"0\",180,8,8,\"PO No. - InvoiceNo.\"");
                            tw.WriteLine("TEXT 762,31,\"0\",180,8,8,\"Quantity\"");
                            tw.WriteLine("TEXT 567,255,\"0\",180,8,8,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 567,176,\"0\",180,8,8,\"" + onholdprintdata.gateentryid + "\"");
                            //tw.WriteLine("TEXT 567,128,\"0\",180,8,8,\"" + onholdprintdata.pono + "-" + onholdprintdata.invoiceno + "\"");
                            tw.WriteLine("QRCODE 125,253,L,3,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 159,173,\"0\",180,7,7,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 567,219,\"0\",180,8,8,\"" + onholdprintdata.receiveddate + "\"");
                            tw.WriteLine("TEXT 567,31,\"0\",180,8,8,\"" + i + "/" + onholdprintdata.noofprint + "\"");
                            //tw.WriteLine("BAR 600,78, 536, 4");
                            //tw.WriteLine("BAR 245,78, 357, 4");
                            tw.WriteLine("QRCODE 125,131,L,3,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");
                            tw.WriteLine("TEXT 163,51,\"0\",180,8,8,\"" + onholdprintdata.gateentryid + "\"");
                            tw.WriteLine("BAR 165,39, 603, 3");
                            tw.WriteLine("TEXT 567,109,\"0\",180,6,8,\"" + onholdprintdata.pono + "-" + onholdprintdata.invoiceno + "\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }

                    }
                }
                try
                {
                    //Send dat ato printer
                    string printerName = "10.29.11.127";
                    printerName = config._OnholdmaterialprintIP;
                    PrintUtilities objIdentification = new PrintUtilities();
                   
                    result = objIdentification.PrintQRCode(path, printerName);
                    



                }

                catch (Exception ex)
                {
                    throw ex;
                }

                if (result == "success")
                {
                    //update count wms_reprinthistory table            
                    //this._poService.updateonholdPrintHistory(onholdprintdata);
                    return "success";
                }
                else
                {
                    return "Error Occured";
                }


            }
            catch(Exception Ex)
            {
                log.ErrorMessage("PODataProvider", "printonholdmaterials", Ex.StackTrace.ToString(), Ex.Message.ToString(), url);
                return result;
            }

        }

    }
}
