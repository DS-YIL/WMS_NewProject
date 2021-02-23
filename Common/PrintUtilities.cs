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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.CoreCompat.System.Drawing;

namespace WMS.Common
{
	public class PrintUtilities
	{
        public string PrintQRCode(string path, string printerName)
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


        public string generatebarcode(string path, string barcodename)
        {
            string barcodepath = null;
            try
            {
              if(barcodename!=null)
                {
                    BarcodeWriter writer = new BarcodeWriter
                    {
                        Format = BarcodeFormat.CODE_128,
                        Options = new EncodingOptions
                        {
                            Height = 90,
                            Width = 100,
                            PureBarcode = false,
                            Margin = 1,

                        },
                    };
                    var bitmap = writer.Write(barcodename);

                    // write text and generate a 2-D barcode as a bitmap
                    writer
                        .Write(barcodename)
                        .Save(@"D:\WMS_NewProject\Barcodes\" + barcodename + "_" + DateTime.Now + ".bmp");

                    barcodepath =  barcodename + "_" + DateTime.Now + ".bmp";
                }
               
            }
            catch(Exception ex)
            {
                return "Error";
            }
            return barcodepath;
        }

        //Generate QRCode
        public string generateqrcode(string path, string qrcodename)
        {
            string barcodepath = null;
            try
            {
                if (qrcodename != null)
                {
                    BarcodeWriter writer = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new EncodingOptions
                        {
                            Height = 90,
                            Width = 100,
                            PureBarcode = false,
                            Margin = 1,

                        },
                    };
                    var bitmap = writer.Write(qrcodename);

                    // write text and generate a 2-D barcode as a bitmap
                    writer
                        .Write(qrcodename)
                        .Save(path + qrcodename + "_" + DateTime.Now + ".bmp");

                    barcodepath = "./Barcodes/" + qrcodename + "_" + DateTime.Now + ".bmp";
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
            return barcodepath;
        }
    }
}
