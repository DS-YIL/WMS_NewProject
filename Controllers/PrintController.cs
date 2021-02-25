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

                    //tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                    //tw.WriteLine("DIRECTION 0,0");
                    //tw.WriteLine("REFERENCE 0,0");
                    //tw.WriteLine("OFFSET 0 mm");
                    //tw.WriteLine("SET PEEL OFF");
                    //tw.WriteLine("SET CUTTER OFF");
                    //tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    //tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");
                    //tw.WriteLine("CLS");
                    //tw.WriteLine("BOX 15,9,1137,436,4");
                    //tw.WriteLine("BAR 17,93, 1119, 4");
                    //tw.WriteLine("BAR 244,93, 4, 342");
                    //tw.WriteLine("BAR 245,161, 891, 4");
                    //tw.WriteLine("BAR 245,226, 891, 4");
                    //tw.WriteLine("BAR 245,362, 891, 4");
                    //tw.WriteLine("BAR 846,9, 4, 424");
                    //tw.WriteLine("CODEPAGE 1252");
                    //tw.WriteLine("TEXT 1128,416,\"0\",180,8,8,\"Gate Entry No.\"");
                    //tw.WriteLine("TEXT 1133,315,\"0\",180,8,8,\"PO No.\"");
                    //tw.WriteLine("TEXT 1128,209,\"0\",180,8,8,\"Gate Entry Time\"");
                    //tw.WriteLine("TEXT 1128,144,\"0\",180,8,8,\"Vehicle No.\"");
                    //tw.WriteLine("TEXT 1128,68,\"0\",180,8,8,\"Transporter Details\"");
                    //tw.WriteLine("TEXT 839,416,\"0\",180,8,8,\"" + model.inwmasterid+"\"");
                    //tw.WriteLine("TEXT 839,213,\"0\",180,8,8,\"" + model.gateentrytime+"\"");
                    //tw.WriteLine("TEXT 839,145,\"0\",180,8,8,\"" + model.vehicleno+"\"");
                    //tw.WriteLine("TEXT 839,87,\"0\",180,8,8,\"" + model.transporterdetails+"\"");
                    ////tw.WriteLine("TEXT 839,48,\"0\",180,8,8,\"" + model.transporterdetails+"\"");
                    //tw.WriteLine("QRCODE 175,353,L,4,A,180,M2,S7,\"" + model.inwmasterid+"\"");
                    //tw.WriteLine("TEXT 235,243,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
                    //tw.WriteLine("TEXT 839,354,\"0\",180,8,8,\"" + model.pono + "\"");
                    ////tw.WriteLine("TEXT 839,354,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                    ////tw.WriteLine("TEXT 1235,243,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
                    //tw.WriteLine("PRINT 1,1");
                    //tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");




                    //Prn code for TSC TE210 printer

                    string formateddate = model.gateentrytime.ToString("dd-MM-yyyy");

                    tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                   // tw.WriteLine("GAP 3 mm, 0 mm");
                    tw.WriteLine("DIRECTION 0,0");
                    tw.WriteLine("REFERENCE 0,0");
                    tw.WriteLine("OFFSET 0 mm");
                    tw.WriteLine("SET PEEL OFF");
                    tw.WriteLine("SET CUTTER OFF");
                    tw.WriteLine("SET PARTIAL_CUTTER OFF");
                    tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");
                    tw.WriteLine("CLS");
                    tw.WriteLine("BOX 15,5,762,260,3");
                    tw.WriteLine("BAR 15,49, 746, 3");
                    tw.WriteLine("BAR 164,49, 3, 209");
                    tw.WriteLine("BAR 165,95, 596, 3");
                    tw.WriteLine("BAR 165,139, 596, 3");
                    tw.WriteLine("BAR 165,208, 596, 3");
                    tw.WriteLine("BAR 572,6, 3, 252");
                    tw.WriteLine("CODEPAGE 1252");
                    tw.WriteLine("TEXT 756,247,\"0\",180,8,8,\"Gate Entry No.\"");
                    tw.WriteLine("TEXT 756,186,\"0\",180,8,8,\"PO No.\"");
                    tw.WriteLine("TEXT 756,128,\"0\",180,8,8,\"Gate Entry Time\"");
                    tw.WriteLine("TEXT 756,84,\"0\",180,8,8,\"Vehicle No.\"");
                    tw.WriteLine("TEXT 756,35,\"0\",180,8,8,\"Transporter Details\"");
                    tw.WriteLine("TEXT 567,248,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 567,130,\"0\",180,8,8,\"" + formateddate + "\"");
                    tw.WriteLine("TEXT 567,84,\"0\",180,8,8,\"" + model.vehicleno + "\"");
                    tw.WriteLine("TEXT 567,35,\"0\",180,8,8,\"" + model.transporterdetails + "\"");
                    //tw.WriteLine("TEXT 839,48,\"0\",180,8,8,\"" + model.transporterdetails+"\"");
                    tw.WriteLine("QRCODE 117,209,L,3,A,180,M2,S7,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 158,128,\"0\",180,8,7,\"" + model.inwmasterid + "\"");
                    tw.WriteLine("TEXT 567,199,\"0\",180,8,8,\"" + model.pono + "\"");
                    //tw.WriteLine("TEXT 839,354,\"0\",180,8,8,\"" + model.inwmasterid + "\"");
                    //tw.WriteLine("TEXT 1235,243,\"0\",180,7,7,\"" + model.inwmasterid + "\"");
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

                string printerName = "10.29.2.47";
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
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else if (printMat.codetype == "N")
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else if (printMat.codetype == "A")
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else if (printMat.codetype == "F")
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else if (printMat.codetype == "Y")
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }
                            else
                            {
                                tw.WriteLine("<xpml><page quantity='0' pitch='180.0 mm'></xpml>SIZE 97.5 mm, 180 mm");
                                tw.WriteLine("GAP 3 mm, 0 mm");
                                tw.WriteLine("SPEED 3");
                                tw.WriteLine("DENSITY 12");
                                tw.WriteLine("SET RIBBON ON");
                                tw.WriteLine("DIRECTION 0,0");
                                tw.WriteLine("REFERENCE 0,0");
                                tw.WriteLine("OFFSET 0 mm");
                                tw.WriteLine("SET PEEL OFF");
                                tw.WriteLine("SET CUTTER OFF");
                                tw.WriteLine("SET PARTIAL_CUTTER OFF");
                                tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='180.0 mm'></xpml>SET TEAR ON");
                                tw.WriteLine("CLS");
                                tw.WriteLine("BOX 22,2121,2291,4060,8");
                                tw.WriteLine("BAR 27,3894, 2261, 8");
                                tw.WriteLine("BAR 27,3658, 2261, 8");
                                tw.WriteLine("BAR 27,3348, 2261, 8");
                                tw.WriteLine("BAR 27,3158, 2261, 8");
                                tw.WriteLine("BAR 27,3016, 2261, 8");
                                tw.WriteLine("BAR 27,2831, 2261, 8");
                                tw.WriteLine("BAR 27,2657, 2261, 8");
                                tw.WriteLine("BAR 27,2476, 2261, 8");
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
                                tw.WriteLine("TEXT 2275,4155,\"0\",180,8,8,\"Ref # - \"");
                                //tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\""+printMat.pono +"-"+printMat.lineitemno+" \"");
                                tw.WriteLine("TEXT 2054,4155,\"0\",180,8,8,\"4508823548 - 000010 \"");
                                tw.WriteLine("BAR 1739,2125, 8, 1932");
                                tw.WriteLine("TEXT 2235,4008,\"0\",180,8,8,\"Serial No. \"");
                                tw.WriteLine("TEXT 2235,3810,\"0\",180,8,8,\"Material \"");
                                tw.WriteLine("TEXT 2235,3574,\"0\",180,8,8,\"MS Code \"");
                                tw.WriteLine("TEXT 2235,3303,\"0\",180,8,8,\"Order \"");
                                tw.WriteLine("TEXT 2235,3123,\"0\",180,8,8,\"Qty \"");
                                tw.WriteLine("TEXT 2235,2958,\"0\",180,8,8,\"S/O Type \"");
                                tw.WriteLine("TEXT 2235,2778,\"0\",180,8,8,\"Insp Rec. \"");
                                tw.WriteLine("TEXT 2235,2601,\"0\",180,8,8,\"Ship-to: \"");
                                //tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\""+printMat.serialno+" \"");
                                tw.WriteLine("TEXT 1697,4008,\"0\",180,8,8,\" - \"");
                                //tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105"+printMat.material+"\"");
                                tw.WriteLine("BARCODE 1697,3778,\"128M\",77,0,180,6,12,\"!105BOP103260\"");
                                //tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\""+printMat.material +"\"");
                                tw.WriteLine("TEXT 1592,3849,\"0\",180,8,8,\"BOP103260\"");
                                //tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\""+printMat.mscode+" \"");
                                tw.WriteLine("TEXT 1700,3613,\"0\",180,8,8,\" BOP103260 \"");
                                //tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\""+printMat.materialdescription+" \"");
                                tw.WriteLine("TEXT 1697,3456,\"0\",180,8,8,\"DELL 146GB 10K 6G 2.5 SAS HDD, \"");
                                //tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105" + printMat.order + "\"");
                                tw.WriteLine("BARCODE 1697,3252,\"128M\",64,0,180,6,12,\"!105 \"");
                                //tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\""+printMat.order+"\"");
                                tw.WriteLine("TEXT 1592,3324,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 1698,3123,\"0\",180,8,8,\"25/ 25ST  1 OF 1 BOXES\"");
                                //tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"" + printMat.saleordertype + "\"");
                                tw.WriteLine("TEXT 1697,2976,\"0\",180,8,8,\"Y201\"");
                                tw.WriteLine("TEXT 1697,2778,\"0\",180,8,8,\"Not Required\"");
                                //tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"" + printMat.customer+"-"+printMat.customername + "\"");
                                tw.WriteLine("TEXT 1697,2601,\"0\",180,8,8,\"-\"");
                                //tw.WriteLine("TEXT 1697,2325,\"0\",180,6,8,\"" + printMat.assetno+"-"+printMat.assetsubno + "\"");
                                tw.WriteLine("TEXT 1785,1998,\"0\",180,10,9,\"Additional Work Instruction\"");
                                tw.WriteLine("TEXT 2235,1840,\"0\",180,8,8,\"Plant\"");
                                tw.WriteLine("BAR 2015,1698, 8, 202");
                                tw.WriteLine("BAR 2091,590, 8, 1109");
                                tw.WriteLine("BAR 667,971, 8, 728");
                                tw.WriteLine("BAR 1502,200, 8, 390");
                                tw.WriteLine("TEXT 2235,178,\"0\",180,8,8,\"2/2\"");
                                tw.WriteLine("TEXT 1074,159,\"0\",180,8,8,\"GR# : -\"");
                                //tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"" + printMat.plant + "\"");
                                tw.WriteLine("TEXT 1977,1840,\"0\",180,8,8,\"-\"");
                                tw.WriteLine("BARCODE 1151,1847,\"128M\",107,0,180,6,12,\"!105\"!1005");
                                tw.WriteLine("TEXT 2235,1637,\"0\",180,8,8,\"#\"");
                                tw.WriteLine("TEXT 1785,1639,\"0\",180,8,8,\"Carry -in -place\"");
                                tw.WriteLine("TEXT 438,1639,\"0\",180,8,8,\"S_Loc\"");
                                tw.WriteLine("TEXT 2211,1249,\"0\",180,8,8,\"1\"");
                                tw.WriteLine("TEXT 2207,1086,\"0\",180,8,8,\"2\"");
                                tw.WriteLine("TEXT 2195,910,\"0\",180,8,8,\"3\"");
                                tw.WriteLine("TEXT 2225,720,\"0\",180,8,8,\"SP\"");
                                tw.WriteLine("TEXT 2016,720,\"0\",180,8,8,\" - \"");
                                tw.WriteLine("TEXT 2228,1405,\"0\",180,8,8,\"GR\"");
                                tw.WriteLine("TEXT 2226,521,\"0\",180,8,8,\"Loading Date\"");
                                tw.WriteLine("TEXT 1421,534,\"0\",180,8,8,\"" + printMat.loadingdate + "\"");
                                tw.WriteLine("TEXT 2207,329,\"0\",180,8,8,\"Linkage No.\"");
                                tw.WriteLine("BAR 1442,220, 8, 143");
                                tw.WriteLine("BAR 1434,220, 4, 143");
                                tw.WriteLine("BAR 1414,220, 12, 143");
                                tw.WriteLine("BAR 1402,220, 4, 143");
                                tw.WriteLine("BAR 1390,220, 8, 143");
                                tw.WriteLine("BAR 1370,220, 12, 143");
                                tw.WriteLine("BAR 1358,220, 4, 143");
                                tw.WriteLine("BAR 1342,220, 4, 143");
                                tw.WriteLine("BAR 1330,220, 8, 143");
                                tw.WriteLine("BAR 1306,220, 12, 143");
                                tw.WriteLine("BAR 1290,220, 4, 143");
                                tw.WriteLine("BAR 1278,220, 8, 143");
                                tw.WriteLine("BAR 1266,220, 8, 143");
                                tw.WriteLine("BAR 1246,220, 4, 143");
                                tw.WriteLine("BAR 1238,220, 4, 143");
                                tw.WriteLine("BAR 1226,220, 4, 143");
                                tw.WriteLine("BAR 1214,220, 8, 143");
                                tw.WriteLine("BAR 1194,220, 12, 143");
                                tw.WriteLine("BAR 1182,220, 4, 143");
                                tw.WriteLine("BAR 1166,220, 4, 143");
                                tw.WriteLine("BAR 1154,220, 8, 143");
                                tw.WriteLine("BAR 1130,220, 12, 143");
                                tw.WriteLine("BAR 1114,220, 4, 143");
                                tw.WriteLine("BAR 1102,220, 8, 143");
                                tw.WriteLine("BAR 1090,220, 8, 143");
                                tw.WriteLine("BAR 1070,220, 4, 143");
                                tw.WriteLine("BAR 1062,220, 4, 143");
                                tw.WriteLine("BAR 1046,220, 8, 143");
                                tw.WriteLine("BAR 1026,220, 16, 143");
                                tw.WriteLine("BAR 1014,220, 8, 143");
                                tw.WriteLine("BAR 1006,220, 4, 143");
                                tw.WriteLine("BAR 998,220, 4, 143");
                                tw.WriteLine("BAR 978,220, 16, 143");
                                tw.WriteLine("BAR 962,220, 4, 143");
                                tw.WriteLine("BAR 954,220, 4, 143");
                                tw.WriteLine("BAR 934,220, 16, 143");
                                tw.WriteLine("BAR 914,220, 8, 143");
                                tw.WriteLine("BAR 898,220, 12, 143");
                                tw.WriteLine("BAR 882,220, 4, 143");
                                tw.WriteLine("BAR 870,220, 8, 143");
                                tw.WriteLine("BAR 846,220, 12, 143");
                                tw.WriteLine("BAR 838,220, 4, 143");
                                tw.WriteLine("BAR 826,220, 8, 143");
                                tw.WriteLine("TEXT 807,323,\"0\",180,8,8,\"2004895294000150\"");
                                tw.WriteLine("TEXT 2235,2440,\"0\",180,8,8,\"Sales Order\"");
                                tw.WriteLine("TEXT 2235,2361,\"0\",180,8,8,\"& Line Item\"");
                                //tw.WriteLine("TEXT 2235,2281,\"0\",180,8,8,\"text\"");
                                tw.WriteLine("PRINT 1,1");
                                tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                            }


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
                string printerName = "10.29.2.44";
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
                            tw.WriteLine("<xpml><page quantity='0' pitch='38.0 mm'></xpml>SIZE 97.5 mm, 38 mm");
                            //tw.WriteLine("GAP 3 mm, 0 mm");
                            tw.WriteLine("DIRECTION 0,0");
                            tw.WriteLine("SET RIBBON ON");
                            
                            tw.WriteLine("REFERENCE 0,0");
                            tw.WriteLine("OFFSET 0 mm");
                            tw.WriteLine("SET PEEL OFF");
                            tw.WriteLine("SET CUTTER OFF");
                            tw.WriteLine("SET PARTIAL_CUTTER OFF");
                            tw.WriteLine("<xpml></page></xpml><xpml><page quantity='1' pitch='38.0 mm'></xpml>SET TEAR ON");
                            
                            tw.WriteLine("CLS");
                            tw.WriteLine("BOX 15,9,1137,436,4");
                            tw.WriteLine("BAR 244,11, 4, 424");
                            tw.WriteLine("BAR 17,226, 1119, 4");
                            tw.WriteLine("BAR 245,293, 891, 4");
                            tw.WriteLine("BAR 245,362, 891, 4");
                            tw.WriteLine("BAR 846,9, 4, 424");
                            tw.WriteLine("CODEPAGE 1252");
                            //tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            //tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");
                            
                            tw.WriteLine("TEXT 1128,416,\"0\",180,8,8,\"Material Code: \"");
                            tw.WriteLine("TEXT 1128,346,\"0\",180,8,8,\"Received Date: \"");
                            tw.WriteLine("TEXT 1128,273,\"0\",180,8,8,\"Gate Entry ID: \"");
                            tw.WriteLine("TEXT 1128,169,\"0\",180,8,8,\"PO No. - InvoiceNo.\"");
                            tw.WriteLine("TEXT 1128,68,\"0\",180,8,8,\"Quantity\"");
                            tw.WriteLine("TEXT 839,416,\"0\",180,8,8,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 839,273,\"0\",180,8,8,\"" + onholdprintdata.gateentryid + "\"");
                            tw.WriteLine("TEXT 839,207,\"0\",180,8,8,\"" + onholdprintdata.pono +"-"+onholdprintdata.invoiceno+ "\"");
                            tw.WriteLine("QRCODE 175,410,L,4,A,180,M2,S7,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 241,299,\"0\",180,7,7,\"" + onholdprintdata.materialid + "\"");
                            tw.WriteLine("TEXT 839,346,\"0\",180,8,8,\"" + onholdprintdata.receiveddate + "\"");
                            tw.WriteLine("TEXT 839,68,\"0\",180,8,8,\"" +i+"/"+ onholdprintdata.noofprint + "\"");
                            tw.WriteLine("BAR 600,78, 536, 4");
                            tw.WriteLine("BAR 245,78, 357, 4");
                            tw.WriteLine("QRCODE 183,216,L,5,A,180,M2,S7,\"" + onholdprintdata.gateentryid + "\"");
                            tw.WriteLine("TEXT 235,85,\"0\",180,8,8,\"" +onholdprintdata.gateentryid + "\"");
                            tw.WriteLine("PRINT 1,1");
                            tw.WriteLine("<xpml></page></xpml><xpml><end/></xpml>");

                        }

                    }
                }
                try
                {
                    //Send dat ato printer
                    string printerName = "10.29.2.48";
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
