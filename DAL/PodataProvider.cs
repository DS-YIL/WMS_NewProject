/*
    Name of File : <<PodataProvider>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this DAL>>
    Review Date :<<>>   Reviewed By :<<>>
    Version : 0.1 
    Sourcecode Copyright : Yokogawa India Limited
*/

using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WMS.Common;
using WMS.Interfaces;
using System.Web;
using WMS.Models;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Sockets;
using System.Net;
using ZXing;
using ZXing.Common;
using ZXing.CoreCompat.System.Drawing;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using ZXing.QrCode.Internal;
using System.Data.OleDb;

/*
    Name of namespace : <<WMS>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this DAL>>
    Review Date :<<>>   Reviewed By :<<>>
    Sourcecode Copyright : Yokogawa India Limited
*/

namespace WMS.DAL
{

	/*
    Name of Class : <<PodataProvider>>  Author :<<Shashikala>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<All code Functionalities except login are written in this class>>
    Review Date :<<>>   Reviewed By :<<>>
   
*/

	public class PodataProvider : IPodataService<OpenPoModel>
	{
		Configurations config = new Configurations();
		ErrorLogTrace log = new ErrorLogTrace();


		/*
    Name of Function : <<CheckPoexists>>  Author :<<Ramesh>>  
    Date of Creation <<12-12-2019>>
    Purpose : <<check pono exists or not >>
	<param name="PONO"></param>
    Review Date :<<>>   Reviewed By :<<>>
	*/

		public OpenPoModel CheckPoexists(string PONO)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					OpenPoModel returndata = new OpenPoModel();
					pgsql.Open();
					//WMSResource.checkponoexists.Replace("#pono", PONO);
					//string query = "select pono,suppliername as vendorname from wms.wms_polist where pono = '" + PONO + "'";
					string query = "select asno.asn as asnno,asno.pono,pl.suppliername as vendorname from wms.wms_asn asno left outer join wms.wms_polist pl on pl.pono = asno.pono where asno.asn = '" + PONO.Trim() + "'";
					//string query = "select asn as asnno,pono from wms.wms_asn where asn = '" + PONO.Trim() + "'";

					var podata = pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);

					int count = podata.Result.Count();

					if (podata != null && (podata.Result.Count()>0))
					{
						//List<OpenPoModel> podataList = podata.Result.ToList();
						//returndata = podata.Result.FirstOrDefault();
						string postr = "";
						int i = 0;
						foreach(OpenPoModel model in podata.Result)
                        {
							returndata = model;
							if(i>0)
                            {
								postr += ",";
                            }
							postr += model.pono;
							returndata.pono = postr;
							i++;
							

						}
						returndata.ispono = false;
						returndata.isasn = true;
						returndata.issupplier = false;
						return returndata;

					}
					else
					{
						query = "select pono,suppliername as vendorname from wms.wms_polist where pono = '" + PONO.Trim() + "'";
						var podata1 = pgsql.QueryFirstOrDefault<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
						if (podata1 != null)
						{
							podata1.ispono = true;
							podata1.isasn = false;
							podata1.issupplier = false;
						}
                        else
                        {
							query = "select suppliername as vendorname from wms.wms_polist where LOWER(suppliername) = LOWER('" + PONO.Trim() + "') ";
							var podata2 = pgsql.QueryFirstOrDefault<OpenPoModel>(
							   query, null, commandType: CommandType.Text);
							if (podata2 != null)
							{
								podata2.ispono = false;
								podata2.isasn = false;
								podata2.issupplier = true;
							}
							return podata2;
						}
						return podata1;
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "CheckPoexists", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
			//throw new NotImplementedException();
		}


		/*
		Name of Function : <<getOpenPoList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get lst of open pono list>>
		<param name="loginid"></param>
		 <param name="pono"></param>
		 <param name="docno"></param>
		 <param name="vendorid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> getOpenPoList(string loginid, string pono = null, string docno = null, string vendorid = null)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.openpolist.Replace("#projectmanager", loginid);
					if (pono != null)
					{
						query = query + " and wp.pono='" + pono + "'";
					}
					if (docno != null)
					{
						query = query + " and wp.documentno='" + docno + "'";
					}
					if (vendorid != null)
					{
						query = query + " and  wp.vendorid=" + vendorid;
					}
					query = query + " group by track.pono,wp.pono order by wp.pono asc ";

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getOpenPoList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getPODataList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get po numbers>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<POList>> getPODataList(string suppliername)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//List<POList> objpo = null;
				try
				{
					//objpo = new List<POList>();
					string query = "select pono,suppliername from wms.wms_polist where (suppliername='"+suppliername+"' AND type='po') ";
					//string query = "select asno.asn as asnno,asno.pono,pl.suppliername as vendorname from wms.wms_asn asno left outer join wms.wms_polist pl on pl.pono = asno.pono where pl.suppliername = '#suppliername'";

					await pgsql.OpenAsync();
					var objpo = await pgsql.QueryAsync<POList>(
					   query, null, commandType: CommandType.Text);
					return objpo;
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "getPOList", ex.StackTrace.ToString());
					return null;
				}

			}
		}

			/*
			Name of Function : <<getPOList>>  Author :<<Gayathri>>  
			Date of Creation <<12-12-2019>>
			Purpose : <<get po numbers and qty>>
			<param: name=postatus - Displaying data based on status selected></param>
			Review Date :<<>>   Reviewed By :<<>>
			*/
			public async Task<IEnumerable<POList>> getPOList(string postatus)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				List<POList> objpo = null;
				try
				{
					//if(postatus=="Instock")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result= await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach(var podata in result)
					//                   {
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '"+podata.POno+"'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);
					//		if(po>0)
					//                       {
					//			podata.status = "Instock";
					//			objpo.Add(podata);
					//		}

					//	}

					//	return objpo;
					//}
					//else if(postatus== "SecurityCheck")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result = await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach (var podata in result)
					//	{
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);

					//		if (po == 0)
					//		{
					//			poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_storeinward sinw on secinw.inwmasterid = sinw.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//			 po = pgsql.QuerySingleOrDefault<int?>(
					//								poquery, null, commandType: CommandType.Text);

					//			if (po > 0)
					//			{
					//				podata.status = "Security Check";
					//				objpo.Add(podata);
					//			}
					//		}



					//	}

					//	return objpo;
					//}
					//else if(postatus== "Pending")
					//               {
					//	objpo = new List<POList>();
					//	string query = WMSResource.getpolist;
					//	await pgsql.OpenAsync();
					//	var result = await pgsql.QueryAsync<POList>(
					//	   query, null, commandType: CommandType.Text);

					//	foreach (var podata in result)
					//	{
					//		string poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_stock sk on secinw.inwmasterid = sk.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//		int? po = pgsql.QuerySingleOrDefault<int?>(
					//							poquery, null, commandType: CommandType.Text);

					//		if (po == 0)
					//		{
					//			poquery = "select count(*) from wms.wms_securityinward secinw join wms.wms_storeinward sinw on secinw.inwmasterid = sinw.inwmasterid where secinw.pono = '" + podata.POno + "'";

					//			po = pgsql.QuerySingleOrDefault<int?>(
					//							   poquery, null, commandType: CommandType.Text);

					//			if (po == 0)
					//			{
					//				 poquery = "select count(*) from wms.wms_securityinward where pono ='" + podata.POno + "'";

					//				 po = pgsql.QuerySingleOrDefault<int?>(
					//									poquery, null, commandType: CommandType.Text);
					//				if (po == 0)
					//				{
					//					podata.status = "Pending";
					//					objpo.Add(podata);
					//				}
					//			}
					//		}
					//	}

					//	return objpo;
					//}

					if (postatus == "Instock")
					{
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono in (select DISTINCT sinw.pono from wms.wms_stock sinw) group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);
						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Instock";
							}
							return result;
						}

					}
					else if (postatus == "SecurityCheck")
					{
						//If PO status is security check get materials
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono in (select DISTINCT sinw.pono from wms.wms_securityinward sinw where sinw.grnnumber is not null and sinw.pono not in (select  DISTINCT pono from wms.wms_stock))group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);

						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Security Check";
							}
							return result;
						}

					}
					else if (postatus == "Pending")
					{
						string query = "select mats.pono,SUM(mats.materialqty) as qty from wms.wms_pomaterials mats where mats.pono not in (select DISTINCT pono from wms.wms_securityinward) group by mats.pono";
						await pgsql.OpenAsync();
						var result = await pgsql.QueryAsync<POList>(
						   query, null, commandType: CommandType.Text);
						if (result != null)
						{
							foreach (var data in result)
							{
								data.status = "Pending";
							}
							return result;
						}

					}

					return null;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getPOList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<generateBarcodeMaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Generate QRCode>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public printMaterial generateBarcodeMaterial(printMaterial printMat)
		{
			try
			{
				string path = "";

				path = Environment.CurrentDirectory + @"\Barcodes\";

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//Check if the material is already printed
					string query = "Select * from wms.wms_securityinward sinw join wms.wms_printstatusmaterial psmat on psmat.inwmasterid=sinw.inwmasterid where sinw.pono='" + printMat.pono + "' and sinw.invoiceno='" + printMat.invoiceno + "' and psmat.materialid='" + printMat.materialid + "'";
					var data = Convert.ToBoolean(DB.ExecuteScalar(query, false));
					if(data!=false)
                    {
						printMat.isprint = true;
                    }
                    else
                    {
						printMat.isprint = false;
					}
				}
				//generate barcode for material code and GRN No.
				var content = printMat.grnno + "-" + printMat.materialid;
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
				var bitmap = writer.Write(content);

				// write text and generate a 2-D barcode as a bitmap
				writer
					.Write(content)
					.Save(path + content + ".bmp");

				printMat.barcodePath = "./Barcodes/" + content + ".bmp";
				//printMat.barcodePath = "./assets/" + content + ".bmp";

				//Barcode design for material code
				//generate barcode for material code and GRN No.
				content = printMat.materialid;
				BarcodeWriter writerData = new BarcodeWriter
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
				bitmap = writerData.Write(content);

				// write text and generate a 2-D barcode as a bitmap
				writer
					.Write(content)
					.Save(path + content + ".bmp");

				printMat.materialcodePath = "./Barcodes/" + content + ".bmp";
				//printMat.materialcodePath = "./assets/" + content + ".bmp";


				
			}
			catch (Exception ex)
			{
				printMat.errorMsg = ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", ex.StackTrace.ToString());
			}
			return printMat;
		}

		/*
		Name of Function : <<printBarcodeMaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Printing barcode- printBarcodeMaterial>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		
		public string printBarcodeMaterial(printMaterial printMat)
		{
			try
			{
				string path = Environment.CurrentDirectory + @"\PRNFiles\";
				bool result = false;
				string printResult = null;
				path = path + printMat.materialid + "-" + string.Format("{0:ddMMyyyyhhmm}", DateTime.Now) + ".prn";
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
						tw.WriteLine("<xpml><page quantity='0' pitch='33.0 mm'></xpml>SIZE 94.10 mm, 38 mm");
						tw.WriteLine("GAP 3 mm, 0 mm");
						//tw.WriteLine("SET RIBBON ON");
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
						tw.WriteLine("BAR 214,15, 3, 272");
						tw.WriteLine("BAR 215,222, 525, 3");
						tw.WriteLine("BAR 9,151, 731, 3");
						tw.WriteLine("BAR 215,86, 525, 3");
						tw.WriteLine("QRCODE 144,251,L,3,A,180,M2,S7,\"" + printMat.materialid + "\"");
						tw.WriteLine("QRCODE 144,106,L,3,A,180,M2,S7,\"" + printMat.grnno +"-"+printMat.materialid+ "\"");
						tw.WriteLine("CODEPAGE 1252");
						tw.WriteLine("TEXT 731,268,\"0\",180,9,9,\"Material Code: \"");
						tw.WriteLine("TEXT 731,195,\"0\",180,8,9,\"Received Date: \"");
						tw.WriteLine("TEXT 732,124,\"0\",180,6,6,\"WMS GRN No. - Material Code: \"");
						tw.WriteLine("TEXT 704,56,\"0\",180,9,9,\"Quantity\"");
						tw.WriteLine("TEXT 482,265,\"0\",180,14,9,\""+printMat.grnno+"\"");
						tw.WriteLine("TEXT 484,124,\"0\",180,9,6,\"" + printMat.grnno + "-" + printMat.materialid + "\"");
						tw.WriteLine("TEXT 486,59,\"0\",180,13,9,\"" + printMat.noofprint + "/" + printMat.noofprint + "\"");
						tw.WriteLine("TEXT 485,199,\"0\",180,13,11,\""+printMat.receiveddate+"\"");

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
					//this._poService.updateSecurityPrintHistory(model);
					return "success";
				}
				else
				{
					return "Error Occured";
				}
			

	
	}
			catch (Exception ex)
			{
				printMat.errorMsg = ex.Message;
				log.ErrorMessage("PODataProvider", "generateBarcodeMaterial", ex.StackTrace.ToString());
			}
			return "success";
		}

		/*
		Name of Function : <<InsertBarcodeInfo>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Inserting barcode info>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public string InsertBarcodeInfo(BarcodeModel dataobj)
		{
			try
			{
				//dataobj.docfile = ;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					if (dataobj.pono == null)
					{
						//If PO is empty
						foreach (var podata in dataobj.polist)
						{
							if (dataobj.pono == null)
							{
								dataobj.pono = podata.POno;
							}
							else
							{
								dataobj.pono = dataobj.pono + "," + podata.POno;
							}
						}

					}
					var q1 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
					int count = int.Parse(DB.ExecuteScalar(q1, null).ToString());

					if (count >= 1)
					{
						var q2 = WMSResource.getprintdetailsforinvoice.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
						var data = Convert.ToBoolean(DB.ExecuteScalar(q2, false));
						if(data==true)
                        {
							return "3"; //for invoice already exist and if data is printed
						}
                        else
                        {
							return "2"; //for invoice already exist
						}
						
						
					}
					else
					{
						dataobj.createddate = System.DateTime.Now;
						var result = "0";
						string insertqueryforinvoice = WMSResource.insertinvoicedata;
						dataobj.receiveddate = System.DateTime.Now;
						if (dataobj.pono == "NONPO")
						{
							string compare = "NP" + DateTime.Now.Year.ToString().Substring(2);
							string Query = WMSResource.getmaxnonpo;
							var data = DB.QueryFirstOrDefault<POList>(
							Query, null, commandType: CommandType.Text);
							if (data != null)
							{
								string[] poserial = data.POno.Split('P');
								int serial = Convert.ToInt32(poserial[1].Substring(2));
								int nextserial = serial + 1;
								string nextpo = "NP" + DateTime.Now.Year.ToString().Substring(2) + nextserial.ToString().PadLeft(5, '0');
								dataobj.pono = nextpo;

								var q2 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
								int count1 = int.Parse(DB.ExecuteScalar(q2, null).ToString());

								if (count1 >= 1)
								{
									return "2"; //for invoice already exist
								}
								string type = "NON PO";
								string insertpoqry = WMSResource.insertpo;
								DB.Execute(insertpoqry, new
								{
									dataobj.pono,
									dataobj.suppliername,
									type
									//barcodeid,
								});


							}
							else
							{
								string nextpo = "NP" + DateTime.Now.Year.ToString().Substring(2) + "00001";
								dataobj.pono = nextpo;
								var q2 = WMSResource.getinvoiceexists.Replace("#pono", dataobj.pono).Replace("#invno", dataobj.invoiceno);
								int count1 = int.Parse(DB.ExecuteScalar(q2, null).ToString());

								if (count1 >= 1)
								{
									return "2"; //for onvoice already exist
								}
								string type = "NON PO";
								string insertpoqry = WMSResource.insertpo;
								try
								{
									DB.Execute(insertpoqry, new
									{
										dataobj.pono,
										dataobj.suppliername,
										type
										//barcodeid,
									});
								}
								catch (Exception ex)
								{
									string msg = ex.Message;
								}

							}


						}
						dataobj.invoicedate = DateTime.Now;

						string filename = "";
						if (dataobj.pono.StartsWith("NP"))
						{
							filename = dataobj.pono + "_" + dataobj.docfile;
						}

						var results = DB.ExecuteScalar(insertqueryforinvoice, new
						{
							dataobj.invoicedate,
							dataobj.departmentid,
							dataobj.invoiceno,
							dataobj.receiveddate,
							dataobj.receivedby,
							dataobj.pono,
							dataobj.deleteflag,
							dataobj.suppliername,
							dataobj.asnno,
							dataobj.inwardremarks,
							dataobj.grnnumber,
							dataobj.createdby,
							filename,
							dataobj.inwmasterid,
							dataobj.vehicleno,
							dataobj.transporterdetails
							//barcodeid,
						});

						if (results != null &&  results != "" )
						{
							dataobj.inwmasterid = results.ToString();
							dataobj.barcode = dataobj.pono + "_" + dataobj.invoiceno;
							//insert bar code data
							dataobj.createddate = DateTime.Now;
							string insertbarcodequery = WMSResource.insertbarcodedata;//to insert bar code data
							var barcodeResult = DB.Execute(insertbarcodequery, new
							{
								dataobj.barcode,
								dataobj.createdby,
								dataobj.createddate,
								dataobj.deleteflag,
								dataobj.pono,
								dataobj.invoiceno,
								dataobj.inwmasterid

							});

							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = dataobj.pono;
							emailmodel.invoiceno = dataobj.invoiceno;
							emailmodel.receivedby = dataobj.receivedby;
							emailmodel.receiveddate = dataobj.receiveddate;
							emailmodel.asnno = dataobj.asnno;
							emailmodel.suppliername = dataobj.suppliername;
							emailmodel.grnnumber = dataobj.grnnumber;
							emailmodel.employeeno = dataobj.receivedby;
							emailmodel.inwmasterid = dataobj.inwmasterid;


							//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							//emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 1,3);

						}


						////}
						return (dataobj.pono);
					}

				}
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "InsertBarcodeInfo", Ex.StackTrace.ToString());
				string errorstring = Ex.Message;
				if(errorstring.Contains("duplicate key"))
                {
					return "2";
				}
                else
                {
                    return "Error:"+Ex.Message;
				}
				
			}

		}

		/*
		Name of Function : <<getinvoiveforpo>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Invoice details based on PONO.>>
		<param name="PONO"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<InvoiceDetails>> getinvoiveforpo(string PONO)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getInvoiceDetails.Replace("#pono", PONO);
					return await pgsql.QueryAsync<InvoiceDetails>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinvoiveforpo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getMaterialDetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Material details based on grn number>>
		<param name="grnNo"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialDetails>> getMaterialDetails(string grnNo)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					List<MaterialDetails> objMaterial = new List<MaterialDetails>();
					MaterialDetails result = null;
					string getMatQuery = WMSResource.getmatforgrnno.Replace("#grn", grnNo);
					var MaterialList = await pgsql.QueryAsync<MaterialDetails>(
					   getMatQuery, null, commandType: CommandType.Text);

					if (MaterialList != null)
					{
						int totalissed = 0;
						foreach (MaterialDetails mtData in MaterialList)
						{
							if (mtData.materialid != null && mtData.materialid != "")
							{
								result = new MaterialDetails();
								result.materialid = mtData.materialid;
								result.materialdescription = mtData.materialdescription;
								//result.availableqty = mtData.availableqty;
								result.grnnumber = mtData.grnnumber;
								result.confirmqty = mtData.confirmqty;
								//To get issued qty get data from material issue, material reserve and gatepassmaterial table
								int issuedqty = 0;
								int reservedqty = 0;

								Enquirydata enquiryobj = new Enquirydata();
								string availableQtyqry = "select sum(availableqty) as availableqty from wms.wms_stock where materialid ='" + mtData.materialid + "' and pono='" + mtData.pono + "' and inwmasterid = '" + mtData.inwmasterid +"'";
								enquiryobj = pgsql.QuerySingleOrDefault<Enquirydata>(
												availableQtyqry, null, commandType: CommandType.Text);
								result.availableqty = enquiryobj.availableqty;

								ReportModel modelobj = new ReportModel();
								string matIssuedQuery = "select sum(iss.issuedqty)as issuedqty from wms.wms_materialissue iss" +
									" join wms.wms_stock sk on sk.pono='" +mtData.pono+"' where iss.itemid = sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid= '" + mtData.inwmasterid +"'";
								modelobj = pgsql.QuerySingleOrDefault<ReportModel>(
												matIssuedQuery, null, commandType: CommandType.Text);
								if (modelobj != null)
								{
									issuedqty = modelobj.issuedqty;
								}
								//Get material reserved qty
								ReserveMaterialModel modeldataobj = new ReserveMaterialModel();
								string matReserveQuery = "select sum(reser.reservequantity )as reservedqty from wms.materialreservedetails reser join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where reser.itemid =sk.itemid and sk.materialid='" + mtData.materialid + "' and sk.inwmasterid='" + mtData.inwmasterid+"'";
								modeldataobj = pgsql.QuerySingleOrDefault<ReserveMaterialModel>(
												matReserveQuery, null, commandType: CommandType.Text);
								if (modeldataobj != null)
								{
									reservedqty = modeldataobj.reservedqty;
								}
								int gatepassissuedqty = 0;

								//get material in gatepass
								gatepassModel obj = new gatepassModel();
								//string matgateQuery = "select sum(gtmat.quantity)as quantity from wms.wms_gatepassmaterial gtmat left join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid where materialid = '" + mtData.materialid + "' and gp.approvedon != null and gp.approverstatus!=null";
								////string matgateQuery = "select sum(matiss.issuedqty)as quantity from wms.wms_gatepassmaterial gtmat join wms.wms_materialissue matiss on matiss.gatepassmaterialid = gtmat.gatepassmaterialid join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid join wms.wms_stock sk on sk.pono='" + mtData.pono + "' where sk.inwmasterid ="+ mtData.inwmasterid+ "and matiss.itemid =sk.itemid and gtmat.materialid ='"+mtData.materialid+"'";
								//IssueRequestModel obj = new IssueRequestModel();
								//pgsql.Open();

								//obj = pgsql.QuerySingle<gatepassModel>(
								//   matgateQuery, null, commandType: CommandType.Text);

								//gatepassissuedqty = obj.quantity;
								//totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty) + gatepassissuedqty;
								totalissed = Convert.ToInt32(issuedqty) + Convert.ToInt32(reservedqty);
								result.issued = totalissed;


							}
							objMaterial.Add(result);
						}
						//objMaterial.Add(result);
					}





					//string reqDetailsQuery = WMSResource.getrequestDetailsMaterial.Replace("#grn", grnNo);



					//var requestDetails = await pgsql.QueryAsync<MaterialDetails>(
					//   reqDetailsQuery, null, commandType: CommandType.Text);
					//if(requestDetails != null)
					//{
					//    foreach(MaterialDetails matData in requestDetails)
					//    {
					//        string reserveQuery = WMSResource.getreserveQtyDetailsMaterial.Replace("#grn", matData.grnnumber);
					//        string issuedQuery = WMSResource.getissuedQtyDetailsMaterial.Replace("#grn", matData.grnnumber);
					//        var reserveDetails = await pgsql.QueryAsync<MaterialDetails>(
					//   reserveQuery, null, commandType: CommandType.Text);



					//        var issuedDetails = await pgsql.QueryAsync<MaterialDetails>(
					//  issuedQuery, null, commandType: CommandType.Text);
					//    }
					//    result.grnnumber = requestDetails.FirstOrDefault().grnnumber;
					//    result.reservedqty = requestDetails.FirstOrDefault().reservedqty;
					//    result.qtyavailable = requestDetails.FirstOrDefault().qtyavailable;
					//    result.qtytotal = requestDetails.FirstOrDefault().qtytotal;
					//}









					//var finaltempdata = requestDetails.Concat(reserveDetails);
					//var finaldata = finaltempdata.Concat(issuedDetails);





					//foreach(MaterialDetails mtdata in finaldata)
					//{
					//    mtdata.issued =Convert.ToInt32( mtdata.reservedqty) + Convert.ToInt32(mtdata.issuedqty);
					//}
					//return finaldata;



					return objMaterial;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getMaterialDetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getlocationdetails>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get location details based on material id>>
		<param name="materialid"></param>
		 <param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<LocationDetails>> getlocationdetails(string materialid, string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getLocationDetails.Replace("#materialid", materialid).Replace("#grn", grnnumber);
					return await pgsql.QueryAsync<LocationDetails>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getlocationdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<getReqMatdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get material requested, acknowledged and issued details>>
		<param name="materialid"></param>
		 <param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReqMatDetails>> getReqMatdetails(string materialid, string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//ReqMatDetails obj = new ReqMatDetails();
				ReqMatDetails objs = null;
				List<ReqMatDetails> listobj = new List<ReqMatDetails>();
				try
				{
					string query = WMSResource.getMaterialRequestDetails.Replace("#materialid", materialid).Replace("#grnnumber", grnnumber);
					//await pgsql.OpenAsync();
					var obj = await pgsql.QueryAsync<ReqMatDetails>(
					   query, null, commandType: CommandType.Text);
					foreach (var matdata in obj)
					{
						string reservequery = "select max(emp.name) as requestername,max(emp1.name)  as approvedby,max(res.reservedon) as issuedon,sum(resdetails.reservequantity) as quantity from wms.materialreservedetails resdetails join wms.materialreserve res on resdetails.reserveid = res.reserveid join wms.wms_stock sk on sk.pono='" + matdata.pono + "' join wms.employee emp on res.reservedby = emp.employeeno join wms.employee emp1 on emp1.employeeno = res.reservedby where resdetails.itemid =sk.itemid and sk.materialid='" + materialid + "' and sk.inwmasterid='" + matdata.inwmasterid +"'";
							//"select max(emp.name) as requestername,max(emp1.name)  as approvedby,max(res.reservedon) as issuedon,sum(res.reservedqty) as quantity from wms.wms_materialreserve res left join wms.employee emp on res.reservedby = emp.employeeno join wms.wms_stock sk on sk.pono='"+ matdata.pono+ "'  left join wms.employee emp1 on emp1.employeeno = res.releasedby where res.itemid=sk.itemid and sk.materialid='"+ materialid + "' and sk.inwmasterid="+matdata.inwmasterid;
						var data = pgsql.QuerySingle<ReqMatDetails>(
						   reservequery, null, commandType: CommandType.Text);
						objs = new ReqMatDetails();
						objs.quantity = data.quantity;
						objs.type = "Project Reserve";
						objs.requestername = data.requestername;
						objs.issuedon = data.issuedon;
						objs.details = matdata.jobname;
						objs.approvername = data.approvername;
						if (objs.quantity != 0)
						{
							listobj.Add(objs);
						}

						string requstedquery = "select sum(issue.issuedqty)as quantity,max(emp.name) as requestername,max(emp1.name) as approvername,max(issue.itemissueddate) as issuedon from wms.wms_materialissue issue inner join wms.materialrequestdetails matreqdetails on matreqdetails.id = issue.requestmaterialid inner join wms.materialrequest req on matreqdetails.requestid = req.requestid  left join wms.employee emp on emp.employeeno = req.requesterid join wms.wms_stock sk on sk.pono='" + matdata.pono + "' left join wms.employee emp1 on emp1.employeeno = req.approverid  where issue.itemid=sk.itemid and sk.materialid='" + materialid + "' and sk.inwmasterid='" + matdata.inwmasterid  +"'";	
						var data1 = pgsql.QuerySingle<ReqMatDetails>(
					   requstedquery, null, commandType: CommandType.Text);
						objs = new ReqMatDetails();
						objs.quantity = data1.quantity;
						objs.type = "Project Requested";
						objs.requestername = data1.requestername;
						objs.issuedon = data1.issuedon;
						objs.details = matdata.jobname;
						objs.approvername = data1.approvername;
						objs.acknowledge = data1.requestername;
						if (objs.quantity != 0)
						{
							listobj.Add(objs);
						}
						//string gatepassquery = " select max(gate.gatepasstype)as gatepasstype,sum(mat.quantity)as quantity,max(gate.approvedon) as issuedon,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_gatepass gate  inner join wms.wms_gatepassmaterial mat on mat.gatepassid = gate.gatepassid  left join wms.employee emp on emp.employeeno = gate.requestedby left join wms.employee emp1 on emp1.employeeno = gate.approvedby where mat.materialid = '" + obj.materialid + "' and gate.approvedon != null and gate.approverstatus!=null";
						string gatepassquery = "select gp.gatepasstype as type,sum(matiss.issuedqty) as quantity,max(gp.approvedon) as issuedon,max(emp.name) as requestername, max(emp1.name) as approvername from wms.wms_gatepassmaterial gtmat join wms.wms_materialissue matiss on matiss.gatepassmaterialid = gtmat.gatepassmaterialid join wms.wms_gatepass gp on gp.gatepassid = gtmat.gatepassid left join wms.employee emp on emp.employeeno = gp.requestedby left join wms.employee emp1 on emp1.employeeno = gp.approverid join wms.wms_stock sk on sk.pono = '" + matdata.pono+"' where sk.inwmasterid = '"+ matdata.inwmasterid + "' and matiss.itemid = sk.itemid and gtmat.materialid = '"+ materialid + "' group by gp.gatepasstype";
							//"select max(gate.gatepasstype)as gatepasstype,sum(wmissue.issuedqty)as quantity,max(gate.approvedon) as issuedon,max(emp.name) as requestername,max(emp1.name) as approvername from wms.wms_gatepass gate  inner join wms.wms_gatepassmaterial mat on mat.gatepassid = gate.gatepassid  left join wms.employee emp on emp.employeeno = gate.requestedby left join wms.employee emp1 on emp1.employeeno = gate.approvedby join wms.wms_materialissue wmissue  on mat.gatepassmaterialid = wmissue.gatepassmaterialid where mat.materialid = '" + matdata.materialid + "' and gate.approvedon != null and gate.approverstatus != null";
						var data2 = await pgsql.QueryAsync<ReqMatDetails>(
					   gatepassquery, null, commandType: CommandType.Text);
						
						if(data2!=null)
                        {
							foreach(var gpdata in data2)
                            {
								if (gpdata.quantity != 0)
								{
									objs = new ReqMatDetails();
									objs.quantity = gpdata.quantity;
									objs.type = gpdata.type;
									objs.requestername = gpdata.requestername;
									objs.issuedon = gpdata.issuedon;
									objs.details = gpdata.jobname;
									objs.approvername = gpdata.approvername;
									objs.acknowledge = gpdata.requestername;
									listobj.Add(objs);
								}
							}
                        }
						

					}
					return listobj;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getReqMatdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}


		/*
		Name of Function : <<GetDeatilsForholdgr>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get details of Hold GR based on status>>
		<param name="status"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> GetDeatilsForholdgr(string status)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getHoldGRdetail.Replace("#status", status);
					var data = await pgsql.QueryAsync<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetDeatilsForholdgr", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<GetDeatilsForthreeWaymatching>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of info for three way matching>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		
		public async Task<IEnumerable<OpenPoModel>> GetDeatilsForthreeWaymatching(string invoiceno, string inwmasterid, bool isgrn, string grnno)
		{

			
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					inwardModel obj = new inwardModel();
					await pgsql.OpenAsync();
					if (!isgrn)
					{
						//string query1 = WMSResource.getinwmasterid.Replace("#pono", pono).Replace("#invoiceno", invoiceno);
						string query1 = WMSResource.getpobyinwardmasterid.Replace("#inw", inwmasterid);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}
					else
					{
						//string query1 = WMSResource.getinwardidbygrn.Replace("#grnno", grnno);
						string query1 = WMSResource.getponobygrn.Replace("#grn", grnno);
						obj = pgsql.QuerySingle<inwardModel>(
						query1, null, commandType: CommandType.Text);
					}

					string pono = obj.pono;
					List<OpenPoModel> datalist = new List<OpenPoModel>();
					if (obj.pono != null && obj.pono != "")
					{
						string query = "";
						if (pono.StartsWith("NP"))
						{
							//Replace("#pono", pono).Replace("#invoiceno", invoiceno)
							query = WMSResource.receivequeryfornonpo;// + pono+"'";//li
							if (isgrn)
							{
								query += " where sinw.grnnumber = '" + grnno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
							}
							else
							{
								query += " where sinw.pono = '" + pono + "' and inw.materialid is NOT NULL  and sinw.invoiceno = '" + invoiceno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
							}

						}
						else
						{
							inwardModel objx = new inwardModel();
							if (!isgrn)
							{
								//string queryx = "select grnnumber,onhold,unholdedby from wms.wms_securityinward where pono = '" + pono + "' and invoiceno = '" + invoiceno + "'";
								//string queryx = WMSResource.isgrnexistsquerybyinvoce.Replace("#pono", pono).Replace("#invno", invoiceno);
								string queryx = WMSResource.isgrnexistbyinwardmasterid.Replace("#inw", inwmasterid);
								objx = pgsql.QuerySingle<inwardModel>(
								 queryx, null, commandType: CommandType.Text);
							}
							else
							{
								//string queryx = "select grnnumber,onhold,unholdedby from wms.wms_securityinward where grnnumber = '" + grnno + "'";
								string queryx = WMSResource.isgrnexistsbygrn.Replace("#grnno", grnno);
								objx = pgsql.QuerySingle<inwardModel>(
								queryx, null, commandType: CommandType.Text);
							}
							string poforquery = pono;
							poforquery = poforquery.Trim();
							poforquery = poforquery.Replace(" ","");
							poforquery = poforquery.Replace(",", "','");

							if ((objx.grnnumber == null || objx.grnnumber == "") && !objx.onhold && (objx.unholdedby == null || objx.unholdedby == ""))
							{


								//query = WMSResource.Getdetailsforthreewaymatching;
								query = WMSResource.getMaterialsforreceipt.Replace("#invoice",invoiceno).Replace("#inw",inwmasterid);
								if (isgrn)
								{
									query += " where sinw.grnnumber = '" + grnno + "'";
								}
								else
								{
									//query += " where mat.pono = '" + pono + "'  and sinw.invoiceno = '" + invoiceno + "'";
									query += " where mat.pono in ('" + poforquery + "')";
								}
							}
							else
							{
								query = WMSResource.receivequeryfornonpo;
								if (isgrn)
								{
									query += " where sinw.grnnumber = '" + grnno + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
								}
								else
								{
									query += " where inw.pono in ('" + poforquery + "')  and sinw.inwmasterid = '" + inwmasterid + "' and (sinw.holdgrstatus is NULL or sinw.holdgrstatus =  'accepted' or sinw.holdgrstatus = 'hold')";
								}
							}

						}
						var data = await pgsql.QueryAsync<OpenPoModel>(
						   query, null, commandType: CommandType.Text);
						if (data.Count() == 0)
						{
							if (pono.StartsWith("NP"))
							{
								OpenPoModel po = new OpenPoModel();
								po.pono = pono;
								po.inwmasterid = inwmasterid;
								po.invoiceno = invoiceno;
								datalist.Add(po);
								data = datalist;
							}
						}
						if (data.Count() > 0)
						{

							foreach (OpenPoModel po in data)
							{
								var fdata = datalist.Where(o => o.Material == po.Material && o.Materialdescription == po.Materialdescription && o.pono == po.pono).FirstOrDefault();
								if (fdata == null)
								{
									string querya = "select inw.pono,inw.materialid,Max(inw.materialqty) as materialqty,SUM(inw.confirmqty) as confirmqty from wms.wms_storeinward inw";
									querya += " where inw.pono = '" + po.pono + "' and inw.materialid = '" + po.Material + "'";
									querya += " group by inw.pono,inw.materialid";
									var datax = await pgsql.QueryAsync<OpenPoModel>(
									querya, null, commandType: CommandType.Text);
									if (datax.Count() > 0)
									{
										po.isreceivedpreviosly = true;
										int pendingqty = 0;
										if (datax.FirstOrDefault().confirmqty > 0)
										{
											pendingqty = po.materialqty - datax.FirstOrDefault().confirmqty;
										}
										else
										{
											pendingqty = po.materialqty - datax.FirstOrDefault().receivedqty;
										}

										if (pendingqty < 0)
										{
											po.pendingqty = 0;
										}
										else
										{
											po.pendingqty = pendingqty;
										}

									}
                                    else
                                    {
										po.pendingqty = po.materialqty;

									}
									datalist.Add(po);
								}

							}
						}
					}


					return datalist;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetDeatilsForthreeWaymatching", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<Getqualitydetails>>  Author :<<Ramesh kumar>>  
		Date of Creation <<07/07/2020>>
		Purpose : <<get list of info for quality check>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> Getqualitydetails(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getdataforqualitydetails.Replace("#grnno", grnnumber);// + pono+"'";//li
					var data = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getqualitydetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<VerifythreeWay>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<To verify three way match and generate GRN No>>
		<param name="invoiceno"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<OpenPoModel> VerifythreeWay(string inwmasterid, string invoiceno)
		{
			OpenPoModel verify = new OpenPoModel();
			sequencModel obj = new sequencModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string lastinsertedgrn = WMSResource.lastinsertedgrn;
					iwardmasterModel info = new iwardmasterModel();
					string query = "select grnnumber, pono from wms.wms_securityinward where inwmasterid = '" + inwmasterid + "'";

					//if (pono.StartsWith("NP"))
					//{
					//	query = "select grnnumber,pono from wms.wms_securityinward where invoiceno = '" + invoiceno + "' and pono = '" + pono + "' group by grnnumber,pono";

					//}
					//else
					//{
					//	query = WMSResource.Verifythreewaymatch.Replace("#pono", pono).Replace("#invoiceno", invoiceno);

					//}

					info = pgsql.QuerySingle<iwardmasterModel>(
					  query, null, commandType: CommandType.Text);
					if (info != null && info.grnnumber == null)
					{
						int grnnextsequence = 0;
						string grnnumber = string.Empty;
						obj = pgsql.QuerySingle<sequencModel>(
					   lastinsertedgrn, null, commandType: CommandType.Text);
						if (obj.id != 0)
						{
							grnnextsequence = (Convert.ToInt32(obj.sequencenumber) + 1);
							grnnumber = obj.sequenceid + "-" + obj.year + "-" + grnnextsequence.ToString().PadLeft(6, '0');
							string updategrnnumber = WMSResource.updategrnnumber.Replace("#inw", inwmasterid);
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(updategrnnumber, new
								{
									grnnumber,

								});
							}
							verify.grnnumber = grnnumber;
							int id = obj.id;
							string updateseqnumber = WMSResource.updateseqnumber;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(updateseqnumber, new
								{
									grnnextsequence,
									id,

								});
							}
						}




						else
						{

						}
						if (info.pono.Contains(","))
                        {
							string[] pos = info.pono.Split(",");
							foreach(string str in pos)
                            {
								string pono = str.Trim();
								string insertqueryforstatus = WMSResource.statusupdatebySecurity;
								using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
								{
									var results = DB.ExecuteScalar(insertqueryforstatus, new
									{
										pono

									});
								}

							}

                        }
                        else
                        {
							string insertqueryforstatus = WMSResource.statusupdatebySecurity;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(insertqueryforstatus, new
								{
									info.pono

								});
							}

						}
						
						//}
					}

					else
					{
						verify.grnnumber = info.grnnumber;
					}
					//if (inwardid != 0)
					
						EmailModel emailmodel = new EmailModel();
					//emailmodel.pono = datamodel[0].pono;
					//emailmodel.jobcode = datamodel[0].projectname;
					emailmodel.grnnumber = verify.grnnumber;

					//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
					emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
					//emailmodel.CC = "sushma.patil@in.yokogawa.com";
					EmailUtilities emailobj = new EmailUtilities();
					emailobj.sendEmail(emailmodel, 2,9);

					return verify;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "VerifythreeWay", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}
		}

		/*
		Name of Function : <<receivequantity>>  Author :<<Ramesh kumar>>  
		Date of Creation <<07/07/2020>>
		Purpose : <<to receive material>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<string> receivequantity(List<inwardModel> datamodel)
		{

			//inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;

				try
				{
					
					datamodel[0].receiveddate = System.DateTime.Now;
					await pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();

					//string query = WMSResource.getinwmasterid.Replace("#pono", datamodel[0].pono).Replace("#invoiceno", datamodel[0].invoiceno);
					//obj = pgsql.QuerySingle<inwardModel>(
					// query, null, commandType: CommandType.Text);
					//string getGRNno = WMSResource.getGRNNo.Replace("#pono", datamodel[0].pono);
					//getgrnnoforpo = pgsql.QueryFirstOrDefault<inwardModel>(
					//   getGRNno, null, commandType: CommandType.Text);
					int inwardid = 0;
					if (datamodel != null && datamodel.Count() > 0)
					{
						int loop = 0;
						bool isupdateprocess = false;

						string unholdedby = datamodel[0].unholdedby;
						if (unholdedby != null && unholdedby != "")
						{
							isupdateprocess = true;
						}
						foreach (var item in datamodel)
						{
							item.receiveddate = System.DateTime.Now;
							string insertforinvoicequery = WMSResource.receiveforinvoice;
							item.deleteflag = false;
							string materialid = item.Material;
							bool? qualitychecked = null;
							if (!item.qualitycheck)
							{
								qualitychecked = true;

							}

								if (!isupdateprocess)
								{
									var results = pgsql.ExecuteScalar(insertforinvoicequery, new
									{
										item.inwmasterid,
										item.receiveddate,
										item.receivedby,
										item.receivedqty,
										materialid,
										item.deleteflag,
										item.qualitycheck,
										qualitychecked,
										item.materialqty,
										item.receiveremarks,
										item.pono

									});
									inwardid = Convert.ToInt32(results);

									if (inwardid != 0 && loop == 0)
									{
										if (item.onhold)
										{
											string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where inwmasterid = '" + item.inwmasterid + "'";
											var results11 = pgsql.ExecuteScalar(qry);
										}

									}
									loop++;

								}
								else
								{
									string qrry = WMSResource.updatereceiptunhold.Replace("#inwardid", item.inwardid.ToString());
									var results = pgsql.ExecuteScalar(qrry, new
									{
										item.receiveddate,
										item.receivedby,
										item.receivedqty,
										item.qualitycheck,
										qualitychecked,
										item.receiveremarks

									});


									if (loop == 0)
									{
										if (item.onhold)
										{
											string qry = "Update wms.wms_securityinward set onhold = " + item.onhold + ",onholdremarks = '" + item.onholdremarks + "',holdgrstatus='hold' where inwmasterid = '" + item.inwmasterid + "'";
											var results11 = pgsql.ExecuteScalar(qry);
										}
									}
									loop++;
								}


								//if (inwardid != 0)
								//{
								//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

								//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
								//    {
								//        inwardid,
								//        datamodel.quality,
								//        datamodel.qtype,
								//        datamodel.qcdate,
								//        datamodel.qcby,
								//        datamodel.remarks,
								//        datamodel.deleteflag,

								//    });
								//string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

								//var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
								//{
								//	item.pono,
								//	item.returnqty

								//});

							}
						Trans.Commit();
					}
					
					return "Saved"+(Convert.ToString(inwardid));
				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString());
					return Ex.Message;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<insertquantity>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Insert data into store Inward >>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<string> insertquantity(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					datamodel[0].receiveddate = System.DateTime.Now;
					await pgsql.OpenAsync();

					string query = WMSResource.getinwmasterid.Replace("#pono", datamodel[0].pono).Replace("#invoiceno", datamodel[0].invoiceno);
					obj = pgsql.QuerySingle<inwardModel>(
					   query, null, commandType: CommandType.Text);
					string getGRNno = WMSResource.getGRNNo.Replace("#pono", datamodel[0].pono);
					getgrnnoforpo = pgsql.QueryFirstOrDefault<inwardModel>(
					   getGRNno, null, commandType: CommandType.Text);
					int inwardid = 0;
					if (obj.inwmasterid != null && obj.inwmasterid != "")
					{

						foreach (var item in datamodel)
						{
							string insertforinvoicequery = WMSResource.insertforinvoicequery;
							item.deleteflag = false;
							string materialid = item.Material;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								var results = DB.ExecuteScalar(insertforinvoicequery, new
								{
									obj.inwmasterid,
									//item.poitemid,
									item.receiveddate,
									item.receivedby,
									item.receivedqty,
									item.returnqty,
									item.confirmqty,
									materialid,
									item.deleteflag,

								});
								inwardid = Convert.ToInt32(results);
								//if (inwardid != 0)
								//{
								//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

								//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
								//    {
								//        inwardid,
								//        datamodel.quality,
								//        datamodel.qtype,
								//        datamodel.qcdate,
								//        datamodel.qcby,
								//        datamodel.remarks,
								//        datamodel.deleteflag,

								//    });
								string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

								var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
								{
									item.pono,
									item.returnqty

								});

							}
						}
					}

					//}
					return (Convert.ToString(inwardid));
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantity", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<InsertStock>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting material details to warehouse>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InsertStock(List<StockModel> data)
		{
			try
			{
				StockModel obj = new StockModel();
				string loactiontext = string.Empty;
				var result = 0;
				//int inwmasterid = 0;
				string inwmasterid = "";
				foreach (var item in data)
				{



					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						StockModel objs = new StockModel();
						pgsql.Open();
						string query = WMSResource.getinwardmasterid.Replace("#grnnumber", item.grnnumber);
						objs = pgsql.QueryFirstOrDefault<StockModel>(
						   query, null, commandType: CommandType.Text);
						if (objs != null)
							inwmasterid = objs.inwmasterid;
					}
					//foreach (var item in data) { 
					item.createddate = System.DateTime.Now;
					string insertquery = WMSResource.insertstock;
					int itemid = 0;
					//if (data.itemid == 0)
					//{
					string materialid = item.Material;
					item.availableqty = item.confirmqty;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							inwmasterid,
							item.pono,
							item.binid,
							item.rackid,
							item.storeid,
							item.vendorid,
							item.totalquantity,
							item.shelflife,
							item.availableqty,
							item.deleteflag,
							//data.itemreceivedfrom,
							item.itemlocation,
							item.createddate,
							item.createdby,
							item.stockstatus,
							materialid,
							item.inwardid,
							item.stocktype
						}));
						if (result != 0)
						{
							itemid = Convert.ToInt32(result);
							string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
							var results = DB.ExecuteScalar(insertqueryforlocationhistory, new
							{
								item.itemlocation,
								itemid,
								item.createddate,
								item.createdby,

							});
							string insertqueryforstatuswarehouse = WMSResource.insertqueryforstatuswarehouse;

							var data1 = DB.ExecuteScalar(insertqueryforstatuswarehouse, new
							{
								item.pono,

							});

							
						}
					}
					//}

					//else
					//{
					//	itemid = data.itemid;
					//	string updatequery = WMSResource.updatelocation.Replace("#itemlocation", data.itemlocation).Replace("#itemid", Convert.ToString(itemid));

					//	using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					//	{
					//		result = DB.Execute(updatequery, new
					//		{
					//			data.binid,
					//			data.rackid

					//		});
					//	}
					//}
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{


						pgsql.Open();

						string selectqueryforloaction = WMSResource.getlocationasresponse.Replace("#itemid", itemid.ToString());
						obj = pgsql.QuerySingle<StockModel>(
							   selectqueryforloaction, null, commandType: CommandType.Text);
						if (obj.binnumber != null)
						{
							loactiontext = obj.binnumber;
						}
						else if (obj.racknumber != null)
						{
							loactiontext = obj.racknumber;
						}
						else
						{
							loactiontext = "no data";
						}
					}
				}
				return (loactiontext);

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "InsertStock", Ex.StackTrace.ToString());
				return null;
			}


		}

		/*
		Name of Function : <<GetListItems>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<to get search data and pass  query dynamically>>
		<param name="Result"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public DataTable GetListItems(DynamicSearchResult Result)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.OpenAsync();
					DataTable dataTable = new DataTable();
					IDbCommand selectCommand = pgsql.CreateCommand();
					string query = "";
					if (Result.tableName == "wms.wms_project")
					{
						query = "select distinct projectcode,projectname from " + Result.tableName + Result.searchCondition + "";
					}
					if (Result.tableName == "wms.wms_stock")
					{
						query = "select distinct itemlocation from " + Result.tableName + Result.searchCondition + "";
					}
					else
					{
						query = "select * from " + Result.tableName + Result.searchCondition + "";
					}

					if (!string.IsNullOrEmpty(Result.query))
						query = Result.query;
					selectCommand.CommandText = query;
					IDbDataAdapter dbDataAdapter = new NpgsqlDataAdapter();
					dbDataAdapter.SelectCommand = selectCommand;

					DataSet dataSet = new DataSet();

					dbDataAdapter.Fill(dataSet);
					return dataTable = dataSet.Tables[0];
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetListItems", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<GetMaterialItems>>  Author :<<gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<GetMaterialItems>>
		<param name="Result"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public DataTable GetMaterialItems(DynamicSearchResult Result)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					pgsql.OpenAsync();
					DataTable dataTable = new DataTable();
					IDbCommand selectCommand = pgsql.CreateCommand();
					string query = "";
					query = "select Distinct(sk.materialid),ygs.unitprice as materialcost from " + Result.tableName + WMSResource.getgatepassunitprice + Result.searchCondition + "";
					if (!string.IsNullOrEmpty(Result.query))
						query = Result.query;

					selectCommand.CommandText = query;
					IDbDataAdapter dbDataAdapter = new NpgsqlDataAdapter();
					dbDataAdapter.SelectCommand = selectCommand;

					DataSet dataSet = new DataSet();

					dbDataAdapter.Fill(dataSet);
					return dataTable = dataSet.Tables[0];
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialItems", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<IssueRequest>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<material request by Project manager>>
		<param name="reqdata"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int IssueRequest(List<IssueRequestModel> reqdata)
		{
			int requestid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				IssueRequestModel obj = new IssueRequestModel();
				pgsql.Open();
				string query = WMSResource.getnextrequestid;
				obj = pgsql.QuerySingle<IssueRequestModel>(
				   query, null, commandType: CommandType.Text);
				//requestid = obj.requestid + 1;
			}
			try
			{

				var result = 0;
				foreach (var item in reqdata)
				{

					item.requesteddate = System.DateTime.Now;
					string insertquery = WMSResource.materialquest;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						result = DB.Execute(insertquery, new
						{
							// item.paitemid,
							item.quantity,
							item.requesteddate,
							item.approveremailid,
							item.approverid,
							item.pono,
							item.materialid,
							item.requesterid,
							item.requestedquantity,
							requestid,
							item.projectcode,
							item.remarks
						});
					}



				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<getitemdeatils>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on grnnumber will get lst of items>>
		<param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getitemdeatils(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.queryforitemdetails.Replace("#grnnumber", grnnumber);
					var data = await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<getitemdeatilsnotif>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on grnnumber will get lst of items for notification>>
		<param name="grnnumber"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getitemdeatilsnotif(string grnnumber)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();
					string queryforitemdetails = WMSResource.getitemsfornotifypage.Replace("#grnnumber", grnnumber);
					return await pgsql.QueryAsync<inwardModel>(
					   queryforitemdetails, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getitemdeatils", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
				//throw new NotImplementedException();
			}

		}

		/*
		Name of Function : <<MaterialRequest_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<requesting for material>>
		<param name="pono"></param>
		 <param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequest_old(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.materialrequestquery.Replace("#reqid", approverid);
				//if (pono != null)
				//{
				//	materialrequestquery = materialrequestquery + " where openpo.pono = '" + pono + "'";
				//}
				if (approverid != null)
				{
					materialrequestquery = materialrequestquery + " where  req.requesterid = '" + approverid + "' ";
				}
				materialrequestquery = materialrequestquery + " group by req.requestid order by req.requestid desc limit 50";
				try
				{
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialIssue", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialRequest>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<requesting for material>>
		<param name="approverid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialTransaction>> MaterialRequest(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getmaterialrequests.Replace("#reqid", approverid);
				
				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach(MaterialTransaction trans in data)
                    {
						trans.materialdata = new List<MaterialTransactionDetail>();
						string materialrequestdataquery = WMSResource.getmaterialrequestdata.Replace("#requestid", trans.requestid);
						var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialIssue", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<acknowledgeMaterialReceived>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<acknowledge for received item from Project manager>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int acknowledgeMaterialReceived(List<IssueRequestModel> dataobj)
		{

			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string ackstatus = string.Empty;
					if (item.status == true)
					{
						ackstatus = "received";
					}
					else if (item.status == false)
					{
						ackstatus = "not received";
					}
					DateTime approveddate = System.DateTime.Now;

					int requestforissueid = item.requestforissueid;
					string requestid = item.requestid;
					string ackremarks = item.ackremarks;
					int issuedquantity = item.issuedquantity;
					string updateackstatus = WMSResource.updateackstatus;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateackstatus, new
						{
							ackstatus,
							ackremarks,
							requestid,

						});
					}
					if (result != 0)
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = item.pono;
						emailmodel.requestid = item.requestid;
						//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
						//emailmodel.CC = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 6,3);
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString());
				return 0;
			}
			//try
			//{
			//    //data.createddate = System.DateTime.Now;
			//    string insertquery = "update  wms.wms_inward set ackstatus='item received',ackremarks=@remarks where inwardid=@inwardid and materialid=@material";
			//    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			//    {
			//        var result = DB.ExecuteScalar(insertquery, new
			//        {
			//            remarks,
			//            inwardid,
			//            material,
			//        });

			//        return (Convert.ToInt32(result));


			//    }
			//}
			//catch (Exception Ex)
			//{
			//    log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceived", Ex.StackTrace.ToString());
			//    return 0;
			//}

		}

		/*
		Name of Function : <<GetMaterialissueList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material issue list>>
		<param name="requesterid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueList(string requesterid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetListForMaterialRequestByrequesterid.Replace("#requesterid", requesterid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialissueListforapprover_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material details based on approver id>>
		<param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover_old(string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetListForMaterialRequestByapproverid.Replace("#approverid", approverid);

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialissueListforapprover>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material details based on approver id>>
		<param name="approverid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetMaterialissueListforapprover(string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getmaterialissuelist;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.requestid);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByrequestid_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of materail details based on particlular requestid>>
		<param name="requestid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid_old(string requestid, string pono)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "";
					if (string.IsNullOrEmpty(pono) || pono == "NULL" || pono == null || pono.Trim() == "")
						query = WMSResource.GetdetailsByrequestidWithoutPO.Replace("#requestid", requestid);
					else
						query = WMSResource.GetdetailsByrequestidWithPO.Replace("#requestid", requestid).Replace("#pono", pono);


					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByrequestid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << get list of materail details based on particlular requestid>>
		<param name="requestid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetmaterialdetailsByrequestid(string requestid, string pono)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "";
					if (string.IsNullOrEmpty(pono) || pono == "NULL" || pono == null || pono.Trim() == "")
						query = WMSResource.getrequestdetailswithoutpo.Replace("#requestid", requestid);
                    else
                    {
						query = WMSResource.getrequestdetailswithoutpo.Replace("#requestid", requestid);
						query += " and rq.pono = '" + pono + "'";
					}
						


					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetRequestList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetPonodetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of pono data>>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetPonodetails(string pono)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.Getponodetailsformaterialissue.Replace("#pono", pono);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetPonodetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updaterequestedqty_old>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting or updating requested qty by PM>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updaterequestedqty_old(List<IssueRequestModel> dataobj)
		{

			int requestid = 1;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				IssueRequestModel obj = new IssueRequestModel();
				pgsql.Open();
				string query = WMSResource.getnextrequestid;
				obj = pgsql.QueryFirstOrDefault<IssueRequestModel>(
				   query, null, commandType: CommandType.Text);
				if (obj != null)
                {

                }
					//requestid = obj.requestid + 1;
			}
			try
			{

				var result = 0;
				foreach (var item in dataobj)
				{
					if (item.quantity > 0)
					{
						int qty = 0;
						item.requesteddate = System.DateTime.Now;
						string insertquery = WMSResource.materialquest;
						string materialid = item.material;
						item.requestedquantity = item.quantity;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							result = DB.Execute(insertquery, new
							{
								//item.paitemid,
								item.quantity,
								item.requesteddate,
								item.approveremailid,
								item.approverid,
								item.pono,
								item.material,
								materialid,
								item.createddate,
								item.createdby,
								item.requesterid,
								requestid,
								item.requestedquantity,
								item.projectcode,
								item.remarks
							});
						}
						if (result != 0)
						{
							EmailModel emailmodel = new EmailModel();
							emailmodel.pono = item.pono;
							emailmodel.jobcode = item.projectname;
							emailmodel.material = item.material;
							emailmodel.createdby = item.requesterid;
							emailmodel.createddate = item.requesteddate;


							//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							//emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel,4,3);
						}
						//if (result != 0)
						//{
						//	int availableqty = item.availableqty - item.requestedquantity;
						//	string updatequery = WMSResource.updatestock.Replace("#availableqty", Convert.ToString(availableqty)).Replace("#itemid", Convert.ToString(item.itemid));
						//	using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						//	{
						//		result = DB.Execute(updatequery, new
						//		{
						//		});
						//	}
						//}

					}

				}
				return (Convert.ToInt32(requestid));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "IssueRequest", Ex.StackTrace.ToString());
				return 0;
			}

			
		}

		/*
		Name of Function : <<updaterequestedqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting or updating requested qty by PM>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updaterequestedqty(List<IssueRequestModel> dataobj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				NpgsqlTransaction Trans = null;
				
                try
                {
					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string calltype = dataobj[0].calltype;
					string rsvid = dataobj[0].reserveid;
					MaterialTransaction mainmodel = new MaterialTransaction();
					mainmodel.pono = dataobj[0].pono;
					mainmodel.projectcode = dataobj[0].projectcode;
					mainmodel.approveremailid = dataobj[0].approveremailid;
					mainmodel.approverid = dataobj[0].approverid;
					mainmodel.remarks = dataobj[0].remarks;
					mainmodel.requesterid = dataobj[0].requesterid;
					mainmodel.requesteddate = System.DateTime.Now;
					string insertmatquery = WMSResource.insertmaterialrequest;
					string materials = "";
					var result = pgsql.ExecuteScalar(insertmatquery, new
					{

						mainmodel.approveremailid,
						mainmodel.approverid,
						mainmodel.pono,
						mainmodel.requesterid,
						mainmodel.projectcode,
						mainmodel.remarks
					});
					if(result != null)
                    {
						var stindex = 0;
						foreach (var item in dataobj)
						{
							if(stindex > 0)
                            {
								materials += ", ";

							}
							materials += item.material;
							MaterialTransactionDetail detail = new MaterialTransactionDetail();
							detail.id = Guid.NewGuid().ToString();
							detail.requestid = result.ToString();
							detail.materialid = item.material;
							detail.requestedquantity = item.quantity;
							string insertdataqry = WMSResource.insertmaterialrequestdetails;
							var result1 = pgsql.Execute(insertdataqry, new
							{

								detail.id,
								detail.requestid,
								detail.materialid,
								detail.requestedquantity

							});

							stindex++;


						}
						if(calltype == "fromreserve")
                        {
							string query1 = "update wms.materialrequest set reserveid = '" + rsvid + "' where requestid = '" + result.ToString() + "'";
							var results111 = pgsql.ExecuteScalar(query1);
						}

                        EmailModel emailmodel = new EmailModel();
                        emailmodel.pono = dataobj[0].pono;
                        emailmodel.jobcode = dataobj[0].projectcode;
                        emailmodel.material = materials;
                        emailmodel.createdby = dataobj[0].requesterid;
                        emailmodel.createddate = DateTime.Now;


                        //emailmodel.ToEmailId = "developer1@in.yokogawa.com";
                        emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
                        //emailmodel.CC = "sushma.patil@in.yokogawa.com";
                        EmailUtilities emailobj = new EmailUtilities();
                        emailobj.sendEmail(emailmodel, 4,3);
                        Trans.Commit();
						return 1;

					}
					else
                    {
						Trans.Rollback();
						return 0;
					}

					


				}
				catch(Exception Ex)
                {
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "MaterialRequest", Ex.StackTrace.ToString());
					return 0;
				}
                finally
                {
					pgsql.Close();
                }
				
			}
			
		}

		/*
		Name of Function : <<ApproveMaterialissue>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<issued matreial list >>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int ApproveMaterialissue(List<IssueRequestModel> dataobj)
		{
			NpgsqlTransaction Trans = null;
			try
			{
				var result = 0;
				
				using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					//data.createddate = System.DateTime.Now;
					DB.Open();
					Trans = DB.BeginTransaction();
					foreach (var item in dataobj)
					{
						var createdate = Convert.ToDateTime(item.createddate).ToString("yyyy-MM-dd");
						string stockquery = "select * from wms.wms_stock where materialid = '" + item.materialid + "' and availableqty > 0 and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' order by itemid";
						var stockdata =  DB.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
						if(stockdata != null)
                        {
							int quantitytoissue = item.issuedqty;
							int issuedqty = 0;
							foreach(StockModel itm in stockdata.Result)
                            {
								string approvedstatus = string.Empty;
								if (item.issuedqty != 0)
								{
									approvedstatus = "Approved";
								}
								DateTime approvedon = System.DateTime.Now;

								int requestforissueid = item.requestforissueid;
								string requestmaterialid = item.requestmaterialid;
								string materialid = item.materialid;
								if(quantitytoissue <= itm.availableqty)
                                {
									issuedqty = quantitytoissue;
								}
                                else
                                {
									issuedqty = itm.availableqty;
								}
								
								quantitytoissue = quantitytoissue - issuedqty;
								
								DateTime itemissueddate = System.DateTime.Now;

								string updateapproverstatus = WMSResource.updateapproverstatus;


								if (item.issuedqty > 0)
								{
									result = DB.Execute(updateapproverstatus, new
									{
										approvedstatus,
										requestmaterialid,
										approvedon,
										issuedqty,
										materialid,
										itm.pono,
										itm.itemid,
										item.itemreturnable,
										item.approvedby,
										itemissueddate,
										item.itemreceiverid,
										itm.itemlocation

									});
									int availableqty = itm.availableqty - item.issuedqty;

									string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));

									var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
									{

									});


								}

								if (quantitytoissue <= 0)
								{
									break;
								}


							}
						}
						

					}
					string requestid = dataobj[0].requestid;
					string approvedby = dataobj[0].approvedby;
					string updaterequest = "update wms.materialrequest set issuedby = '" + approvedby + "',issuedon=current_date where requestid='" + requestid + "'";

					var data2 = DB.ExecuteScalar(updaterequest, new
					{

					});
					Trans.Commit();
				}
				
				EmailModel emailmodel = new EmailModel();
				//emailmodel.pono = datamodel[0].pono;
				//emailmodel.jobcode = datamodel[0].projectname;
				emailmodel.materialissueid = dataobj[0].materialissueid;
				emailmodel.requestid =dataobj[0].requestid;
				//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
				emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
				//emailmodel.CC = "sushma.patil@in.yokogawa.com";
				EmailUtilities emailobj = new EmailUtilities();
				emailobj.sendEmail(emailmodel, 5,11);

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<GetgatepassList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of gatepass data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassModel>> GetgatepassList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getgatepasslist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetgatepassList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<NonreturnGetgatepassList>>  Author :<<Ramesh>>  
		Date of Creation <<23/07/2020>>
		Purpose : <<non returnable gatepass for outward entry>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<gatepassModel>> NonreturnGetgatepassList(string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getnonreturnablegatepassdata;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);
					if (type == "1")
					{

						var data1 = data.Where(o => o.gatepasstype == "Returnable").ToList();
						return data1;

					}
					else
					{
						var data1 = data.Where(o => o.gatepasstype == "Non Returnable").ToList();
						return data1;
					}



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "NonreturnGetgatepassList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<outingatepassreport>>  Author :<<Ramesh>>  
		Date of Creation <<23/07/2020>>
		Purpose : <<Report (out)>>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<outwardinwardreportModel>> outingatepassreport()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.outinreportquery;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<outwardinwardreportModel>(
					   query, null, commandType: CommandType.Text);

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "outingatepassreport", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<SaveOrUpdateGatepassDetails>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert or update gatepass info>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int SaveOrUpdateGatepassDetails(gatepassModel dataobj)
		{
			NpgsqlTransaction Trans = null;

			try
			{
				//foreach(var item in dataobj._list)
				//{
				string status = "Pending";
				string remarks = dataobj.statusremarks;
				EmailModel emailmodel = new EmailModel();

				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
                {
					pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();
					if (dataobj.gatepassid == "" || dataobj.gatepassid == null)
					{
						dataobj.requestedon = System.DateTime.Now;
						string insertquery = WMSResource.insertgatepassdata;
						string fmapprovedstatus = "";
						string approverstatus = "Pending";

						string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
						dataobj.deleteflag = false;
						dataobj.fmapproverid = null;
						if (dataobj.gatepasstype == "Non Returnable")
						{
							dataobj.fmapproverid = "400104";
							dataobj.fmapprovedstatus = "Pending";

						}
							dataobj.status = status;
							dataobj.approverstatus = approverstatus;
							var gatepassid = pgsql.ExecuteScalar(insertquery, new
							{

								dataobj.gatepasstype,
								dataobj.status,
								dataobj.requestedon,
								dataobj.referenceno,
								dataobj.vehicleno,
								dataobj.requestedby,
								dataobj.deleteflag,
								dataobj.vendorname,
								dataobj.reasonforgatepass,
								dataobj.approverid,
								dataobj.fmapproverid,
								dataobj.requestid,
								dataobj.fmapprovedstatus,
								dataobj.approverstatus,
								remarks

							});
							dataobj.gatepassid = gatepassid.ToString();
							if (dataobj.gatepasstype == "Returnable")
							{
								string approvername = dataobj.managername;
								int label = 1;
								//string approverstatus = "Pending";
								var gatepasshistory = pgsql.ExecuteScalar(insertgatepasshistory, new
								{

									dataobj.approverid,
									approvername,
									gatepassid,
									label,
									approverstatus
								});
								
								emailmodel.pono = dataobj.pono;
								emailmodel.requestid = dataobj.requestid;
								emailmodel.gatepassid = dataobj.gatepassid;
								emailmodel.gatepasstype = dataobj.gatepasstype;

								emailmodel.requestedon = dataobj.requestedon;
								emailmodel.requestedby = dataobj.requestedby;

								//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
								emailmodel.FrmEmailId = "developer1@in.yokogawa.com";
								//emailmodel.CC = "sushma.patil@in.yokogawa.com";
								
							}
							else if (dataobj.gatepasstype == "Non Returnable")
							{
								//string updategatepasshistoryfornonreturn = WMSResource.updategatepasshistoryfornonreturn;
								{
									string approvername = dataobj.managername;
									int label = 1;
									//string approverstatus = "Pending";
									var gatepasshistory = pgsql.ExecuteScalar(insertgatepasshistory, new
									{

										dataobj.approverid,
										approvername,
										gatepassid,
										label,
										approverstatus,
									});
									string approverid = "400104";
									approvername = "Lakshmi Prasanna";
									label = 2;
									var gatepassdata = pgsql.ExecuteScalar(insertgatepasshistory, new
									{

										approverid,
										gatepassid,
										label,
										approverstatus,
										approvername
									});
								}
								
								emailmodel.pono = dataobj.pono;
								emailmodel.requestid = dataobj.requestid;
								emailmodel.gatepassid = dataobj.gatepassid;
								emailmodel.gatepasstype = dataobj.gatepasstype;
								emailmodel.requestedon = dataobj.requestedon;
								emailmodel.requestedby = dataobj.requestedby;
								//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
								emailmodel.FrmEmailId = "developer1@in.yokogawa.com";
								//emailmodel.CC = "sushma.patil@in.yokogawa.com";
								
							}


							if (dataobj.gatepassid == "" || dataobj.gatepassid == null)
								dataobj.gatepassid = gatepassid.ToString();
						
					}
					else
					{
						dataobj.requestedon = System.DateTime.Now;
						string insertquery = WMSResource.updategatepass.Replace("#gatepassid", Convert.ToString(dataobj.gatepassid));

						
							var result = pgsql.ExecuteScalar(insertquery, new
							{

								dataobj.gatepasstype,
								dataobj.status,
								dataobj.requestedon,
								dataobj.referenceno,
								dataobj.vehicleno,
								dataobj.requestedby,
								dataobj.vendorname,
								dataobj.reasonforgatepass,
								remarks

							});
						
					}
					foreach (var item in dataobj.materialList)
					{
						int itemid = 0;
						
							//string materialrequestquery = "select itemid from wms.wms_materialissue where gatepassmaterialid=" + item.gatepassmaterialid;
							//gatepassModel gatemodel = new gatepassModel();
							//gatemodel = pgsql.QueryFirstOrDefault<gatepassModel>(
							//		  materialrequestquery, null, commandType: CommandType.Text);
							//if (gatemodel != null)
							//	itemid = gatemodel.itemid;
					


						
							if (item.gatepassmaterialid == 0)
							{
								string insertquerymaterial = WMSResource.insertgatepassmaterial;
								dataobj.deleteflag = false;
								var results = pgsql.ExecuteScalar(insertquerymaterial, new
								{

									dataobj.gatepassid,
									item.materialid,
									item.quantity,
									dataobj.deleteflag,
									item.remarks,
									item.materialcost,
									item.expecteddate,
									//item.returneddate,
									item.issuedqty
								});

							}
							else
							{
								//string updatestockquery = "update wms.wms_stock set availableqty=availableqty+" + item.quantity + " where itemid=" + itemid;

								//var result1 = DB.ExecuteScalar(updatestockquery, new
								//{
								//});

								string updatequery = WMSResource.updategatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(item.gatepassmaterialid));

								var result = pgsql.ExecuteScalar(updatequery, new
								{

									dataobj.gatepassid,
									item.materialid,
									item.quantity,
									item.remarks,
									item.materialcost,
									item.expecteddate,
									item.returneddate,

								});


							}
						
					}
					Trans.Commit();
					EmailUtilities emailobj = new EmailUtilities();
					emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
					emailobj.sendEmail(emailmodel, 8,8);
					
				}

				
				return (1);
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "SaveOrUpdateGatepassDetails", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<checkmaterialandqty>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<check material in stock>>
		<param name="material"></param>
		 <param name="qty"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string checkmaterialandqty(string material = null, int qty = 0)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				materialistModel obj = new materialistModel();
				string returnvalue = string.Empty;
				try
				{
					pgsql.Open();
					if (material != null && qty == 0)
					{
						string query = WMSResource.checkmaterial.Replace("#materialid", material);
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
						   query, null, commandType: CommandType.Text);
						if (obj == null)
						{
							returnvalue = "material does not exists";
						}
						else
						{
							returnvalue = "true";
						}
					}
					else if (qty != 0 && material == null)
					{
						string query = WMSResource.checkqty.Replace("#availableqty", Convert.ToString(qty));
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
					query, null, commandType: CommandType.Text);
						if (obj == null)
						{
							returnvalue = "qty not available";
						}
						else
						{
							returnvalue = "true";
						}
					}
					else if (material != null && qty != 0)
					{
						//string query = WMSResource.checkmaterialandqty.Replace("#availableqty", Convert.ToString(qty)).Replace("#materialid", material);
						string query = WMSResource.checkmaterialandqty.Replace("#materialid", material);
						obj = pgsql.QueryFirstOrDefault<materialistModel>(
					query, null, commandType: CommandType.Text);
						if (obj != null)
						{
							if (obj.availableqty >= qty)
							{
								returnvalue = "true";
							}
							else
							{
								if (obj.availableqty < qty)
								{
									//returnvalue = "Material and quantity does not exists for " + material + " available qty is " + obj.availableqty;
									returnvalue = "Available quantity for " + material + "  is " + obj.availableqty;
								}
							}

						}
						else
						{
							returnvalue = "false";
						}
					}
					return returnvalue;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkmaterialandqty", Ex.StackTrace.ToString());
					return "false";
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<deletegatepassmaterial>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<delete gatepass>>
		<param name="gatepassmaterialid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int deletegatepassmaterial(int gatepassmaterialid)
		{
			int returndata = 0;
			try
			{
				string insertquery = WMSResource.deletegatepassmaterial.Replace("#gatepassmaterialid", Convert.ToString(gatepassmaterialid));
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data = DB.Execute(insertquery, new

					{


					});
					returndata = Convert.ToInt32(data);
				}
				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "deletegatepassmaterial", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<updategatepassapproverstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update gatepass approver info>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updategatepassapproverstatus_old(List<gatepassModel> model)
		{
			int returndata = 0;
			try
			{
				foreach (var item in model)
				{
					gatepassModel gatemodel = new gatepassModel();
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{


						string query = "select * from wms.wms_stock where materialid='" + item.materialid + "'and availableqty>0 order by createddate asc";

						pgsql.OpenAsync();
						gatemodel = pgsql.QueryFirstOrDefault<gatepassModel>(
						  query, null, commandType: CommandType.Text);
					}
					string updateapproverstatus = WMSResource.updategatepassmaterialissue;
					string approvedstatus = item.approverstatus;
					item.itemissueddate = System.DateTime.Now;
					item.approvedon = System.DateTime.Now;
					Boolean itemreturnable = false;
					if (item.gatepasstype == "Returnable")
					{
						itemreturnable = true;
					}

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result = DB.Execute(updateapproverstatus, new
						{
							approvedstatus,
							item.gatepassmaterialid,
							item.approvedon,
							item.issuedqty,
							item.materialid,
							item.pono,
							item.itemid,
							itemreturnable,
							item.approvedby,
							item.itemissueddate,
							item.itemreceiverid,

						});
					}
					string updateissueqty = "update  wms.wms_gatepassmaterial set issuedqty=" + item.issuedqty + " where gatepassmaterialid=" + item.gatepassmaterialid;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var result = DB.Execute(updateissueqty, new
						{

						});
					}
					string updatestockavailable = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var result = DB.Execute(updatestockavailable, new
						{

						});
					}


				}
				model[0].approvedon = System.DateTime.Now;
				model[0].status = "Issued";
				string insertquery = WMSResource.updategatepassapproverstatus.Replace("#gatepassid", Convert.ToString(model[0].gatepassid));
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data = DB.Execute(insertquery, new

					{
						model[0].status,
						model[0].approverremarks,
						model[0].approverstatus,
						model[0].approvedon,

					});
					returndata = Convert.ToInt32(data);
				}

				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
			Name of Function : <<updategatepassapproverstatus>>  Author :<<Ramesh>>  
			Date of Creation <<12-12-2019>>
			Purpose : <<update gatepass approver info>>
			<param name="model"></param>
			Review Date :<<>>   Reviewed By :<<>>
			*/
		public int updategatepassapproverstatus(List<gatepassModel> model)
		{
			int returndata = 0;
			NpgsqlTransaction Trans = null;
			try
			{
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{

					pgsql.OpenAsync();
					Trans = pgsql.BeginTransaction();
					foreach (var item in model)
					{

						var createdate = Convert.ToDateTime(item.createddate).ToString("yyyy-MM-dd");
						string stockquery = "select * from wms.wms_stock where materialid = '" + item.materialid + "' and availableqty > 0 and itemlocation = '" + item.itemlocation + "' and createddate::DATE = '" + createdate + "' order by itemid";
						var stockdata = pgsql.QueryAsync<StockModel>(stockquery, null, commandType: CommandType.Text);
						if (stockdata != null)
						{
							int quantitytoissue = item.issuedqty;
							int issuedqty = 0;
							int totalissued = 0;
							foreach (StockModel itm in stockdata.Result)
							{
								if (quantitytoissue <= itm.availableqty)
								{
									issuedqty = quantitytoissue;
								}
								else
								{
									issuedqty = itm.availableqty;
								}

								quantitytoissue = quantitytoissue - issuedqty;
								totalissued = totalissued + issuedqty;
								string updateapproverstatus = WMSResource.updategatepassmaterialissue;
								string approvedstatus = item.approverstatus;
								item.itemissueddate = System.DateTime.Now;
								item.approvedon = System.DateTime.Now;
								Boolean itemreturnable = false;
								if (item.gatepasstype == "Returnable")
								{
									itemreturnable = true;
								}

								var result1 = pgsql.Execute(updateapproverstatus, new
								{
									approvedstatus,
									item.gatepassmaterialid,
									item.approvedon,
									issuedqty,
									item.materialid,
									item.pono,
									itm.itemid,
									itemreturnable,
									item.approvedby,
									item.itemissueddate,
									item.itemreceiverid,

								});

								string updateissueqty = "update  wms.wms_gatepassmaterial set issuedqty=" + totalissued + " where gatepassmaterialid=" + item.gatepassmaterialid;
								var result2 = pgsql.Execute(updateissueqty);
								string updatestockavailable = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));
								var result3 = pgsql.Execute(updatestockavailable);

								if (quantitytoissue <= 0)
								{
									break;
								}


							}

						}
						else
						{
							Trans.Rollback();
							log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", "No material in stock");
							return 0;
						}
						model[0].approvedon = System.DateTime.Now;
						model[0].status = "Issued";
						string insertquery = WMSResource.updategatepassapproverstatus.Replace("#gatepassid", Convert.ToString(model[0].gatepassid));
						var data = pgsql.Execute(insertquery, new
						{
							model[0].status,
							model[0].approverremarks,
							model[0].approverstatus,
							model[0].approvedon,

						});
						returndata = Convert.ToInt32(data);

						
					}
					Trans.Commit();
					return returndata;

				}
			}
			catch (Exception Ex)
			{
				Trans.Rollback();
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString());
				return 0;
			}
		}
		/// <summary>
		/// get list of material based on gatepassid
		/// </summary>
		/// <param name="gatepassid"></param>
		/// <returns></returns>

		/*
		Name of Function : <<GetmaterialList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of material based on gatepassid>>
		<param name="gatepassid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassModel>> GetmaterialList(string gatepassid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getgatepassmaterialdetailList.Replace("#gatepassid", Convert.ToString(gatepassid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetmaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/*
		Name of Function : <<getGatePassApprovalHistoryList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get gatepass approval history based on gatepassid>>
		<param name="gatepassid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassapprovalsModel>> getGatePassApprovalHistoryList(string gatepassid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getGatePassApprovalHistoryList.Replace("#gatepassid", Convert.ToString(gatepassid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassapprovalsModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getGatePassApprovalHistoryList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateprintstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<updating print status>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updateprintstatus(gatepassModel model)
		{
			{
				int returndata = 0;
				int data = 0;
				try
				{
					gatepassModel obj = new gatepassModel();

					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						string query = WMSResource.getprintdetails.Replace("#gatepassid", Convert.ToString(model.gatepassid));

						pgsql.Open();
						obj = pgsql.QueryFirstOrDefault<gatepassModel>(
						   query, null, commandType: CommandType.Text);
					}
					if (obj.print == true)
					{
						string insertquery = WMSResource.printstatusupdate.Replace("#gatepassid", Convert.ToString(model.gatepassid));
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var data1 = DB.Execute(insertquery, new

							{
								model.printedby,
							});
							returndata = Convert.ToInt32(data);
						}
					}
					else
					{
						using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
						{
							string query = WMSResource.checkreprintalreadydone;

							query = query + " gatepassid=" + model.gatepassid + " order by reprintcount desc limit 1";

							pgsql.Open();
							obj = pgsql.QueryFirstOrDefault<gatepassModel>(
							   query, null, commandType: CommandType.Text);


						}
						string insertquery = WMSResource.insertreprintcount;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{

							if (obj == null)
							{ model.reprintcount = 1; }
							else
							{
								model.reprintcount = model.reprintcount + 1;
							}
							data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

							{

								model.gatepassid,
								model.reprintedby,
								model.reprintcount,

							}));
							returndata = Convert.ToInt32(data);
						}


						string updatequery = WMSResource.updatereprintcount.Replace("#reprinthistoryid", Convert.ToString(data));

						model.reprintcount = model.reprintcount + 1;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var data1 = DB.Execute(updatequery, new
							{
								model.reprintcount,
							});
							returndata = Convert.ToInt32(data);
						}
					}
					return returndata;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString());
					return 0;
				}
			}
		}
		//int returndata = 0;
		//try
		//{

		//    string insertquery = WMSResource.printstatusupdate.Replace("#gatepassid", Convert.ToString(model.gatepassid));
		//    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
		//    {
		//        var data = DB.Execute(insertquery, new

		//        {  
		//            model.printedby,
		//        });
		//        returndata = Convert.ToInt32(data);
		//    }
		//    return returndata;

		//}
		//catch (Exception Ex)
		//{
		//    log.ErrorMessage("PODataProvider", "updateprintstatus", Ex.StackTrace.ToString());
		//    return 0;
		//}
		//}

		/*
		Name of Function : <<updatereprintstatus>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<updating reprint status >>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updatereprintstatus(reprintModel model)
		{
			reprintModel obj = new reprintModel();
			reprintModel secondobj = new reprintModel();
			int returndata = 0;
			try
			{
				int data = 0;
				model.inwmasterid = (model.inwmasterid == null) ? null : model.inwmasterid;
				model.gatepassid = (model.gatepassid == null) ? null : model.gatepassid;
				string insertquery = WMSResource.insertreprintcount;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

					{
						model.inwmasterid,
						model.gatepassid,
						model.reprintcount,
						model.reprintedby,


					}));
					returndata = Convert.ToInt32(data);
				}
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					string query = WMSResource.checkreprintalreadydone;
					if (model.inwmasterid !=null && model.inwmasterid != "")
					{
						query = query + " inwmasterid= '" + model.inwmasterid + "' order by reprintcount desc limit 1";
					}
					else if (model.gatepassid != null)
					{
						query = query + " gatepassid=" + model.gatepassid + " order by reprintcount desc limit 1";
					}
					pgsql.Open();
					obj = pgsql.QuerySingle<reprintModel>(
					   query, null, commandType: CommandType.Text);
				}
				string updatequery = WMSResource.updatereprintcount.Replace("#reprinthistoryid", Convert.ToString(data));
				int reprintcount = obj.reprintcount + 1;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var data1 = DB.Execute(updatequery, new
					{
						reprintcount
					});
					returndata = Convert.ToInt32(data);
				}
				return returndata;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updategatepassapproverstatus", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<GetreportBasedCategory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list based on ABC category>>
		<param name="categoryid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReportModel>> GetreportBasedCategory(int categoryid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getcategorylist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetreportBasedCategory", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GetreportBasedMaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list based on materail in category>>
		<param name="materailid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReportModel>> GetreportBasedMaterial(string materailid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getcategorylistbymaterailid.Replace("#materialid", Convert.ToString(materailid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetreportBasedMaterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateABCcategorydata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update ABC categorydata>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updateABCcategorydata(List<ABCCategoryModel> model)
		{
			int returndata = 0;
			try
			{

				int data = 0;

				foreach (var item in model)
				{
					if (item.categoryid != 0)
					{
						string updatequery = WMSResource.updateABCrange.Replace("#categoryid", item.categoryid.ToString());
						item.updatedon = System.DateTime.Now;

						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							data = Convert.ToInt32(DB.Execute(updatequery, new

							{

								item.updatedby,
								item.updatedon,

							}));
							returndata = Convert.ToInt32(data);

						}
						//break;
					}
				}

				//insert ABC category data
				foreach (var item in model)
				{
					if (item.categoryname == "C")
						item.minpricevalue = 0;
					//item.startdate = item.startdate.AddDays(1);
					//item.enddate = item.enddate.AddDays(1);
					string insertquery = WMSResource.insertABCrange;
					//item.createdon = System.DateTime.Now;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new

						{
							item.categoryname,
							item.minpricevalue,
							item.maxpricevalue,
							item.createdby,
							item.startdate,
							item.enddate
						}));
						returndata = Convert.ToInt32(data);
					}
				}

				List<ReportModel> obj = new List<ReportModel>();
				//using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				//{


				//        // string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
				//        string query = WMSResource.getcategorylist;

				//    await pgsql.OpenAsync();
				//    return await pgsql.QueryAsync<ReportModel>(
				//       query, null, commandType: CommandType.Text);



				//}
				return returndata;
			}

			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updateABCcategorydata", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<GetABCCategorydata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC Category data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ABCCategoryModel>> GetABCCategorydata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					// string query = WMSResource.getcategorylist.Replace("#categoryid", Convert.ToString(categoryid));
					string query = WMSResource.getabccategorydata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ABCCategoryModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCCategorydata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetABCavailableqtyList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC available qty List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReportModel>> GetABCavailableqtyList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetallavlqtyABCList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCavailableqtyList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetEnquirydata>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns Enquiry Details>>
		<param name="materialid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<Enquirydata> GetEnquirydata(string materialid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = "select stock.materialid, SUM(stock.availableqty) as availableqty, Max(op.materialdescription) as materialdescription from wms.wms_stock stock left outer join wms.\"MaterialMasterYGS\" op on  stock.materialid =op.material  where stock.materialid='" + materialid + "' group by stock.materialid";

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<Enquirydata>(
					   query, null, commandType: CommandType.Text);
					return data.FirstOrDefault();
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetEnquirydata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetCyclecountList>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns all  Materials to count>>
		<param name="limita"></param>
		 <param name="limitb"></param>
		 <param name="limitc"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<CycleCountList>> GetCyclecountList(int limita, int limitb, int limitc)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();

					//Ramesh (08/06/2020) returns category A/B/C configuration

					string QueryABC = "select * from wms.wms_rd_category where deleteflag is false";
					var abcdata = await pgsql.QueryAsync<ABCCategoryModel>(QueryABC, null, commandType: CommandType.Text);
					foreach (ABCCategoryModel dt in abcdata)
					{
						if (dt.categoryname == "A")
						{
							config.amin = Convert.ToInt32(dt.minpricevalue);
							config.amax = Convert.ToInt32(dt.maxpricevalue);
						}
						else if (dt.categoryname == "B")
						{
							config.bmin = Convert.ToInt32(dt.minpricevalue);
							config.bmax = Convert.ToInt32(dt.maxpricevalue);
						}
						else if (dt.categoryname == "C")
						{
							config.cmin = Convert.ToInt32(dt.minpricevalue);
							config.cmax = Convert.ToInt32(dt.maxpricevalue);
						}
						config.startdate = dt.startdate;
						config.enddate = dt.enddate;

					}

					//Ramesh (08/06/2020) returns today counted list 
					string Querychecktodaycounted = "Select * from wms.cyclecount where counted_on = current_date";
					var todaycountdata = await pgsql.QueryAsync<CycleCountList>(Querychecktodaycounted, null, commandType: CommandType.Text);

					//Ramesh (08/06/2020) returns random A/B/C List
					string QueryA = "select sum(ws.availableqty) as availableqty,'A' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric >= " + config.amin + " group by ws.materialid order by random() limit " + limita + "";
					string QueryB = "select sum(ws.availableqty) as availableqty,'B' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric >= " + config.bmin + " and ws.unitprice::numeric <= " + config.bmax + " group by ws.materialid order by random() limit " + limitb + "";
					string QueryC = "select sum(ws.availableqty) as availableqty,'C' AS category,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice::numeric <= " + config.cmax + " group by ws.materialid order by random() limit " + limitc + "";
					var adata = await pgsql.QueryAsync<CycleCountList>(QueryA, null, commandType: CommandType.Text);
					var bdata = await pgsql.QueryAsync<CycleCountList>(QueryB, null, commandType: CommandType.Text);
					var cdata = await pgsql.QueryAsync<CycleCountList>(QueryC, null, commandType: CommandType.Text);
					var finaltempdata = adata.Concat(bdata);
					var finaldata = finaltempdata.Concat(cdata);
					foreach (CycleCountList cc in finaldata)
					{
						if (todaycountdata != null && todaycountdata.Count() > 0)
						{
							cc.todayscount = todaycountdata.Count();
						}
						else
						{
							cc.todayscount = 0;

						}
						string Querycheckcounted = "Select * from wms.cyclecount where materialid = '" + cc.materialid + "' and counted_on = current_date";
						var countdata = await pgsql.QueryAsync<CycleCountList>(Querycheckcounted, null, commandType: CommandType.Text);
						if (countdata != null && countdata.Count() > 0)
						{
							var dt = countdata.FirstOrDefault();
							cc.status = dt.status;
							cc.physicalqty = dt.physicalqty;
							cc.difference = dt.difference;
							cc.iscounted = true;

						}
					}

					return finaldata;

					//string query = WMSResource.getCyclecountList;
					//return await pgsql.QueryAsync<CycleCountList>(
					//query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetCyclecountPendingList>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<returns All counted Material list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<CycleCountList>> GetCyclecountPendingList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfigmodel config = new Cyclecountconfigmodel();
					await pgsql.OpenAsync();
					string QueryA = "Select * from wms.cyclecount";
					var adata = await pgsql.QueryAsync<CycleCountList>(QueryA, null, commandType: CommandType.Text);
					return adata;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountPendingList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UpdateinsertCycleCount>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update or insert cycle count>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateinsertCycleCount(List<CycleCountList> dataobj)
		{
			using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				DB.OpenAsync();
				try
				{
					foreach (CycleCountList obj in dataobj)
					{
						//Ramesh (08/06/2020) approver action updation 
						if (obj.isapprovalprocess)
						{
							string status = obj.isapproved ? "Approved" : "Rejected";

							string insertquery = "update wms.cyclecount set status='" + status + "', remarks='" + obj.remarks + "',verified_on = current_date , verified_by = 'Ramesh' where id = '" + obj.id + "' ";
							var result = DB.ExecuteScalar(insertquery);



						}
						else
						{
							//Ramesh (08/06/2020) user count action updation/insertion 
							string selquery = "select * from wms.cyclecount where materialid = '" + obj.materialid + "' and counted_on = current_date ";
							var seldata = DB.QueryAsync<CycleCountList>(selquery, null, commandType: CommandType.Text);
							CycleCountList dt = new CycleCountList();
							if (seldata.Result.Count() > 0)
							{
								//Ramesh (08/06/2020) user count action updation 
								obj.difference = Math.Abs(obj.physicalqty - obj.availableqty);
								string insertquery = "update wms.cyclecount set category='" + obj.category + "', materialid= '" + obj.materialid + "', availableqty= " + obj.availableqty + ", physicalqty=" + obj.physicalqty + ", difference=" + obj.difference + ", status='Pending', counted_on = current_date , counted_by = 'Ramesh', verified_on = null , verified_by = null where materialid = '" + obj.materialid + "' ";
								var result = DB.ExecuteScalar(insertquery);

							}
							else
							{
								//Ramesh (08/06/2020) user count action insertion 
								obj.difference = Math.Abs(obj.physicalqty - obj.availableqty);
								string insertquery = "insert into wms.cyclecount(category, materialid, availableqty, physicalqty, difference, status, counted_on, counted_by, verified_on, verified_by) values('" + obj.category + "', '" + obj.materialid + "', " + obj.availableqty + ", " + obj.physicalqty + ", " + obj.difference + ", 'Pending', current_date , 'Ramesh', null, null)";
								var result = DB.ExecuteScalar(insertquery);
							}

						}


					}
					return 1;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateinsertCycleCount", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					DB.Close();
				}
			}
		}

		/*
		Name of Function : <<UpdateCycleCountconfig>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update cycle count configuration >>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateCycleCountconfig(Cyclecountconfig dataobj)
		{
			try
			{
				//foreach(var item in dataobj._list)
				//{




				string insertquery = "update wms.cyclecountconfig set apercentage = " + dataobj.apercentage + ",bpercentage = " + dataobj.bpercentage + ",cpercentage = " + dataobj.cpercentage + ",cyclecount = " + dataobj.cyclecount + ",frequency = '" + dataobj.frequency + "',notificationtype='" + dataobj.notificationtype + "',notificationon='" + dataobj.notificationon + "' where id = 1";

				//string insertquery = WMSResource.updatecyclecountconfig.Replace("#cid", "1");

				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					var result = DB.ExecuteScalar(insertquery);
					//var result = DB.ExecuteScalar(insertquery, new
					//{

					//    dataobj.apercentage,
					//    dataobj.bpercentage,
					//    dataobj.cpercentage,
					//    dataobj.cyclecount,
					//    dataobj.frequency
					//});

				}
				return 1;

			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "UpdateCycleCountconfig", Ex.StackTrace.ToString());
				return 0;
			}
		}


		/*
		Name of Function : <<GetCyclecountConfig>>  Author :<<Ramesh>>  
		Date of Creation <<08/06/2020>>
		Purpose : <<update cycle count configuration>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<Cyclecountconfig> GetCyclecountConfig()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					Cyclecountconfig config = new Cyclecountconfig();
					await pgsql.OpenAsync();
					int count = 0;
					//Ramesh (08/06/2020) get count of all materials on which cycle configuration and randon percentage will be applied
					string QueryALL = "select sum(availableqty) as availableqty,ws.materialid AS materialid from wms.wms_stock ws WHERE ws.materialid IS NOT null and ws.unitprice IS NOT null  group by materialid";
					var alldata = await pgsql.QueryAsync<CycleCountList>(QueryALL, null, commandType: CommandType.Text);

					//Ramesh (08/06/2020) get cycle configuration

					string QueryABC = "select * from wms.cyclecountconfig where id = 1";
					var abcdata = await pgsql.QueryAsync<Cyclecountconfig>(QueryABC, null, commandType: CommandType.Text);
					if (abcdata != null)
					{
						config = abcdata.FirstOrDefault();
					}

					//Ramesh (08/06/2020) get ABC configuration start and end date of cycle
					string QueryABC1 = "select * from wms.wms_rd_category where deleteflag is false";
					var abcdata1 = await pgsql.QueryAsync<ABCCategoryModel>(QueryABC1, null, commandType: CommandType.Text);
					ABCCategoryModel abcconfig = new ABCCategoryModel();
					if (abcdata1 != null && config != null)
					{
						abcconfig = abcdata1.FirstOrDefault();
						config.startdate = abcconfig.startdate;
						config.enddate = abcconfig.enddate;
					}
					if (alldata != null && config != null)
					{
						count = alldata.Count();
						config.countall = count;

					}


					return config;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetCyclecountConfig", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetABCListBycategory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get ABC List By category>>
		<param name="category"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReportModel>> GetABCListBycategory(string category)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.GetABCdetailsBycategory.Replace("abcname", category);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetABCListBycategory", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetFIFOList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get FIFI list of material>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<FIFOModel>> GetFIFOList(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getFIFOList;
					if (material == "null")
					{
						query = query + " order by createddate asc";
					}
					else
					{
						query = query + " and sk.materialid like'%" + material + "' order by createddate asc";
					}


					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<FIFOModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetFIFOList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<checkloldestmaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<check oldest material>>
		<param name="materialid"></param>
		 <param name="createddate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public ReportModel checkloldestmaterial(string materialid, string createddate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.checkoldestmaterial.Replace("#materialid", materialid).Replace("#createddate", Convert.ToString(createddate));


					pgsql.Open();
					return pgsql.QuerySingle<ReportModel>(
					   query, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkloldestmaterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<FIFOitemsupdate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<FIFO items update>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int FIFOitemsupdate(List<FIFOModel> model)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					foreach (var item in model)
					{

						string insertforinvoicequery = WMSResource.insertFIFOdata;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertforinvoicequery, new
							{
								item.itemid,
								item.materialid,
								item.pono

							});
							int availableqty = item.availableqty - item.issuedqty;

							string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{

							});

						}
					}


					//}
					return (1);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getASNList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get list of todays expected shipments>>
		<param name="deliverydate"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> getASNList(string deliverydate)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + deliverydate + " 00:00:00' and asno.deliverydate <= '" + deliverydate + " 23:59:59'";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getASNListdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get ASN List data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<OpenPoModel>> getASNListdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					DateTime todayDate = DateTime.Now;
					string currentdatestr = todayDate.ToString("yyyy-MM-dd");
					DateTime weekbeforeDate = DateTime.Now.AddDays(-7);
					string weekbeforeDatestr = weekbeforeDate.ToString("yyyy-MM-dd");
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + weekbeforeDatestr + " 00:00:00' and asno.deliverydate <= '" + currentdatestr + " 23:59:59' order by asno.deliverydate";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getUserDashboarddata>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get User Dashboard data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<UserDashboardDetail> getUserDashboarddata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					UserDashboardDetail detail = new UserDashboardDetail();
					string deliverydate = DateTime.Now.ToString("yyyy-MM-dd");
					string query = WMSResource.getASNList;
					query = query + " where asno.deliverydate >= '" + deliverydate + " 00:00:00' and asno.deliverydate <= '" + deliverydate + " 23:59:59'";
					await pgsql.OpenAsync();
					var expectedrcpts = await pgsql.QueryAsync<OpenPoModel>(
					   query, null, commandType: CommandType.Text);
					if (expectedrcpts != null && expectedrcpts.Count() > 0)
					{
						detail.pendingshipments = expectedrcpts.Count();

					}
					string receivedlistqry = WMSResource.getsecurityreceivedlist;
					receivedlistqry = receivedlistqry + " where sl.invoicedate <= '" + deliverydate + " 23:59:59' and sl.invoicedate >= '" + deliverydate + " 00:00:00'";
					var receivedrcpts = await pgsql.QueryAsync<SecurityInwardreceivedModel>(
					   receivedlistqry, null, commandType: CommandType.Text);
					if (receivedrcpts != null && receivedrcpts.Count() > 0)
					{
						detail.receivedshipments = receivedrcpts.Count();

					}
					string outinquery = WMSResource.getnonreturnablegatepassdata;
					var outindata = await pgsql.QueryAsync<gatepassModel>(
					   outinquery, null, commandType: CommandType.Text);
					if (outindata != null && outindata.Count() > 0)
					{
						var outwardata = outindata.Where(o => o.outwarddate == null);
						var inwarddata = outindata.Where(o => o.outwarddate != null && o.inwarddate == null);
						var countoutward = outwardata.GroupBy(item => item.gatepassid).Select(g => new { name = g.Key, count = g.Count() });
						var countinward = inwarddata.GroupBy(item => item.gatepassid).Select(g => new { name = g.Key, count = g.Count() });
						detail.pendingtoinward = countinward.Count();
						detail.pendingtooutward = countoutward.Count();

					}

					string materialrequestquery = WMSResource.getpendingreceiptslist;

					var pendingrcptsdata = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					if (pendingrcptsdata != null && pendingrcptsdata.Count() > 0)
					{
						detail.pendingtoreceive = pendingrcptsdata.Count();

					}

					string materialacceptquery = WMSResource.getgrnlistdata;

					var mataccpt = await pgsql.QueryAsync<ddlmodel>(
					  materialacceptquery, null, commandType: CommandType.Text);

					if (mataccpt != null && mataccpt.Count() > 0)
					{
						detail.pendingtoaccetance = mataccpt.Count();

					}




					string materialputawayquery = WMSResource.getgrnlistdataforputaway;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					var matputaway = await pgsql.QueryAsync<ddlmodel>(
					  materialputawayquery, null, commandType: CommandType.Text);
					if (matputaway != null && matputaway.Count() > 0)
					{
						foreach (ddlmodel ddl in matputaway)
						{
							var exixtedrow = returnlist.Where(o => o.text == ddl.text).FirstOrDefault();
							if (exixtedrow == null)
							{
								returnlist.Add(ddl);

							}

						}
						detail.pendingtoputaway = returnlist.Count();

					}

					string queryqc = WMSResource.getqualitycheckdropdown;// + pono+"'";//li
					var qcdata = await pgsql.QueryAsync<ddlmodel>(
					   queryqc, null, commandType: CommandType.Text);
					if (qcdata != null && qcdata.Count() > 0)
					{
						detail.pendingtoqualitycheck = qcdata.Count();

					}
					DateTime sevendaysbefore = DateTime.Now.AddDays(-7);
					string sevendaysbeforestr = sevendaysbefore.ToString("yyyy-MM-dd");
					string queryrsrv = "select itemid as value from wms.materialreserve where reservedby = '" + empno + "' and reserveupto >= '" + sevendaysbeforestr + " 00:00:00' and reserveupto <= '" + deliverydate + " 23:59:59'";
					var rsvdata = await pgsql.QueryAsync<ddlmodel>(
					   queryrsrv, null, commandType: CommandType.Text);
					if (rsvdata != null && rsvdata.Count() > 0)
					{
						detail.reservedquantityforthisweek = rsvdata.Count();

					}




					return detail;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getASNList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterial_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Item location List By mterial_old>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial_old(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationList.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListBymterial>>  Author :<<Ramesh>>  
		Date of Creation <<06_10_2020>>
		Purpose : <<get itemlocation to issue materials>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterial(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationforissue.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getItemlocationListByIssueId>>  Author :<<Ramesh>>  
		Date of Creation <<10_01_2020>>
		Purpose : <<Get list of Material issued by issueid>>
		<param name="requestforissueid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		/// <summary>
		/// get materiallistfor stock transfer
		/// Ramesh  kumar 12_10_2020
		/// </summary>
		/// <param name="material"></param>
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListBymterialsourcelocation(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationforstocktransfer.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}
		/// <summary>
		/// Get list of Material issued by issueid
		/// </summary>
		/// <param name="requestforissueid"></param>
		/// Revised by Ramesh 10_01_2020
		/// <returns></returns>
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByIssueId(string requestforissueid)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					//string query = WMSResource.getitemlocationListBysIssueId.Replace("#requestforissueid", requestforissueid);
					string query = WMSResource.getitemlocationListBysIssueId_v1.Replace("#requestforissueid", requestforissueid);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);

					IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.itemlocation, c.createddate }).Select(t => new IssueRequestModel
					{

						availableqty = t.Sum(u => u.availableqty),
						issuedqty = t.Sum(u => u.issuedqty),
						pono = t.First().pono,
						materialid = t.First().materialid,
						itemid = t.First().itemid,
						Materialdescription = t.First().Materialdescription,
						material = t.First().material,
						itemlocation = t.Key.itemlocation,
						createddate = t.Key.createddate
					});
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getItemlocationListByGatepassmaterialid>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Item location List By Gatepass materialid>>
		<param name="gatepassmaterialid"></param>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getItemlocationListByGatepassmaterialid(string gatepassmaterialid)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getItemlocationListByGatepassmaterialid_v1.Replace("#gatepassmaterialid", gatepassmaterialid);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					 query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);

					//IEnumerable<IssueRequestModel> result = data.GroupBy(c => new { c.itemlocation, c.createddate }).Select(t => new IssueRequestModel
					//{

					//	availableqty = t.Sum(u => u.availableqty),
					//	issuedqty = t.Sum(u => u.issuedqty),
					//	pono = t.First().pono,
					//	materialid = t.First().materialid,
					//	itemid = t.First().itemid,
					//	Materialdescription = t.First().Materialdescription,
					//	material = t.First().material,
					//	itemlocation = t.Key.itemlocation,
					//	createddate = t.Key.createddate
					//});
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetItemlocationListforST>>  Author :<<Ramesh>>  
		Date of Creation <<29/07/2020>>
		Purpose : <<Get item location list for stock transfer>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetItemlocationListforST(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getitemlocationList.Replace("#materialid", material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					data = data.OrderByDescending(o => o.createddate);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetItemlocationListBymterial", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<updateissuedmaterial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update issued material details>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updateissuedmaterial(List<IssueRequestModel> obj)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					foreach (var item in obj)
					{

						string insertforinvoicequery = WMSResource.insertFIFOdata;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							//string approvedstatus = string.Empty;
							//if (item.issuedqty != 0)
							//{
							//	approvedstatus = "approved";
							//}
							////else
							////{
							////	approvedstatus = "rejected";
							////}
							//DateTime approvedon = System.DateTime.Now;
							//int itemid = 0;
							//int reserveformaterialid = item.reserveformaterialid;
							//string materialid = item.materialid;
							//int issuedqty = item.issuedqty;
							//DateTime itemissueddate = System.DateTime.Now;
							//string updateapproverstatus = WMSResource.updateapproverstatus;



							//var result = DB.Execute(updateapproverstatus, new
							//{
							//	approvedstatus,
							//	reserveformaterialid,
							//	approvedon,
							//	issuedqty,
							//	materialid,
							//	item.pono,
							//	itemid,
							//	item.itemreturnable,
							//	item.approvedby,
							//	itemissueddate,
							//	item.itemreceiverid,

							//});

							int availableqty = item.availableqty - item.issuedquantity;

							string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(item.issuedqty));

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{

							});

						}
					}


					//}
					return (1);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "FIFOitemsupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<AssignRoles>>  Author :<<prasanna>>  
		Date of Creation <<10-06-2020>>
		Purpose : <<insert method to Asssign roles for employee>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int assignRole(authUser model)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					model.createddate = System.DateTime.Now;
					string insertquery = WMSResource.insertAuthUserData;
					model.deleteflag = false;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var results = DB.ExecuteScalar(insertquery, new
						{
							model.employeeid,
							model.roleid,
							model.createddate,
							model.createdby,
							model.deleteflag
						});
						return (Convert.ToInt32(results));
					}
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "assignRole", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		//Name of Function : <<getuserAcessList>>  Author :<<prasanna>>  
		//Date of Creation <<11-06-2020>>
		//Purpose : <<function used to get Acessnames list based on employeeid,roleid >>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<IEnumerable<userAcessNamesModel>> getuserAcessList(string employeeid, string roleid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getUserAcessNames.Replace("#employeeid", employeeid);
					query = query.Replace("#roleid", roleid);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<userAcessNamesModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserAcessList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Name of Function : <<getuserAcessList>>  Author :<<Ramesh>>  
		//Date of Creation <<28-07-2020>>
		//Purpose : <<function used to get role list based on employeeid>>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<IEnumerable<userAcessNamesModel>> getuserroleList(string employeeid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string query = WMSResource.getuserroles.Replace("#employeeid", employeeid);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<userAcessNamesModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getuserroleList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		//Name of Function : <<getdashboarddata>>  Author :<<Ramesh>>  
		//Date of Creation <<17-06-2020>>
		//Purpose : <<function to get dashboard data >>
		//Review Date :<<>>   Reviewed By :<<>>
		public async Task<DashboardModel> getdashboarddata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					DashboardModel result = new DashboardModel();
					result.graphdata = new List<DashboardGraphModel>();
					DateTime dt = DateTime.Now;
					int i = 1;
					while (i <= 7)
					{
						if (i == 1)
						{
							string date = dt.ToString("yyyy-MM-dd");
							string query1 = "Select Count(*) as todayexpextedcount from wms.openpolistview where deliverydate = current_date";
							string query2 = "select count(*) as todayreceivedcount from wms.wms_securityinward where receiveddate <= '" + date + " 23:59:59' and receiveddate >= '" + date + " 00:00:00' ";
							string query3 = "select count(*) as todaytoissuecount from wms.wms_materialrequest where requesteddate = '" + date + " 23:59:59' and requesteddate >= '" + date + " 00:00:00'";

							await pgsql.OpenAsync();
							var data1 = await pgsql.QueryAsync<DashboardModel>(query1, null, commandType: CommandType.Text);
							var data2 = await pgsql.QueryAsync<DashboardModel>(query2, null, commandType: CommandType.Text);
							var data3 = await pgsql.QueryAsync<DashboardModel>(query3, null, commandType: CommandType.Text);
							DashboardGraphModel md = new DashboardGraphModel();
							md.date = dt.ToString("dd/MM/yyyy");
							if (data1 != null)
							{
								result.todayexpextedcount = data1.FirstOrDefault().todayexpextedcount;
								md.expectedcount = data1.FirstOrDefault().todayexpextedcount;
							}
							if (data2 != null)
							{
								result.todayreceivedcount = data2.FirstOrDefault().todayreceivedcount;
								md.receivedcount = data2.FirstOrDefault().todayreceivedcount;
							}
							if (data3 != null)
							{
								result.todaytoissuecount = data3.FirstOrDefault().todaytoissuecount;
								md.toissuecount = data3.FirstOrDefault().todaytoissuecount;
							}
							result.graphdata.Add(md);
						}
						else
						{
							dt = dt.AddDays(-1);
							string date = dt.ToString("yyyy-MM-dd");
							string date1 = dt.ToString("dd/MM/yyyy");
							string query1 = "Select Count(*) as todayexpextedcount from wms.openpolistview where deliverydate = '" + date + "'";
							string query2 = "select count(*) as todayreceivedcount from wms.wms_securityinward where receiveddate <= '" + date + " 23:59:59' and receiveddate >= '" + date + " 00:00:00' ";
							string query3 = "select count(*) as todaytoissuecount from wms.wms_materialrequest where requesteddate = '" + date + " 23:59:59' and requesteddate >= '" + date + " 00:00:00'";
							var data1 = await pgsql.QueryAsync<DashboardModel>(query1, null, commandType: CommandType.Text);
							var data2 = await pgsql.QueryAsync<DashboardModel>(query2, null, commandType: CommandType.Text);
							var data3 = await pgsql.QueryAsync<DashboardModel>(query3, null, commandType: CommandType.Text);
							DashboardGraphModel md = new DashboardGraphModel();
							md.date = date1;
							if (data1 != null)
							{
								md.expectedcount = data1.FirstOrDefault().todayexpextedcount;
							}
							if (data2 != null)
							{
								md.receivedcount = data2.FirstOrDefault().todayreceivedcount;
							}
							if (data3 != null)
							{
								md.toissuecount = data3.FirstOrDefault().todaytoissuecount;
							}
							result.graphdata.Add(md);
						}

						i++;
					}



					return result;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getdashboarddata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getUserdashboardgraphdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get User  dashboard graph data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<UserDashboardGraphModel>> getUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT receiveddate::date as graphdate, COUNT(*),'Receive' as type ";
					rcvqry += " from wms.wms_storeinward";
					rcvqry += " WHERE receiveddate > now() - interval '7 days'";
					rcvqry += " GROUP BY receiveddate::date";
					rcvqry += " ORDER BY receiveddate::date ASC";

					string qcqry = "SELECT qcdate::date as graphdate, COUNT(*),'Quality' as type ";
					qcqry += " from wms.wms_qualitycheck";
					qcqry += " WHERE qcdate > now() - interval '7 days'";
					qcqry += " GROUP BY qcdate::date";
					qcqry += " ORDER BY qcdate::date ASC";

					string accqry = "SELECT returnedon::date as graphdate, COUNT(*),'Accept' as type ";
					accqry += " from wms.wms_storeinward";
					accqry += " WHERE returnedon > now() - interval '7 days'";
					accqry += " GROUP BY returnedon::date";
					accqry += " ORDER BY returnedon::date ASC";

					string pwqry = "SELECT createddate::date as graphdate, COUNT(*),'Putaway' as type ";
					pwqry += " from wms.wms_stock";
					pwqry += " WHERE createddate > now() - interval '7 days'";
					pwqry += " GROUP BY createddate::date";
					pwqry += " ORDER BY createddate::date ASC";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getManagerdashboardgraphdata>>  Author :<<Gayathri>>  
		Date of Creation <<>>
		Purpose : <<get Manager  dashboard graph data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<ManagerDashboard> getManagerdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					//Get count of po and invoice for pending receipts
					string penqry = "select count(*) as pendingcount from wms.wms_securityinward where receiveddate> now() - interval '7 days' and grnnumber is null and onhold is null ";

					//Get count of po and invoice for on hold receipts
					string onholdqry = "select count(*) as onholdcount from wms.wms_securityinward where receiveddate> now() - interval '7 days' and grnnumber is null and onhold = true ";

					//Get count of po and invoice for complete receipts
					string completeqry = "select count(*) as completedcount from wms.wms_securityinward where receiveddate> now() - interval '7 days' and grnnumber is not null and onhold is null ";

					//Get count of quality check completed
					string qualitycompl= "select count(*),COUNT(*) OVER () as  qualitycompcount from wms.wms_storeinward where receiveddate> now() - interval '7 days' and qualitycheckrequired =true and qualitychecked =true group by inwmasterid";

					//Get count of quality check pending
					string qualitypending = "select count(*),COUNT(*) OVER () as qualitypendcount from wms.wms_storeinward where receiveddate> now() - interval '7 days' and qualitycheckrequired =true and qualitychecked is null group by inwmasterid";

					//Get count of pending GRN's - putaway 
					string putawaypend= " select count(*),COUNT(*) OVER () as putawaypendcount from wms.wms_securityinward secinw  where secinw.inwmasterid not in (select distinct inwmasterid  from wms.wms_stock where inwmasterid is not null order by inwmasterid desc) and receiveddate> now() - interval '7 days' group by secinw.inwmasterid ";

					//Get count of completed GRN's - putaway 
					string putawaycomp = " select sinw.grnnumber,COUNT(*) OVER () as putawaycompcount from wms.wms_storeinward stinw  join wms.wms_securityinward sinw on stinw.inwmasterid = sinw.inwmasterid where stinw.returnedby is not null and sinw.isdirecttransferred is NOT true and stinw.inwardid  in (select distinct inwardid from wms.wms_stock where inwardid is not null and createddate> now() - interval '7 days'  order by inwardid desc) group by sinw.grnnumber";

					//Get count of In progress GRN's - putaway
					string putawayinprogres = " select sinw.grnnumber,COUNT(*) OVER () as putawayinprocount from wms.wms_storeinward stinw join wms.wms_securityinward sinw on stinw.inwmasterid = sinw.inwmasterid where stinw.returnedby is not null and sinw.isdirecttransferred is NOT true and stinw.inwardid not in (select distinct inwardid from wms.wms_stock where inwardid is not null and createddate> now() - interval '7 days'  order by inwardid desc)  group by sinw.grnnumber";

					//Get count of pending GRN's - Acceptance 
					string acceptancepenqry = "select count(*),COUNT(*) OVER () as acceptancependcount from wms.wms_storeinward stin where receiveddate> now() - interval '7 days' and returnqty is null and confirmqty is null   group by(inwmasterid)";

					//Get count of Accepted GRN's - Acceptance 
					string acceptancecomptqry = "select count(*),COUNT(*) OVER () as acceptancecompcount from wms.wms_storeinward stin where receiveddate> now() - interval '7 days' and returnqty is not null and confirmqty is not null  group by(inwmasterid)";

					var data1 = await pgsql.QueryAsync<ManagerDashboard>(penqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<ManagerDashboard>(onholdqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<ManagerDashboard>(completeqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<ManagerDashboard>(qualitycompl, null, commandType: CommandType.Text);
					var data5 = await pgsql.QueryAsync<ManagerDashboard>(qualitypending, null, commandType: CommandType.Text);
                    var data6 = await pgsql.QueryAsync<ManagerDashboard>(putawaypend, null, commandType: CommandType.Text);
                    var data7 = await pgsql.QueryAsync<ManagerDashboard>(putawaycomp, null, commandType: CommandType.Text);
                    var data8 = await pgsql.QueryAsync<ManagerDashboard>(putawayinprogres, null, commandType: CommandType.Text);
                    var data9 = await pgsql.QueryAsync<ManagerDashboard>(acceptancepenqry, null, commandType: CommandType.Text);
					var data10 = await pgsql.QueryAsync<ManagerDashboard>(acceptancecomptqry, null, commandType: CommandType.Text);

					var data = new ManagerDashboard();
					data.pendingcount = data1.FirstOrDefault().pendingcount;
					data.onholdcount = data2.FirstOrDefault().onholdcount;
					data.completedcount = data3.FirstOrDefault().completedcount;
					if(data4.Count()>0)
                    {
						data.qualitycompcount = data4.FirstOrDefault().qualitycompcount;
					}
					
					data.qualitypendcount = data5.FirstOrDefault().qualitypendcount;
					if (data7.Count() > 0)
					{
						data.putawaycompcount = data7.FirstOrDefault().putawaycompcount;
					}
					
                    data.putawaypendcount = data6.FirstOrDefault().putawaypendcount;
                    data.putawayinprocount = data8.FirstOrDefault().putawayinprocount;
                    data.acceptancependcount = data9.FirstOrDefault().acceptancependcount;
					if (data10.Count()>0)
					{
						data.acceptancecompcount = data10.FirstOrDefault().acceptancecompcount;
					}
					

					return data;
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getManagerdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
		Name of Function : <<getWeeklyUserdashboardgraphdata>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Weekly User dashboard graphdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<UserDashboardGraphModel>> getWeeklyUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{

					string rcvqry = "SELECT date_part('year', receiveddate::date) as syear,";
					rcvqry += " date_part('week', receiveddate::date) AS sweek,COUNT(*) as count, 'Receive' as type";
					rcvqry += " FROM wms.wms_storeinward where receiveddate is not null and receiveddate > now() - interval '1 month' and extract(year from receiveddate) > 2000";
					rcvqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string qcqry = "SELECT date_part('year', qcdate::date) as syear,";
					qcqry += " date_part('week', qcdate::date) AS sweek,COUNT(*) as count, 'Quality' as type";
					qcqry += " FROM wms.wms_qualitycheck where qcdate is not null and qcdate > now() - interval '1 month' and extract(year from qcdate) > 2000";
					qcqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string accqry = "SELECT date_part('year', returnedon::date) as syear,";
					accqry += " date_part('week', returnedon::date) AS sweek,COUNT(*) as count, 'Accept' as type";
					accqry += " FROM wms.wms_storeinward where returnedon is not null and returnedon > now() - interval '1 month' and extract(year from returnedon) > 2000";
					accqry += " GROUP BY syear, sweek ORDER BY syear, sweek";

					string pwqry = "SELECT date_part('year', createddate::date) as syear,";
					pwqry += " date_part('week', createddate::date) AS sweek,COUNT(*) as count, 'Putaway' as type";
					pwqry += " FROM wms.wms_stock where createddate is not null and createddate > now() - interval '1 month' and extract(year from createddate) > 2000";
					pwqry += " GROUP BY syear, sweek ORDER BY syear, sweek";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getWeeklyUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getmonthlyUserdashboardgraphdata>>  Author :<<LP>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get monthly User dashboard graphdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<UserDashboardGraphModel>> getmonthlyUserdashboardgraphdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{


					string rcvqry = "select to_char(receiveddate,'Mon') as smonth,";
					rcvqry += " extract(year from receiveddate) as syear,";
					rcvqry += " count(*) as count,";
					rcvqry += " 'Receive' as type";
					rcvqry += " from wms.wms_storeinward where receiveddate > now() - interval '1 year' and receiveddate is not null and extract(year from receiveddate) > 2000";
					rcvqry += " group by 1,2 order by to_char(receiveddate, 'Mon') DESC";

					string qcqry = "select to_char(qcdate,'Mon') as smonth,";
					qcqry += " extract(year from qcdate) as syear,";
					qcqry += " count(*) as count,";
					qcqry += " 'Quality' as type";
					qcqry += " from wms.wms_qualitycheck where qcdate > now() - interval '1 year' and qcdate is not null and extract(year from qcdate) > 2000";
					qcqry += " group by 1,2 order by to_char(qcdate, 'Mon') DESC";

					string accqry = "select to_char(returnedon,'Mon') as smonth,";
					accqry += " extract(year from returnedon) as syear,";
					accqry += " count(*) as count,";
					accqry += " 'Accept' as type";
					accqry += " from wms.wms_storeinward where returnedon > now() - interval '1 year' and returnedon is not null and extract(year from returnedon) > 2000";
					accqry += " group by 1,2 order by to_char(returnedon, 'Mon') DESC";

					string pwqry = "select to_char(createddate,'Mon') as smonth,";
					pwqry += " extract(year from createddate) as syear,";
					pwqry += " count(*) as count,";
					pwqry += " 'Putaway' as type";
					pwqry += " from wms.wms_stock where createddate > now() - interval '1 year' and createddate is not null and extract(year from createddate) > 2000";
					pwqry += " group by 1,2 order by to_char(createddate, 'Mon') DESC";


					var data1 = await pgsql.QueryAsync<UserDashboardGraphModel>(rcvqry, null, commandType: CommandType.Text);
					var data2 = await pgsql.QueryAsync<UserDashboardGraphModel>(qcqry, null, commandType: CommandType.Text);
					var data3 = await pgsql.QueryAsync<UserDashboardGraphModel>(accqry, null, commandType: CommandType.Text);
					var data4 = await pgsql.QueryAsync<UserDashboardGraphModel>(pwqry, null, commandType: CommandType.Text);

					var data = data1.Concat(data2.Concat(data3.Concat(data4)));

					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmonthlyUserdashboardgraphdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialRequestdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Request data>>
		<param name="approverid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialRequestdata(string pono, string approverid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialdetailfprrequest.Replace("#manager", approverid);
					if (pono != null && pono != "undefined" && pono != "null")
					{
						materialrequestquery = materialrequestquery + " and (sk.pono = '" + pono + "' or po.suppliername = '" + pono + "')";
					}
					//if (approverid != null)
					//{
					//	materialrequestquery = materialrequestquery + " and pro.projectmanager = '" + approverid + "' ";
					//}
					materialrequestquery = materialrequestquery + " group by sk.materialid";
					await pgsql.OpenAsync();
					var  data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data.Where(o => o.availableqty > 0); 

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<MaterialReservedata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Material Reserve data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> MaterialReservedata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					string materialrequestquery = WMSResource.getmaterialstoreserve;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "MaterialRequestdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getempnamebycode>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get emp name by code>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<User> getempnamebycode(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();

					string userquery = "select  * from wms.employee where employeeno='" + empno + "'";
					return pgsql.QuerySingle<User>(
					   userquery, null, commandType: CommandType.Text);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getempnamebycode", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getissuematerialdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get issue material details>>
		<param name="requestid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getissuematerialdetails(int requestid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.issuedqtydetails.Replace("#requestid", Convert.ToString(requestid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<IssueRequestModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertResevematerial_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert Resevematerial_old>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<int> insertResevematerial_old(List<ReserveMaterialModel> datamodel)
		{
			int reserveid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				ReserveMaterialModel obj = new ReserveMaterialModel();
				pgsql.Open();
				string query = WMSResource.getnextreservetid;
				obj = pgsql.QueryFirstOrDefault<ReserveMaterialModel>(
				   query, null, commandType: CommandType.Text);
				if (obj == null)
					reserveid = 1;
				else
				{
					//reserveid = obj.reserveid + 1;
				}

			}
			try
			{
				var result = 0;
				foreach (var item in datamodel)
				{

					if (item.quantity > 0)
					{

						item.materialid = item.material;
						item.reservedqty = item.quantity;
						item.reservedby = item.requesterid;

						string insertquery = WMSResource.insertreservematerial;
						string itemnoquery = WMSResource.getitemiddata.Replace("#materialid", item.material);
						if (item.pono != null)
						{
							itemnoquery = itemnoquery + "and pono='" + item.pono + "'";
						}
						using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
						{

							DB.Open();
							//await DB.OpenAsync();
							var itemData = await DB.QueryAsync<StockModel>(
						    itemnoquery, null, commandType: CommandType.Text);
							int remainingqty = item.quantity;
							itemData = itemData.OrderBy(o => o.createddate);
							int reservedqty = item.reservedqty;
							int Totalreservedqty = 0;
							foreach (StockModel data in itemData)
							{
								if (item.pono == null)
								{
									item.pono = data.pono;
								}
								if (reservedqty >= 0)
								{
									if (reservedqty <= data.availableqty)
										reservedqty = reservedqty;
									else
										reservedqty = data.availableqty;
									item.itemid = data.itemid;

									result = DB.Execute(insertquery, new
									{
										item.materialid,
										item.itemid,
										item.pono,
										item.reservedby,
										reservedqty,
										reserveid,
										item.reserveupto,
										item.projectcode,
										item.remarks
									});


									//update stock table
									if (result != 0)
									{
										//int availableqty = item.availableqty - reserveQty;
										string updatequery = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(reservedqty));
										//WMSResource.updatestock.Replace("#availableqty", Convert.ToString(availableqty)).Replace("#itemid", Convert.ToString(item.itemid));
										using (IDbConnection pgsql = new NpgsqlConnection(config.PostgresConnectionString))
										{
											result = pgsql.Execute(updatequery, new
											{
											});
										}
										Totalreservedqty += reservedqty;
										reservedqty = item.reservedqty - reservedqty;
									}
									//break;

								}
								if (Totalreservedqty >= item.reservedqty)
									break;

							}
						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "insertResevematerial", Ex.StackTrace.ToString());
				return 0;
			}
		}


		/*
		Name of Function : <<insertResevematerial>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Reserve material + Email-->>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<int> insertResevematerial(List<ReserveMaterialModel> datamodel)
		{
			int reserveid = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				NpgsqlTransaction Trans = null;

				try
				{
					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string materials = "";
					MaterialTransaction mainmodel = new MaterialTransaction();
										
					mainmodel.pono = datamodel[0].pono;
					mainmodel.projectcode = datamodel[0].projectcode;
					mainmodel.remarks = datamodel[0].remarks;
					mainmodel.reservedby = datamodel[0].requesterid;
					mainmodel.reserveupto = datamodel[0].reserveupto;
					string insertmatquery = WMSResource.insertmaterialreserve;
					var result = pgsql.ExecuteScalar(insertmatquery, new
					{

						mainmodel.pono,
						mainmodel.reservedby,
						mainmodel.reserveupto,
						mainmodel.projectcode,
						mainmodel.remarks
					});
					if (result != null)
					{
						int stindex = 0;
						foreach (var item in datamodel)
						{
							if(stindex > 0)
                            {
								materials += ", ";
                            }
							materials += item.material;

							MaterialTransactionDetail detail = new MaterialTransactionDetail();
							detail.id = Guid.NewGuid().ToString();
							detail.reserveid = result.ToString();
							detail.materialid = item.material;
							detail.reservedqty = item.quantity;
							detail.itemid = item.itemid;

							string itemnoquery = WMSResource.getitemiddata.Replace("#materialid", item.material);
							var itemData = await pgsql.QueryAsync<StockModel>(
							itemnoquery, null, commandType: CommandType.Text);
							int remainingqty = item.quantity;
							itemData = itemData.OrderBy(o => o.createddate);
							int reservedqty = item.quantity;
							int Totalreservedqty = 0;
							string insertdataqry = WMSResource.insertmaterialreservedetails;
							foreach (StockModel data in itemData)
							{
								if (item.pono == null)
								{
									item.pono = data.pono;
								}
								if (reservedqty >= 0)
								{
									if (reservedqty <= data.availableqty)
										reservedqty = reservedqty;
									else
										reservedqty = data.availableqty;
									item.itemid = data.itemid;
									string id = Guid.NewGuid().ToString();
									var result1 = pgsql.Execute(insertdataqry, new
									{

										id,
										detail.reserveid,
										detail.materialid,
										data.itemid,
										reservedqty

									});

								


									//update stock table
									if (result1 != 0)
									{
										string updatequery = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(item.itemid)).Replace("#issuedqty", Convert.ToString(reservedqty));
											var result3 = pgsql.Execute(updatequery, new
											{
											});
										
										Totalreservedqty += reservedqty;
										reservedqty = item.quantity - reservedqty;
									}
									//break;

								}
								stindex++;
								if (Totalreservedqty >= item.quantity)
									break;

							}
							
							




						}
                        EmailModel emailmodel = new EmailModel();
                        emailmodel.pono = datamodel[0].pono;
                        emailmodel.jobcode = datamodel[0].projectcode;
                        emailmodel.material = materials;
                        emailmodel.createdby = datamodel[0].requesterid;
                        emailmodel.createddate = DateTime.Now;


                        //emailmodel.ToEmailId = "developer1@in.yokogawa.com";
                        emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
                        //emailmodel.CC = "sushma.patil@in.yokogawa.com";
                        EmailUtilities emailobj = new EmailUtilities();
                        emailobj.sendEmail(emailmodel,4,3);
                        Trans.Commit();
						return 1;

					}
					else
					{
						Trans.Rollback();
						return 0;
					}




				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "MaterialRequest", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}

			
		}

		/*
		Name of Function : <<GetReservedMaterialList_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Reserved MaterialList_old>>
		<param name="reservedby"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReserveMaterialModel>> GetReservedMaterialList_old(string reservedby)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.getreservedmaterialList.Replace("#reservedby", reservedby);
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReservedMaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getOpenPoList>>  Author :<<Ramesh>>  
		Date of Creation <<05-10-2020>>
		Purpose : <<get material reserve list>>
		<param name="reservedby"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialTransaction>> GetReservedMaterialList(string reservedby)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				string materialrequestquery = WMSResource.getmaterialreserves.Replace("#reservedby", reservedby);

				try
				{
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialTransaction>(
					   materialrequestquery, null, commandType: CommandType.Text);
					foreach (MaterialTransaction trans in data)
					{
						trans.materialdata = new List<MaterialTransactionDetail>();
						string materialrequestdataquery = WMSResource.getmaterialreservedata.Replace("#reserveid", trans.reserveid);
						var data1 = await pgsql.QueryAsync<MaterialTransactionDetail>(
						materialrequestdataquery, null, commandType: CommandType.Text);
						trans.materialdata = data1.ToList();

					}
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReservedMaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
			
		}

		/*
		Name of Function : <<getissuematerialdetailsforreserved>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get issue material details for reserved>>
		<param name="reservedid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<ReserveMaterialModel>> getissuematerialdetailsforreserved(int reservedid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{


				try
				{
					string materialrequestquery = WMSResource.Getreleasedqty.Replace("#reserveid", Convert.ToString(reservedid));

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getissuematerialdetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetReleasedmaterialList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Released material List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReserveMaterialModel>> GetReleasedmaterialList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.GetreleasedmaterialList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetReleasedmaterialList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetmaterialdetailsByreserveid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get material details By reserveid>>
		<param name="reserveid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ReserveMaterialModel>> GetmaterialdetailsByreserveid(string reserveid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getmaterialdetailsbyreserveid.Replace("#reserveid", reserveid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetmaterialdetailsByreserveid", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<ApproveMaterialRelease>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Approve Material Release>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int ApproveMaterialRelease(List<ReserveMaterialModel> dataobj)
		{
			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string approvedstatus = string.Empty;
					if (item.issuedqty != 0)
					{
						approvedstatus = "approved";
					}
					//else
					//{
					//	approvedstatus = "rejected";
					//}
					DateTime approvedon = System.DateTime.Now;
					int itemid = 0;
					using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
					{
						IssueRequestModel obj = new IssueRequestModel();
						IssueRequestModel objs = new IssueRequestModel();
						pgsql.Open();
						string getitemidqry = WMSResource.getitemid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);
						// string getmaterialidqry = WMSResource.getrequestforissueid.Replace("#materialid", item.materialid).Replace("#pono", item.pono);

						obj = pgsql.QueryFirstOrDefault<IssueRequestModel>(
						   getitemidqry, null, commandType: CommandType.Text);
						itemid = obj.itemid;
						//objs = pgsql.QuerySingle<IssueRequestModel>(
						//getmaterialidqry, null, commandType: CommandType.Text);

						//requestforissueid = objs.requestforissueid;
					}



					int reserveformaterialid = item.reserveformaterialid;
					string materialid = item.materialid;
					int issuedqty = item.issuedqty;
					DateTime itemissueddate = System.DateTime.Now;

					string updateapproverstatus = WMSResource.updateapproverstatusforrelease;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							approvedstatus,
							reserveformaterialid,
							approvedon,
							issuedqty,
							materialid,
							item.pono,
							itemid,
							item.itemreturnable,
							item.approvedby,
							itemissueddate,
							item.itemreceiverid,

						});
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "updaterequestedqty", Ex.StackTrace.ToString());
				return 0;
			}


		}


		/*
		Name of Function : <<acknowledgeMaterialReceivedforreserved>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<acknowledge Material Received for reserved>>
		<param name="dataobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int acknowledgeMaterialReceivedforreserved(List<ReserveMaterialModel> dataobj)
		{
			try
			{
				var result = 0;
				//data.createddate = System.DateTime.Now;
				foreach (var item in dataobj)
				{
					string ackstatus = string.Empty;
					if (item.status == true)
					{
						ackstatus = "received";
					}
					else if (item.status == false)
					{
						ackstatus = "not received";
					}
					DateTime approveddate = System.DateTime.Now;

					int requestforissueid = item.reserveformaterialid;
					string reserveid = item.reserveid;
					string ackremarks = item.ackremarks;
					int issuedquantity = item.issuedqty;
					string updateackstatus = WMSResource.updateackstatusforreserved;

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateackstatus, new
						{
							ackstatus,
							ackremarks,
							reserveid,

						});
					}
				}
				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "acknowledgeMaterialReceivedforreserved", Ex.StackTrace.ToString());
				return 0;
			}

		}

		/*
		Name of Function : <<getSecurityreceivedList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get received po list based on current date>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<SecurityInwardreceivedModel>> getSecurityreceivedList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					DateTime dt = DateTime.Now;
					string date = dt.ToString("yyyy-MM-dd");
					string query = WMSResource.getsecurityreceivedlist;
					query = query + " where sl.invoicedate <= '" + date + " 23:59:59' and sl.invoicedate >= '" + date + " 00:00:00'";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<SecurityInwardreceivedModel>(
					   query, null, commandType: CommandType.Text);
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSecurityreceivedList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertquantitycheck>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert quantity check>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<string> insertquantitycheck(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				var results = 0;
				try
				{
					await pgsql.OpenAsync();
					
					string checkedon = DateTime.Now.ToString("yyyy-MM-dd");
					foreach (var item in datamodel)
					{
						//string insertforquality = WMSResource.insertqualitycheck.Replace("#inwardid", item.inwardid.ToString());
						string insertforquality = WMSResource.savequalityquery;
						string materialid = item.Material;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							results = Convert.ToInt32(DB.ExecuteScalar(insertforquality, new
							{
								item.inwardid,
								item.qualitypassedqty,
								item.qualityfailedqty,
								item.receivedby,
								item.remarks

							})
							);
							string query = "UPDATE wms.wms_storeinward set  qualitychecked = True where inwardid = '" + item.inwardid + "'";
							DB.ExecuteScalar(query);
							//inwardid = Convert.ToInt32(results);
							//if (inwardid != 0)
							//{
							//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

							//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
							//    {
							//        inwardid,
							//        datamodel.quality,
							//        datamodel.qtype,
							//        datamodel.qcdate,
							//        datamodel.qcby,
							//        datamodel.remarks,
							//        datamodel.deleteflag,

							//    });
						

						}
						
					}
					if (Convert.ToInt32(results) != 0)
					{
						EmailModel emailmodel = new EmailModel();
						emailmodel.pono = datamodel[0].pono;
						emailmodel.jobcode = datamodel[0].projectname;
						emailmodel.grnnumber = datamodel[0].grnnumber;
						//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
						//emailmodel.CC = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 3,3);
					}

					//}
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<getprojectlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getprojectlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getAllprojectlist;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					var senddata = data.Where(o => o.projectmanager != null);
					return senddata;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getprojectlistbymanager>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get project list by manager>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getprojectlistbymanager(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getprojectlist.Replace("#manager",empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getprojectlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getmatlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material list>>
		<param name="querytext"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getmatlist(string querytext = "")
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select material as value,materialdescription as text from wms.\"MaterialMasterYGS\" ";
					if(querytext != "" && querytext != null )
                    {
						materialrequestquery += " where material ilike '" + querytext + "%' or materialdescription ilike '" + querytext + "%' ";

					}
					materialrequestquery += "limit 10";



					//string materialrequestquery = WMSResource.getmaterialfortransfer.Replace("#requestor", empno);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<getmatlistbyproject>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get mat list by project>>
		<param name="projectcode"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getmatlistbyproject(string projectcode)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getmaterialsbyprojectcode.Replace("#projectcode", projectcode);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatlistbyproject", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}


		/*
				Name of Function : <<insertreturn>>  Author :<<Ramesh>>  
				Date of Creation <<12-12-2019>>
				Purpose : <<insert return>>
				<param name="datamodel"></param>
				Review Date :<<>>   Reviewed By :<<>>
				*/
		public async Task<string> insertreturn(List<inwardModel> datamodel)
		{

			inwardModel obj = new inwardModel();
			inwardModel getgrnnoforpo = new inwardModel();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{

				try
				{
					await pgsql.OpenAsync();

					string checkedon = DateTime.Now.ToString("yyyy-MM-dd");
					foreach (var item in datamodel)
					{
						string insertforquality = WMSResource.insertreturndata.Replace("#inwardid", item.inwardid.ToString());
						string materialid = item.Material;
						using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
						{
							var results = DB.ExecuteScalar(insertforquality, new
							{
								item.confirmqty,
								item.returnqty,
								item.receivedby,
								item.returnremarks

							});
							//inwardid = Convert.ToInt32(results);
							//if (inwardid != 0)
							//{
							//    string insertqueryforqualitycheck =WMSResource.insertqueryforqualitycheck;

							//    var data = DB.ExecuteScalar(insertqueryforqualitycheck, new
							//    {
							//        inwardid,
							//        datamodel.quality,
							//        datamodel.qtype,
							//        datamodel.qcdate,
							//        datamodel.qcby,
							//        datamodel.remarks,
							//        datamodel.deleteflag,

							//    });
							string insertqueryforstatusforqty = WMSResource.insertqueryforstatusforqty;

							var data1 = DB.ExecuteScalar(insertqueryforstatusforqty, new
							{
								item.pono,
								item.returnqty

							});


						}
					}

					//}
					return "counted";
				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertquantitycheck", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



		/*
		Name of Function : <<Getlocationdata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get location data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getlocationdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_locator where deleteflag=false order by locatorname asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getlocationdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getbindata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get bin data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getbindata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = "select * from wms.wms_rd_bin where deleteflag=false  order by binnumber asc";
					string materialrequestquery =  "select binnumber, Max(binid) as binid  from wms.wms_rd_bin where deleteflag = false group by binnumber  order by binnumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getbindata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getrackdata>>  Author :<<Shashikala>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get rack data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getrackdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//string materialrequestquery = "select * from wms.wms_rd_rack where deleteflag=false order by racknumber asc";
					string materialrequestquery = "select racknumber,Max(rackid) as rackid  from wms.wms_rd_rack where deleteflag=false group by racknumber  order by racknumber asc";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getrackdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getbindataforputaway>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get bin data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getbindataforputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_bin where deleteflag=false  order by binnumber asc";
					//string materialrequestquery = "select binnumber, Max(binid) as binid  from wms.wms_rd_bin where deleteflag = false group by binnumber  order by binnumber asc";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getbindata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getrackdataforputaway>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get rack data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<dropdownModel>> Getrackdataforputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rd_rack where deleteflag=false order by racknumber asc";
					//string materialrequestquery = "select racknumber,Max(rackid) as rackid  from wms.wms_rd_rack where deleteflag=false group by racknumber  order by racknumber asc";
					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<dropdownModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getrackdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}


		/*
		Name of Function : <<Getrackdata>>  Author :<<Ramesh Kumar>>  
		Date of Creation <<15/07/2020>>
		Purpose : <<Get list of materials >>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<Materials>> GetMaterialcombo()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select material,materialdescription,qualitycheck from wms.\"MaterialMasterYGS\"";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialcombo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
	Name of Function : <<getapproverList>>  Author :<<Ramesh Kumar>>  
	Date of Creation <<15/07/2020>>
	Purpose : <<get approver List >>
	<param name="empid"></param>
	Review Date :<<>>   Reviewed By :<<>>
	*/
		public async Task<IEnumerable<employeeModel>> getapproverList(string empid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getimmediatemnger.Replace("#employeeno", empid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<employeeModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getapproverList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgatepassByapproverList>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get gatepass By approver List>>
		<param name="empid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<gatepassModel>> getgatepassByapproverList(string empid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getgatepassapproverdata.Replace("#approverid", empid);

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<gatepassModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgatepassByapproverList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<Getpagesbyroleid>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Getpages by roleid>>
		<param name="roleid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<pageModel>> Getpagesbyroleid(int roleid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = "select * from wms.wms_pages where roleid = " + roleid + "";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<pageModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getpagesbyroleid", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<Getpages>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get pages>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<pageModel>> Getpages()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = "select * from wms.wms_pages";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<pageModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "Getpages", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GatepassapproveByMail>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Gatepass approve By Mail>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int GatepassapproveByMail(gatepassModel model)
		{
			try
			{
				model.approverremarks = string.Empty;
				model.fmapproverremarks = string.Empty;
				var result = 0;
				return result;

				string updateapproverstatus = string.Empty;


				if (model.categoryid == 1)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbymanager.Replace("#approverstatus", model.approverstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							model.approverremarks,
							model.gatepassid

						});
						if (result == 1)
						{
							int label = 1;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.approverstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}
				else if (model.categoryid == 2)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbyFMmanager.Replace("#fmapprovedstatus", model.fmapprovedstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result1 = DB.Execute(updateapproverstatus, new
						{
							model.fmapproverremarks,
							model.gatepassid

						});
						if (result1 == 1)
						{
							int label = 2;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.fmapprovedstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GatepassapproveByMail", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<GatepassapproveByManager>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Gate pass approve By Manager>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int GatepassapproveByManager(gatepassModel model)
		{
			try
			{
				var result = 0;

				string updateapproverstatus = string.Empty;

				if (model.categoryid == 1)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbymanager.Replace("#approverstatus", model.approverstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						result = DB.Execute(updateapproverstatus, new
						{
							model.approverremarks,
							model.gatepassid

						});
						if (result == 1)
						{
							int label = 1;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus =model.approverstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.approvername = model.approvedby;
							emailmodel.approverid = model.approverid;
							emailmodel.gatepassid = model.gatepassid;
							emailmodel.approverstatus = model.approverstatus;
							emailmodel.gatepasstype = model.gatepasstype;
							emailmodel.requestedby = model.requestedby;
							emailmodel.requestedon = model.requestedon;

							//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							//emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							if(model.gatepasstype=="Returnable")
                            {
								emailobj.sendEmail(emailmodel, 15,3);
							}
                            else
                            {
								emailobj.sendEmail(emailmodel, 16,10);
							}
							
						}
					}
				}
				else if (model.categoryid == 2)
				{
					updateapproverstatus = WMSResource.updateApprovedstatusbyFMmanager.Replace("#fmapprovedstatus", model.fmapprovedstatus);

					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{

						var result1 = DB.Execute(updateapproverstatus, new
						{
							model.fmapproverremarks,
							model.gatepassid

						});
						if (result1 == 1)
						{
							int label = 2;
							string approvername = model.approvedby;
							string insertgatepasshistory = WMSResource.insertgatepassapprovalhistory;
							string approverstatus = model.fmapprovedstatus;
							var gatepasshistory = DB.ExecuteScalar(insertgatepasshistory, new
							{

								model.approverid,
								approvername,
								model.gatepassid,
								label,
								approverstatus
							});
							EmailModel emailmodel = new EmailModel();
							emailmodel.approvername = model.approvedby;
							emailmodel.approverid = model.approverid;
							emailmodel.gatepassid = model.gatepassid;
							emailmodel.approverstatus = model.approverstatus;
							emailmodel.requestedby = model.requestedby;
							emailmodel.requestedon = model.requestedon;

							//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							//emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 17,3);


						}
					}
				}

				return (Convert.ToInt32(result));
			}
			catch (Exception Ex)
			{
				log.ErrorMessage("PODataProvider", "GatepassapproveByManager", Ex.StackTrace.ToString());
				return 0;
			}
		}

		/*
		Name of Function : <<getSafteyStockList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Saftey Stock List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<safteyStockList>> getSafteyStockList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string query = WMSResource.getSafteyStockList;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<safteyStockList>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getSafteyStockList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}

		}

		/*
		Name of Function : <<GetBinList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Get Bin List>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> GetBinList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string approverlist = WMSResource.getbinlist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<StockModel>(
					  approverlist, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetBinList", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<GetMaterialstockcombo>>  Author :<<Ramesh>>  
		Date of Creation <<15/07/2020>>
		Purpose : <<Get list of materials for stock transfer >>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<Materials>> GetMaterialstockcombo()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "Select distinct st.materialid as material,mat.materialdescription  from wms.wms_stock st left outer join wms.\"MaterialMasterYGS\" mat on mat.material = st.materialid";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<Materials>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "GetMaterialstockcombo", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UpdateStockTransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Updating location for stock>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public string UpdateStockTransfer(List<StockModel> data)
		{


			StockModel obj = new StockModel();
			string loactiontext = string.Empty;

			foreach (StockModel stck in data)
			{
				NpgsqlTransaction Trans = null;
				stocktransferModel transfer = new stocktransferModel();


				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					try
					{
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						StockModel objs = new StockModel();

						string query = "select * from wms.wms_stock where itemid = '" + stck.itemid + "'";
						objs = pgsql.QueryFirstOrDefault<StockModel>(
						   query, null, commandType: CommandType.Text);

						if (objs != null)
						{
							transfer.itemid = stck.itemid;
							transfer.materialid = objs.materialid;
							transfer.previousqty = objs.availableqty;
							transfer.previouslocation = objs.itemlocation;
							transfer.transferedon = System.DateTime.Now;
							transfer.transferedby = stck.createdby;
							transfer.transferedqty = stck.availableqty;
							transfer.currentlocation = stck.itemlocation;
							transfer.remarks = stck.remarks;
							int decavail = objs.availableqty - stck.availableqty;

							string query1 = "UPDATE wms.wms_stock set availableqty=" + decavail + "  where itemid = '" + stck.itemid + "'";
							pgsql.ExecuteScalar(query1);
							StockModel objs1 = new StockModel();
							string query2 = "select * from wms.wms_stock where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.itemlocation + "'";
							objs1 = pgsql.QueryFirstOrDefault<StockModel>(
							   query2, null, commandType: CommandType.Text);
							if (objs1 != null)
							{
								int availqty = objs1.availableqty + stck.availableqty;

								string query4 = "UPDATE wms.wms_stock set availableqty=" + availqty + "  where pono = '" + objs.pono + "' and materialid = '" + objs.materialid + "' and itemlocation = '" + stck.itemlocation + "'";
								pgsql.ExecuteScalar(query4);
								string stockinsertqry = WMSResource.stocktransferinternal;
								var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
								{
									transfer.itemid,
									transfer.materialid,
									transfer.previouslocation,
									transfer.previousqty,
									transfer.currentlocation,
									transfer.transferedqty,
									transfer.transferedon,
									transfer.transferedby,
									transfer.remarks

								});
							}
							else
							{
								string insertqueryx = WMSResource.insertstock;
								stck.createddate = System.DateTime.Now;
								int itemid = 0;
								var result = 0;
								result = Convert.ToInt32(pgsql.ExecuteScalar(insertqueryx, new
								{
									objs.inwmasterid,
									objs.pono,
									stck.binid,
									objs.vendorid,
									objs.totalquantity,
									objs.shelflife,
									stck.availableqty,
									objs.deleteflag,
									//data.itemreceivedfrom,
									stck.itemlocation,
									stck.createddate,
									stck.createdby,
									objs.stockstatus,
									objs.materialid
								}));
								if (result != 0)
								{
									itemid = Convert.ToInt32(result);
									string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
									var results = pgsql.ExecuteScalar(insertqueryforlocationhistory, new
									{
										stck.itemlocation,
										itemid,
										stck.createddate,
										stck.createdby,

									});

									string stockinsertqry = WMSResource.stocktransferinternal;
									var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
									{
										transfer.itemid,
										transfer.materialid,
										transfer.previouslocation,
										transfer.previousqty,
										transfer.currentlocation,
										transfer.transferedqty,
										transfer.transferedon,
										transfer.transferedby,
										transfer.remarks

									});

								}

							}




						}
						Trans.Commit();
					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer", Ex.StackTrace.ToString());
						return null;
					}
				}
			}




			return loactiontext;




		}

		/*
		Name of Function : <<InvStockTransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Inv Stock Transfer>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string InvStockTransfer(invstocktransfermodel data)
		{


			StockModel obj = new StockModel();
			string loactiontext = string.Empty;
			int x = 0;
			invstocktransfermodel transfer = new invstocktransfermodel();
			foreach (stocktransfermateriakmodel stck in data.materialdata)
			{
				NpgsqlTransaction Trans = null;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
					try
					{
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						StockModel objs = new StockModel();
						if (x == 0)
						{

							transfer.transferredby = data.transferredby;
							transfer.transferredon = System.DateTime.Now;
							if (data.sourceplant == data.destinationplant)
							{
								transfer.transfertype = "interstock";
							}
							else
							{
								transfer.transfertype = "intrastock";
							}
							transfer.sourceplant = data.sourceplant;
							transfer.destinationplant = data.destinationplant;
							transfer.remarks = data.remarks;
                            string mainstockinsertqueryy = WMSResource.insertInvStocktransfer;
                            var resultsxx = pgsql.ExecuteScalar(mainstockinsertqueryy, new
                            {
                                transfer.transferredby,
                                transfer.transferredon,
                                transfer.transfertype,
                                transfer.sourceplant,
                                transfer.destinationplant,
                                transfer.remarks

                            });
							transfer.transferid = resultsxx.ToString();

                        }

						string query = "select * from wms.wms_stock where materialid ='"+stck.materialid+"' and itemlocation = '"+stck.sourcelocation+"' and availableqty > 0 order by itemid";

						var stockdata = pgsql.QueryAsync<StockModel>(query, null, commandType: CommandType.Text);
						if (stockdata != null)
						{
							int quantitytotransfer = stck.transferqty;
							int issuedqty = 0;
							foreach (StockModel itm in stockdata.Result)
							{
							
								if (quantitytotransfer <= itm.availableqty)
								{
									issuedqty = quantitytotransfer;
								}
								else
								{
									issuedqty = itm.availableqty;
								}
								string inwmasterid = "";
								int? inwardid = null;
								if (itm.inwmasterid != null && itm.inwmasterid != "")
								{
									inwmasterid = itm.inwmasterid;

								}
								if (itm.inwmasterid != null && itm.inwmasterid != "")
								{
									inwardid = itm.inwardid;

								}

								quantitytotransfer = quantitytotransfer - issuedqty;

								string insertqueryforstatusforqty = WMSResource.updateqtyafterissue.Replace("#itemid", Convert.ToString(itm.itemid)).Replace("#issuedqty", Convert.ToString(issuedqty));
								var data1 = pgsql.ExecuteScalar(insertqueryforstatusforqty);
								StockModel objs1 = new StockModel();
								string query2 = "select * from wms.wms_stock where pono = '" + itm.pono + "' and materialid = '" + itm.materialid + "' and itemlocation = '" + stck.destinationlocation + "' order by itemid";
								objs1 = pgsql.QueryFirstOrDefault<StockModel>(
								   query2, null, commandType: CommandType.Text);
								if (objs1 != null)
								{
									int availqty = objs1.availableqty + issuedqty;

									string query4 = "UPDATE wms.wms_stock set availableqty=" + availqty + "  where itemid = "+ objs1.itemid+ "";
									pgsql.ExecuteScalar(query4);
									string stockinsertqry = WMSResource.insertinvtransfermaterial;
									int sourceitemid = itm.itemid;
									int destinationitemid = objs1.itemid;
									int transferqty = issuedqty;
									var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
									{
										transfer.transferid,
										stck.materialid,
										stck.sourcelocation,
										sourceitemid,
										stck.destinationlocation,
										destinationitemid,
										transferqty

									});
								}
								else
								{
									string insertqueryx = WMSResource.insertstock;
									DateTime createddate = System.DateTime.Now;
									
									int? binid = null;
									int? rackid = null;
									int? storeid = null;
									if(stck.binid > 0)
                                    {
										binid = stck.binid;
                                    }
									if (stck.rackid > 0)
									{
										rackid = stck.rackid;
									}
									if (stck.storeid > 0)
									{
										storeid = stck.storeid;
									}
									//int availableqty = stck.transferqty;
									int availableqty = issuedqty;
									string itemlocation = stck.destinationlocation;
									string createdby = transfer.transferredby;
									string stocktype = itm.stocktype;
									int itemid = 0;
									var result = 0;
									result = Convert.ToInt32(pgsql.ExecuteScalar(insertqueryx, new
									{
										inwmasterid,
										itm.pono,
										binid,
										rackid,
										storeid,
										itm.vendorid,
										itm.totalquantity,
										itm.shelflife,
										availableqty,
										itm.deleteflag,
										itemlocation,
										createddate,
										createdby,
										itm.stockstatus,
										stck.materialid,
										inwardid,
										stocktype
									}));
									if (result != 0)
									{
										itemid = Convert.ToInt32(result);
										string insertqueryforlocationhistory = WMSResource.insertqueryforlocationhistory;
										var results = pgsql.ExecuteScalar(insertqueryforlocationhistory, new
										{
											itemlocation,
											itemid,
											createddate,
											createdby,

										});
										int sourceitemid = itm.itemid;
										int destinationitemid = result;
										string stockinsertqry = WMSResource.insertinvtransfermaterial;
										stck.destinationitemid = itemid;
										int transferqty = issuedqty;
										var resultsxx = pgsql.ExecuteScalar(stockinsertqry, new
										{
											transfer.transferid,
											stck.materialid,
											stck.sourcelocation,
											sourceitemid,
											stck.destinationlocation,
											destinationitemid,
											transferqty
										});

									}

								}
								if (quantitytotransfer <= 0)
								{
									break;
								}


							}
						}

						//if (objs != null)
						//{
							

						//	int decavail = objs.availableqty - stck.transferqty;
						//	int? inwmasterid = null;
						//	int? inwardid = null;
						//	if (objs.inwmasterid > 0)
						//	{
						//		inwmasterid = objs.inwmasterid;

						//	}
						//	if (objs.inwmasterid > 0)
						//	{
						//		inwardid = objs.inwardid;

						//	}

						//	string query1 = "UPDATE wms.wms_stock set availableqty=" + decavail + "  where itemid = '" + stck.sourceitemid + "'";
						//	pgsql.ExecuteScalar(query1);
							

						//}
						//else
						//{
						//	Trans.Rollback();
						//	return null;
						//}
						Trans.Commit();
						x++;

					}
					catch (Exception Ex)
					{
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateStockTransfer1", Ex.StackTrace.ToString());
						return null;
					}
				}
			}




			return loactiontext;




		}

		/*
		Name of Function : <<getstocktransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock transfer data>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<stocktransferModel>> getstocktransferdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getinternalstocktransferdata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<stocktransferModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getstocktransferdatagroup>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock transfer data group>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<stocktransferModel>> getstocktransferdatagroup()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.Stocktransferbygroup;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<stocktransferModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getstocktransferdatagroup1>>  Author :<<Ramesh>>  
		Date of Creation <<18/07/2020>>
		Purpose : <<get stock transferdata>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<invstocktransfermodel>> getstocktransferdatagroup1()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.invstocktransfermainquery;

					await pgsql.OpenAsync();
					var result = await pgsql.QueryAsync<invstocktransfermodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					if (result != null & result.Count() > 0)
					{
						foreach (invstocktransfermodel dt in result)
						{
							var matqry = WMSResource.getinvtransfermaterialdetail.Replace("#tid", dt.transferid);
							var result1 = await pgsql.QueryAsync<stocktransfermateriakmodel>(
								  matqry, null, commandType: CommandType.Text);
							if (result1 != null & result1.Count() > 0)
							{
								dt.materialdata = result1.ToList();
							}
						}

					}
					return result;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktransferdatagroup1", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptance>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptance()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getgrnlistdata;

					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);
					//List<ddlmodel> senddata = new List<ddlmodel>();
					//foreach(ddlmodel grn in data)
					//               {
					//	var qry = "select inwardid as value,inwmasterid as text from wms.wms_storeinward where qualitycheckrequired = True and (qualitychecked = False or qualitychecked is null) and inwmasterid = " + grn.value + "";
					//	var data1 = await pgsql.QueryAsync<ddlmodel>(
					//     qry, null, commandType: CommandType.Text);
					//	if(data1 == null || data1.Count() == 0)
					//                   {
					//		ddlmodel md = new ddlmodel();
					//		md.value = grn.text;
					//		md.text = grn.text;
					//		senddata.Add(md);
					//                   }
					//}
					return data.OrderByDescending(o => o.value);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptance", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance put away>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceputaway()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getgrnlistdataforputaway;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					foreach (ddlmodel ddl in data)
					{
						var exixtedrow = returnlist.Where(o => o.text == ddl.text).FirstOrDefault();
						if (exixtedrow == null)
						{
							returnlist.Add(ddl);

						}

					}
					return returnlist.OrderByDescending(o => o.value);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptancenotify>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance notify>>
		<param name="type"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getgrnlistforacceptancenotify(string type)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "";
					if (type == "Pending")
					{
						materialrequestquery = WMSResource.grnlistfornotify;
					}
					else if (type == "Sent")
					{
						materialrequestquery = WMSResource.getnotifiedgrnlist;
					}
					else
					{
						materialrequestquery = WMSResource.grnlistfornotify;
					}

					List<inwardModel> returnlist = new List<inwardModel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<inwardModel>(
					  materialrequestquery, null, commandType: CommandType.Text);




					foreach (inwardModel ddl in data)
					{
						string validatequery = WMSResource.validategrnlistfornotify.Replace("#inwmasterid", ddl.inwmasterid.ToString());
						var datax = await pgsql.QueryAsync<ddlmodel>(
									validatequery, null, commandType: CommandType.Text);
						if (datax == null || datax.Count() == 0)
						{
							returnlist.Add(ddl);
						}


					}
					return returnlist.OrderByDescending(o => o.inwmasterid);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceputaway", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getholdgrlist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get hold gr list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getholdgrlist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getHoldGRList;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getholdgrlist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceqc>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<getgrnlistforacceptanceqc>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqc()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getqualitycheckdropdown;
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getgrnlistforacceptanceqcbydate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get grn list for acceptance qc by date>>
		<param name="fromdt"></param>
		 <param name="todt"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getgrnlistforacceptanceqcbydate(string fromdt, string todt)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getqcdropdownbydate.Replace("#fromdt", fromdt).Replace("#todate", todt);
					List<ddlmodel> returnlist = new List<ddlmodel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					return data;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getgrnlistforacceptanceqc", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getnotifiedgrbydate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get notified gr by date>>
		<param name="fromdt"></param>
		 <param name="todt"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<inwardModel>> getnotifiedgrbydate(string fromdt, string todt)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "";


					materialrequestquery = WMSResource.getnotifiedgrbydate.Replace("#fromdate", fromdt).Replace("#todate", todt);
					List<inwardModel> returnlist = new List<inwardModel>();
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<inwardModel>(
					  materialrequestquery, null, commandType: CommandType.Text);




					foreach (inwardModel ddl in data)
					{
						string validatequery = WMSResource.validategrnlistfornotify.Replace("#inwmasterid", ddl.inwmasterid.ToString());
						var datax = await pgsql.QueryAsync<ddlmodel>(
									validatequery, null, commandType: CommandType.Text);
						if (datax == null || datax.Count() == 0)
						{
							returnlist.Add(ddl);
						}


					}
					return returnlist.OrderByDescending(o => o.inwmasterid);


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getnotifiedgrbydate", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<pendingreceiptslist>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<pending receipts list>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> pendingreceiptslist()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getpendingreceiptslist;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "pendingreceiptslist", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UnholdGRdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Unhold GR data>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UnholdGRdata(UnholdGRModel datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();
					string status = datamodel.unholdaction == true ? "accepted" : "returned";
					string qry = "Update wms.wms_securityinward set onhold = False,holdgrstatus='" + status + "',unholdedby = '" + datamodel.unholdedby + "',unholdedon = current_date,unholdremarks = '" + datamodel.unholdremarks + "' where inwmasterid = '" + datamodel.inwmasterid + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = 1;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}
		//current
		/*
		Name of Function : <<mattransfer>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mat transfer>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mattransfer(materialtransferMain datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{
					int transferqty = 0;
					string createdby = datamodel.transferedby;
					string remarks = datamodel.transferremarks;
					string materialid = "";
					string updatereturnqty = "";
					string fromprojectcode = datamodel.projectcodefrom;
					string mailto = "";

					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					updatereturnqty = WMSResource.updatetransferdata;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						var resultx = DB.ExecuteScalar(updatereturnqty, new
						{
							transferqty,
							createdby,
							remarks,
							datamodel.projectcode,
							materialid,
							datamodel.approvallevel,
							datamodel.finalapprovallevel,
							fromprojectcode
						});
						string tid = resultx.ToString();
						if (tid != "")
						{
							datamodel.transferid = tid;


							string rsltqry = WMSResource.inserttransferapproval;
							if(datamodel.finalapprovallevel > 1)
                            {
								for (int i = 2; i <= datamodel.finalapprovallevel; i++)
                                {
									if(i == 2)
                                    {
										string userquery = "select  * from wms.employee where employeeno='" + datamodel.projectmanagerfrom + "'";
										User userdata = pgsql.QuerySingle<User>(
										   userquery, null, commandType: CommandType.Text);
										mailto = userdata.email;
										if (datamodel != null)
										{
											string approverid = datamodel.projectmanagerfrom;
											string approvername = userdata.name;
											string approveremail = userdata.email;
											int approvallevel = 1;
											var resultxyy = DB.ExecuteScalar(rsltqry, new
											{
												datamodel.transferid,
												approverid,
												approvername,
												approveremail,
												approvallevel

											});

										}

									}
									if (i == 3)
									{
										string userquery = "select  * from wms.employee where employeeno='" + datamodel.projectmanagerto + "'";
										User userdata = pgsql.QuerySingle<User>(
										   userquery, null, commandType: CommandType.Text);
										if (datamodel != null)
										{
											string approverid = datamodel.projectmanagerto;
											string approvername = userdata.name;
											string approveremail = userdata.email;
											int approvallevel = 2;
											var resultxyy = DB.ExecuteScalar(rsltqry, new
											{
												datamodel.transferid,
												approverid,
												approvername,
												approveremail,
												approvallevel

											});

										}

									}

								}
								
								

							}
							
							
							foreach (materialtransferTR trdata in datamodel.materialdata)
							{
								string insertqrry = WMSResource.inserttransfermaterials;

								var result11 = DB.Execute(insertqrry, new
								{

									datamodel.transferid,
									trdata.materialid,
									trdata.transferredqty

								});
							}
							result = 1;
							Trans.Commit();
							EmailModel emailmodel = new EmailModel();
							emailmodel.transferid = datamodel.transferid.ToString();
							emailmodel.transferbody = "Material Transfer request initiated for approval with Transferid :MATFR" + datamodel.transferid.ToString();
							emailmodel.ToEmailId = "developer1@in.yokogawa.com";
							//emailmodel.ToEmailId = mailto;
							emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							emailmodel.CC = "sushma.patil@in.yokogawa.com";
							EmailUtilities emailobj = new EmailUtilities();
							emailobj.sendEmail(emailmodel, 14);
						}

					}



				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}


		/*
		Name of Function : <<mattransferapprove>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mat transfer approve>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mattransferapprove(List<materialtransferMain> datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{

					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					
					List<EmailModel> emailmodels = new List<EmailModel>();
					foreach(materialtransferMain data in datamodel)
                    {
						string mailto = "";
						DateTime todayDate = DateTime.Now;
						string currentdatestr = todayDate.ToString("yyyy-MM-dd");
						if (data.isapproved)
                        {
                            string query = "update wms.wms_materialtransferapproval set approvaldate ='" + currentdatestr + "'";
                            query += " ,isapproved = " + data.isapproved + ", remarks = '"+data.approvalremarks+"'  where transferid = '"+data.transferid+"' and approverid = '"+data.approverid + "'";

							var rslt = pgsql.ExecuteScalar(query);
							int nextlevel = data.approvallevel + 1;
							

							string query1 = "update wms.wms_transfermaterial set approvallevel = " + nextlevel + " where transferid = '" + data.transferid + "'";
							var rslt1 = pgsql.ExecuteScalar(query1);

							string userquery = "select approveremail from wms.wms_materialtransferapproval where approvallevel = "+ nextlevel + "";
							var nextmailobj = pgsql.Query<materialtransferapproverModel>(
					        userquery, null, commandType: CommandType.Text).ToList();
							if(nextmailobj!= null && nextmailobj.Count()>0)
                            {
								mailto = nextmailobj[0].approveremail;
								EmailModel emailmodel1 = new EmailModel();
								emailmodel1.transferid = "MATFR" + data.transferid.ToString();
								emailmodel1.transferbody = "Material Transfer request initiated for approval with Transferid :MATFR" + data.transferid.ToString();
								emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
								//emailmodel.ToEmailId = mailto;
								emailmodel1.FrmEmailId = "developer1@in.yokogawa.com";
								emailmodel1.CC = "sushma.patil@in.yokogawa.com";
								emailmodels.Add(emailmodel1);
								

							}
                            else
                            {

								mailto = data.requesteremail;
								EmailModel emailmodel1 = new EmailModel();
								emailmodel1.transferid = "MATFR" + data.transferid.ToString();
								emailmodel1.transferbody = "Material Transfer request approved with Transferid :MATFR" + data.transferid.ToString();
								emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
								//emailmodel.ToEmailId = mailto;
								emailmodel1.FrmEmailId = "developer1@in.yokogawa.com";
								emailmodel1.CC = "sushma.patil@in.yokogawa.com";
								emailmodels.Add(emailmodel1);
							}

						}
						else if (!data.isapproved)
						{
							string query = "update wms.wms_materialtransferapproval set approvaldate ='" + currentdatestr + "'";
							query += " ,isapproved = " + data.isapproved + ", remarks = '" + data.approvalremarks + "'  where transferid = " + data.transferid + " and approverid = '" + data.approverid + "'";

							var rslt = pgsql.ExecuteScalar(query);
					


							string query1 = "update wms.wms_transfermaterial set approvallevel = 5 where transferid = " + data.transferid + "";
							var rslt1 = pgsql.ExecuteScalar(query1);
							mailto = data.requesteremail;
							EmailModel emailmodel1 = new EmailModel();
							emailmodel1.transferid = data.transferid.ToString();
							emailmodel1.transferbody = "Material Transfer request rejected with Transferid :MATFR" + data.transferid.ToString();
							emailmodel1.ToEmailId = "developer1@in.yokogawa.com";
							//emailmodel.ToEmailId = mailto;
							emailmodel1.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
							emailmodel1.CC = "sushma.patil@in.yokogawa.com";
							emailmodels.Add(emailmodel1);

						}
					}
					
							result = 1;
							Trans.Commit();
					foreach(EmailModel mdl in emailmodels)
                    {
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(mdl, 14);
					}
				




				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "mattransferapprove", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<mrnupdate>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<mrn update>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int mrnupdate(MRNsavemodel datamodel)
		{
			int result = 0;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{


					pgsql.OpenAsync();
					string qry = "Update wms.wms_securityinward set isdirecttransferred = True,projectcode='" + datamodel.projectcode + "',mrnby = '" + datamodel.directtransferredby + "',mrnon = current_date,mrnremarks = '" + datamodel.mrnremarks + "' where grnnumber = '" + datamodel.grnnumber + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = 1;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "mrnupdate", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<updateonholdrow>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update onhold row>>
		<param name="datamodel"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<string> updateonholdrow(updateonhold datamodel)
		{
			string result = "";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{

					string pono = datamodel.invoiceno.Split('-')[0];
					string invno = datamodel.invoiceno.Split('-')[1];
					await pgsql.OpenAsync();
					string qry = "Update wms.wms_securityinward set onhold = " + datamodel.onhold + ",onholdremarks = '" + datamodel.remarks + "' where pono = '" + pono + "' and invoiceno = '" + invno + "'";
					var results11 = pgsql.ExecuteScalar(qry);
					result = "Saved";

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
			return result;
		}

		/*
		Name of Function : <<getdepartmentmasterdata>>  Author :<<Ramesh>>  
		Date of Creation <<28/07/2020>>
		Purpose : <<get organasation dropdown>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<ddlmodel>> getdepartmentmasterdata()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getdepartmentmasterdata;

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<ddlmodel>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getdepartmentmasterdata", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getrbadetails>>  Author :<<Ramesh>>  
		Date of Creation <<28/07/2020>>
		Purpose : <<get rba details>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<rbamaster>> getrbadetails()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = "select * from wms.wms_rbamaster";

					await pgsql.OpenAsync();
					return await pgsql.QueryAsync<rbamaster>(
					  materialrequestquery, null, commandType: CommandType.Text);

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getrbadetails", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<insertdatacsv>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<insert data csv>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int insertdatacsv(ddlmodel obj)
		{
			ddlmodel mdl = new ddlmodel();

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
                try
                {
					string materialrequestquery = "select material as value,materialdescription as text from  wms.\"MaterialMasterYGS\"";
					var datalist = pgsql.Query<ddlmodel>(
					materialrequestquery, null, commandType: CommandType.Text);
					var filePath = @"D:\A_StagingTable\StockstagecsvTest1.xlsx";
					//var filePath = "D:\A_StagingTable\StockstagecsvTest1.xlsx";
					foreach (ddlmodel ddl in datalist)
					{




						Random rnd = new Random();
						int str = rnd.Next(1, 3);
						int rk = rnd.Next(1, 6);
						int bn = rnd.Next(1, 6);
						int qty = rnd.Next(50, 500);
						string store = "store" + str.ToString();
						string rack = "a" + rk.ToString();
						string bin = "bin" + bn.ToString();
						//making query    
						string query = "INSERT INTO wms.testcsv (material, material_description, store,rack, bin,quantity,grn,received_date,shelf_life_expiration,date_of_Manufacture,datasource,data_entered_by,data_entered_on) ";
						query += " VALUES('" + ddl.value + "','" + ddl.text + "','" + store + "','" + rack + "','" + bin + "'," + qty+ ",'" + string.Empty + "','22/09/2020','22/12/2020','22/05/2020','" + string.Empty + "','303268','22/05/2020')";
						var resultsx = pgsql.ExecuteScalar(query);
					}

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "insertdatacsv", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}
				


			}


			return 1;


		}


		/*
		Name of Function : <<requesttoreserve>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<request to reserve>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int requesttoreserve(materialReservetorequestModel obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				NpgsqlTransaction Trans = null;
				try
				{


					pgsql.Open();
					Trans = pgsql.BeginTransaction();
					string query = "update wms.materialreserve set requestedby = '" + obj.requestedby + "', requestedon = current_date  where reserveid = '" + obj.reserveid + "'";
					var results11 = pgsql.ExecuteScalar(query);

					string materialrequestquery = WMSResource.getreservedatabyid.Replace("#reserveid",obj.reserveid.ToString());
					var datalist = pgsql.Query<ReserveMaterialModel>(
					  materialrequestquery, null, commandType: CommandType.Text);

					
					List<IssueRequestModel> reqdata = new List<IssueRequestModel>();
					foreach (ReserveMaterialModel rv in datalist)
					{
						IssueRequestModel model = new IssueRequestModel();
						model.quantity = rv.reservedqty;
						model.requesteddate = System.DateTime.Now;
						model.approveremailid = null;
						model.approverid = null;
						model.pono = rv.pono;
						model.material = rv.materialid;
						model.requesterid = obj.requestedby;
						model.requestedquantity = 0;
						model.projectcode = rv.projectcode;
						model.calltype = "fromreserve";
						model.reserveid = obj.reserveid;
						reqdata.Add(model);

					}
					int saverequest = updaterequestedqty(reqdata);
					if (saverequest == 0)
					{
						Trans.Rollback();
						return 0;
					}
					


					Trans.Commit();
					return 1;


				}
				catch (Exception Ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}

		/*
		Name of Function : <<updatematmovement>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update mat movement>>
		<param name="obj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int updatematmovement(List<materialistModel> obj)
		{

			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.OpenAsync();

					foreach (materialistModel mat in obj)
					{
						//gatepassid integer references wms.wms_gatepass(gatepassid) not null,
						//gatepassmaterialid integer references wms.wms_gatepassmaterial(gatepassmaterialid) not null,
						//outwarddate TIMESTAMP without time zone null,
						//outwardby varchar(50) null,
						//outwardremarks text null,
						//outwardqty integer null,
						//inwarddate TIMESTAMP without time zone null,
						//inwardby varchar(50) null,
						//intwardremarks text null,
						//inwardqty integer null
						int results11 = 0;
						string query = "";
						int gatepassmaterialid = Convert.ToInt32(mat.gatepassmaterialid);
						if (mat.movetype == "out")
						{
							query = "insert into wms.outwatdinward(gatepassid, gatepassmaterialid, outwarddate, outwardby, outwardremarks, outwardqty)";
							query += " values('"+mat.gatepassid+"', "+gatepassmaterialid+", '"+mat.outwarddatestring+"', '"+mat.movedby+"', '"+mat.remarks+"', "+mat.outwardqty+")";
							var resultsx = pgsql.ExecuteScalar(query);
							//query = WMSResource.outwardinsertquery;
							//results11 = pgsql.Execute(query, new
							//{
							//	mat.gatepassid,
							//	gatepassmaterialid,
							//	mat.outwarddatestring,
							//	mat.movedby,
							//	mat.remarks,
							//	mat.outwardqty
							//});

							//query = "update wms.wms_gatepassmaterial set outwardqty = " + mat.outwardqty + ", outwarddate = '" + mat.outwarddatestring + "' , outwardedby='" + mat.movedby + "',outwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
						}
						
						else if(mat.movetype == "in")
						{
							query = "insert into wms.outwatdinward(gatepassid, gatepassmaterialid, securityinwardby, securityinwarddate, securityinwardremarks)";
							query += " values('" + mat.gatepassid + "', " + gatepassmaterialid + ",'" + mat.movedby + "', '" + mat.inwarddatestring + "', '" + mat.remarks + "')";
							var resultsx = pgsql.ExecuteScalar(query);
							//query = "update wms.wms_gatepassmaterial set inwardqty = " + mat.inwardqty + ", inwarddate = '" + mat.inwarddatestring + "' , inwardedby='" + mat.movedby + "',inwardremarks='" + mat.remarks + "' where gatepassmaterialid = " + mat.gatepassmaterialid + "";
							//results11 = pgsql.ExecuteScalar(query);
							//query = WMSResource.inwardinsertquery;
							//results11 = pgsql.Execute(query, new
							//{
							//	mat.gatepassid,
							//	gatepassmaterialid,
							//	mat.movedby,
							//	mat.remarks,
							//	mat.inwardqty
							//});
						}
                        else
                        {
                            query = WMSResource.inwardinsertquery;
                            results11 = pgsql.Execute(query, new
                            {
                                mat.gatepassid,
                                gatepassmaterialid,
                                mat.movedby,
                                mat.remarks,
                                mat.inwardqty
                            });

                        }

					}

					return 1;


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "updateonholdrow", Ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}



			}
		}


		/*
		Name of Function : <<getstocktype>>  Author :<<Ramesh>>  
		Date of Creation <<22/07/2020>>
		Purpose : <<get stock type>>
		<param name="locdetails"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string getstocktype(locataionDetailsStock locdetails)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string materialrequestquery = WMSResource.getpendingreceiptslist;

					pgsql.OpenAsync();
					string query = WMSResource.getstocktype.Replace("#locationid", locdetails.locationid).Replace("#locationname", locdetails.locationname).Replace("#stid", Convert.ToString(locdetails.storeid)).Replace("#rkid", Convert.ToString(locdetails.rackid)).Replace("#biid", Convert.ToString(locdetails.binid));
					string stocktype = pgsql.QueryFirstOrDefault<string>(
					   query, null, commandType: CommandType.Text);
					return stocktype;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getstocktype", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<UpdateMaterialReserve>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Update Material Reserve>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateMaterialReserve()
		{
			int result = 0;
			List<ReserveMaterialModel> _listobj = new List<ReserveMaterialModel>();
			EmailUtilities emailobj = new EmailUtilities();
			EmailModel emailmodel = new EmailModel();
			emailmodel.FrmEmailId = "developer1@in.yokogawa.com";
			emailmodel.CC = "sushma.patil@in.yokogawa.com";
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.Open();
					string query = WMSResource.Getreservelist;
					string updatequery = string.Empty;
					string updatedon = WMSResource.updatedon;
					_listobj = pgsql.Query<ReserveMaterialModel>(
					  query, null, commandType: CommandType.Text).ToList();
					if (_listobj.Count != 0)
					{
						foreach (var items in _listobj)
						{
							var dateAndTime = DateTime.Now;
							var date = dateAndTime.Date;
							var reservedate = items.reserveupto.Date;
							if (reservedate == date)
							{
								updatequery = WMSResource.updatetostockreserveqty.Replace("@reservedqty", Convert.ToString(items.reservedqty)).Replace("@itemid", Convert.ToString(items.itemid));
								result = pgsql.Execute(updatequery, new
								{

								});
								updatedon = WMSResource.updatedon.Replace("@reserveformaterialid", Convert.ToString(items.reserveformaterialid));
								result = pgsql.Execute(updatedon, new
								{

								});
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 10);
							}
							else if (reservedate == date.AddDays(1))
							{
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 11);
							}
							else if (reservedate == date.AddDays(2))
							{
								emailmodel.ToEmailId = items.email;
								emailobj.sendEmail(emailmodel, 12);
							}
						}
					}
				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "UpdateMaterialReserve", ex.StackTrace.ToString());
					return 0;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return result;


		}

		/*
		Name of Function : <<getstockdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get stock details>>
		<param name="materialid"></param>
		 <param name="pono"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public stockCardPrint getstockdetails(string pono, string materialid)
		{
			stockCardPrint objstock = new stockCardPrint();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					pgsql.Open();
					string query = WMSResource.GetStockDetails.Replace("#pono", pono).Replace("#materialid", materialid);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					objstock = pgsql.QuerySingle<stockCardPrint>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "getstockdetails", ex.StackTrace.ToString());
					return objstock;
				}
				finally
				{
					pgsql.Close();
				}

			}

			return objstock;

		}/*
		Name of Function : <<UpdateReturnqty_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting the data to retrunmaterial by pm>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnqty_old(List<IssueRequestModel> _listobj)
		{
			int result = 0;
			int requestid = 0;
			if (_listobj.Count != 0)
			{
				try
				{
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						IssueRequestModel obj = new IssueRequestModel();
						DB.Open();
						string query = WMSResource.getnxtreturnid;
						obj = DB.QueryFirstOrDefault<IssueRequestModel>(
						   query, null, commandType: CommandType.Text);
						if (obj != null)
							requestid = obj.matreturnid + 1;
						else
							requestid = 1;

						string updatereturnqty = string.Empty;
						foreach (var item in _listobj)
						{
							item.matreturnid = requestid;

							if (item.returnqty != 0)
							{
								item.materialid = item.material;
								updatereturnqty = WMSResource.UpdateReturnqty;

								result = DB.Execute(updatereturnqty, new
								{
									item.materialissueid,
									item.requestforissueid,
									item.returnqty,
									item.createdby,
									item.requesttype,
									item.transferqty,
									item.requestid,
									item.remarks,
									item.materialid,
									item.matreturnid
								});
							}
						}
					}
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "UpdateReturnqty", ex.StackTrace.ToString());
					return 0;
				}

			}

			return result;
		}

		/*
		Name of Function : <<UpdateReturnqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<inserting the data to retrunmaterial by pm>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnqty(List<IssueRequestModel> _listobj)
		{
			int result = 0;
			if (_listobj.Count != 0)
			{
				NpgsqlTransaction Trans = null;
				try
				{
					using (var DB = new NpgsqlConnection(config.PostgresConnectionString))
					{



						IssueRequestModel obj = new IssueRequestModel();
						DB.Open();
						Trans = DB.BeginTransaction();
						string query = WMSResource.getnxtreturnid;
						string createdby = _listobj[0].createdby;
						string mainquery = WMSResource.insertmaterialreturnquery;
						var rslt = DB.ExecuteScalar(mainquery, new
						{
							createdby
						});
						if (rslt != null)
						{
							var returnid = rslt.ToString();
							string updatereturnqty = string.Empty;
							foreach (var item in _listobj)
							{
								string id = Guid.NewGuid().ToString();


								if (item.returnqty != 0)
								{
									item.materialid = item.material;
									updatereturnqty = WMSResource.insertmaterialreturndetails;

									result = DB.Execute(updatereturnqty, new
									{
										id,
										returnid,
										item.materialid,
										item.returnqty,
										item.remarks


									});
								}
							}

							Trans.Commit();
						}
						else
						{
							Trans.Rollback();
						}
					}
				}
				catch (Exception ex)
				{
					Trans.Rollback();
					log.ErrorMessage("PODataProvider", "UpdateReturnqty", ex.StackTrace.ToString());
					return 0;
				}

			}

			return result;
		}

		/*
		Name of Function : <<UpdateReturnmaterialTostock>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<Update Returnmaterial To stock>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int UpdateReturnmaterialTostock(List<IssueRequestModel> model)
		{
			int result = 0;
			if (model.Count > 0)
			{
				NpgsqlTransaction Trans = null;
				using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
				{
                    try
                    {
						pgsql.Open();
						Trans = pgsql.BeginTransaction();
						string updatereturnqtytomaterialissue = WMSResource.updatereturnqtyByInvMngr.Replace("@returnid", Convert.ToString(model[0].returnid));
						result = pgsql.Execute(updatereturnqtytomaterialissue, new
						{

						});
						if(result != 0)
                        {
							foreach(var item in model)
                            {
								int availableqty = item.confirmqty;
								string materialid = item.material;
								string createdby = item.createdby;
								string returnid = item.id;
								int? binid = null;
								if (item.binid != 0)
								{
									binid = item.binid;

								}

								//string updatereturnqtytostock = WMSResource.updatereturnmaterialToStock.Replace("@availableqty", Convert.ToString(item.returnqty)).Replace("@itemid", Convert.ToString(item.itemid));
								string updatereturnqtytostock = WMSResource.updatetostockbyinvmanger;
								
								  result = pgsql.Execute(updatereturnqtytostock, new
									{

										materialid,
										item.itemlocation,
										availableqty,
										createdby,
										returnid,
										item.stocktype,
										item.storeid,
										item.rackid,
										binid
									});
								
							}
                        }

						Trans.Commit();
						
					}
					catch(Exception ex)
                    {
						Trans.Rollback();
						log.ErrorMessage("PODataProvider", "UpdateReturnmaterialTostock", ex.StackTrace.ToString());
						return 0;

					}
                    finally
                    {
						pgsql.Close();
					}

					
					
				}

			 //   foreach (var item in model)
				//  {
				//	try
				//	{
    //                    string updatereturnqtytomaterialissue = WMSResource.updatereturnqtyByInvMngr.Replace("@returnid", Convert.ToString(item.returnid));
    //                    using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
    //                    {
    //                        result = DB.Execute(updatereturnqtytomaterialissue, new
    //                        {

    //                        });
    //                    }
    //                    if (result != 0)
    //                    {
    //                        int availableqty = item.confirmqty;
				//			string materialid = item.material;
				//			string createdby = item.createdby;
				//		    string returnid = item.id;
				//			int? binid = null;
				//			if(item.binid != 0)
    //                        {
				//				binid = item.binid;

				//			}
							
				//			//string updatereturnqtytostock = WMSResource.updatereturnmaterialToStock.Replace("@availableqty", Convert.ToString(item.returnqty)).Replace("@itemid", Convert.ToString(item.itemid));
				//			string updatereturnqtytostock = WMSResource.updatetostockbyinvmanger;
				//			using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				//			{
				//				result = DB.Execute(updatereturnqtytostock, new
				//				{

				//					materialid,
				//					item.itemlocation,
				//					availableqty,
				//					createdby,
				//					returnid,
				//					item.stocktype,
				//					item.storeid,
				//					item.rackid,
				//					binid
				//				});
				//			}
				//		}
				//	}
				//	catch (Exception ex)
				//	{
				//		log.ErrorMessage("PODataProvider", "UpdateReturnmaterialTostock", ex.StackTrace.ToString());
				//		return 0;
				//	}
				//}
			}
			return result;
		}

		/*
		Name of Function : <<updateputawayfilename>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update putaway filename>>
		<param name="file"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateputawayfilename(ddlmodel file)
		{
			string result = "";
			int rslt = 0;
			try
			{


				string grn = file.text;
				string filename = file.value;
				string updateqry = "update wms.wms_securityinward set putawayfilename = '" + filename + "' where grnnumber = '" + grn + "'";
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(updateqry);
					if (rslt != 0)
					{
						result = "saved";
					}
					else
					{
						result = "error";
					}
				}

			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "updateputawayfilename", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<notifyputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<notify put away>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string notifyputaway(notifymodel data)
		{
			string result = "";
			int rslt = 0;
			try
			{


				string grn = data.grnnumber;
				string remarks = data.notifyremarks;
				string notifyby = data.notifiedby;
				//inw.notifyremarks,inw.notifiedby,inw.notifiedtofinance,inw.notifiedon,inw.putawayfilename,
				string updateqry = "update wms.wms_securityinward set notifiedtofinance = True,notifiedon = current_date,notifiedby='" + notifyby + "', notifyremarks = '" + remarks + "' where grnnumber = '" + grn + "'";
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(updateqry);
					if (rslt != 0)
					{
						result = "saved";
						EmailModel emailmodel = new EmailModel();
						emailmodel.jobcode = data.grnnumber;
						//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
						emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
						//emailmodel.CC = "sushma.patil@in.yokogawa.com";
						EmailUtilities emailobj = new EmailUtilities();
						emailobj.sendEmail(emailmodel, 13,10);


					}
					else
					{
						result = "error";
					}
				}

			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<notifymultipleputaway>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<notify multiple putaway>>
		<param name="datalst"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string notifymultipleputaway(List<notifymodel> datalst)
		{
			string result = "";
			int rslt = 0;
			try
			{
				string grns = "";
				int loop = 1;

				foreach (notifymodel data in datalst)
				{
					string grn = data.grnnumber;
					string remarks = data.notifyremarks;
					string notifyby = data.notifiedby;
					//inw.notifyremarks,inw.notifiedby,inw.notifiedtofinance,inw.notifiedon,inw.putawayfilename,
					string updateqry = "update wms.wms_securityinward set notifiedtofinance = True,notifiedon = current_date,notifiedby='" + notifyby + "', notifyremarks = '" + remarks + "' where grnnumber = '" + grn + "'";
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(updateqry);
						if (rslt != 0)
						{
							result = "saved";
							if (loop > 1)
							{
								grns += ", ";
							}
							grns += grn;

						}
						else
						{
							result = "error";
						}
					}

				}

				EmailModel emailmodel = new EmailModel();
				emailmodel.jobcode = grns;
				//emailmodel.ToEmailId = "developer1@in.yokogawa.com";
				emailmodel.FrmEmailId = "ramesh.kumar@in.yokogawa.com";
				//emailmodel.CC = "sushma.patil@in.yokogawa.com";
				EmailUtilities emailobj = new EmailUtilities();
				emailobj.sendEmail(emailmodel, 13,10);



			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<GetReturnmaterialList>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << onload to display the returned already by PM>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialList()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.GetreturnList_v1;
					string updatequery = string.Empty;
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialList", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}

		/*
		Name of Function : <<GetReturnmaterialListForConfirm>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<based on request/return id we will get details for confirm>>
		<param name="requestid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> GetReturnmaterialListForConfirm(string requestid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getlistforconfirm.Replace("#returnid", requestid);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					return await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "GetReturnmaterialListForConfirm", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}


		}

		/*
		Name of Function : <<getreturndata_old>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<getreturn data by empno>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getreturndata_old(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getreturndata.Replace("#createdby", empno); ;
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					return await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "getreturndata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getreturndata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get return data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialReturn>> getreturndata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getmaterialreturnquery.Replace("#createdby", empno); 
					var data = await pgsql.QueryAsync<MaterialReturn>(
					  query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (MaterialReturn mat in data)
						{
							string query1 = WMSResource.getmaterialreturndetailquery.Replace("#returnid", mat.returnid); ;
							var data1 = await pgsql.QueryAsync<MaterialReturnTR>(
							  query1, null, commandType: CommandType.Text);
							if (data1 != null && data1.Count() > 0)
							{
								mat.materialdata = data1.ToList();
							}
						}


					}



					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "getreturndata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getmaterialreturnreqList>>  Author :<<Gayathri>>  
		Date of Creation <<14/08/2020>>
		Purpose : <<get materials requested for return data based on material return requested id >>
		<param name="matreturnid"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<IssueRequestModel>> getmaterialreturnreqList(string matreturnid)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					//string query = WMSResource.getmatreturndetails.Replace("@matreid", Convert.ToString(matreturnid));
					string query = WMSResource.getmaterialreturndetails.Replace("#returnid", Convert.ToString(matreturnid));
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<IssueRequestModel>(
					  query, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<gettransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get transferred data based on login id>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialtransferMain>> gettransferdata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.gettransferdata.Replace("#createdby", empno);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<materialtransferMain>(
					   query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (materialtransferMain dt in data)
						{
							string query1 = WMSResource.gettransferiddetail.Replace("#tid", dt.transferid.ToString());
							var datadetail = await pgsql.QueryAsync<materialtransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}

							string query2 = WMSResource.getapproverdatabyid.Replace("#transferid", dt.transferid.ToString());
							var datadetail1 = await pgsql.QueryAsync<materialtransferapproverModel>(
							   query2, null, commandType: CommandType.Text);

							if (datadetail1 != null && datadetail1.Count() > 0)
							{
								dt.approverdata = datadetail1.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<gettransferdataforapproval>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get transferred data based on login id>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialtransferMain>> gettransferdataforapproval(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.gettransferdataforapproval.Replace("#approver", empno);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<materialtransferMain>(
					   query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (materialtransferMain dt in data)
						{
							string query1 = WMSResource.gettransferiddetail.Replace("#tid", dt.transferid.ToString());
							var datadetail = await pgsql.QueryAsync<materialtransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}

							string query2 = WMSResource.getapproverdatabyid.Replace("#transferid", dt.transferid.ToString());
							var datadetail1 = await pgsql.QueryAsync<materialtransferapproverModel>(
							   query2, null, commandType: CommandType.Text);

							if (datadetail1 != null && datadetail1.Count() > 0)
							{
								dt.approverdata = datadetail1.ToList();
							}
						}
					}
					return data.OrderByDescending(o=>o.transferid);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdataforapproval", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getdirecttransferdata>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get direct transferred data>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<DirectTransferMain>> getdirecttransferdata(string empno)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.directtransfermainquery.Replace("#empno", empno);
					string updatequery = string.Empty;
					//string updatedon = WMSResource.updatedon;
					var data = await pgsql.QueryAsync<DirectTransferMain>(
					   query, null, commandType: CommandType.Text);
					if (data != null && data.Count() > 0)
					{
						foreach (DirectTransferMain dt in data)
						{
							string query1 = WMSResource.directtransfertr.Replace("#inw", dt.inwmasterid.ToString());
							var datadetail = await pgsql.QueryAsync<DirectTransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<Updatetransferqty>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : << update/insert transfer material details>>
		<param name="_listobj"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public int Updatetransferqty(List<IssueRequestModel> _listobj)
		{
			int result = 0;
			if (_listobj.Count != 0)
			{
				string updatereturnqty = string.Empty;
				foreach (var item in _listobj)
				{

					try
					{
						if (item.transferqty != 0)
						{
							item.materialid = item.material;
							updatereturnqty = WMSResource.updatetransferdata;
							using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
							{
								result = DB.Execute(updatereturnqty, new
								{

									item.transferqty,
									item.createdby,
									item.remarks,
									item.projectcode,
									item.materialid
								});
							}
						}
					}
					catch (Exception ex)
					{
						log.ErrorMessage("PODataProvider", "UpdateReturnqty", ex.StackTrace.ToString());
						return 0;
					}
				}
			}
			return result;
		}

		/*
		Name of Function : <<checkMatExists>>  Author :<<Gayathri>>  
		Date of Creation <<06/08/2020>>
		Purpose : <<check whether the material exists and generate qrcode for material>>
		<param name="material"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string checkMatExists(string material)
		{
			string returnvalue = null;
			string path = null;
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					//pgsql.OpenAsync();
					string materialrequestquery = WMSResource.checkmatexists.Replace("#materialid", Convert.ToString(material));

					var obj = pgsql.QueryFirstOrDefault<MaterialData>(
				materialrequestquery, null, commandType: CommandType.Text);
					if (obj == null)
					{

						return returnvalue = "No material exists";
					}
					else
					{
						path = Environment.CurrentDirectory + @"\Barcodes\";

						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						//generate barcode for material code and GRN No.
						var content = material;
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
						var bitmap = writer.Write(content);

						// write text and generate a 2-D barcode as a bitmap
						writer
							.Write(content)
							.Save(path + content + ".bmp");

						path = "./Barcodes/" + content + ".bmp";

						return path;
					}


				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "checkMatExists", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}

			}
		}

		/*
		Name of Function : <<getPODetails>>  Author :<<Ramesh>>  
		Date of Creation <<10/08/2020>>
		Purpose : <<Get podetails list>>
		<param name="empno"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<PODetails>> getPODetails(string empno)
		{
			//List<PODetails> objPO = new List<PODetails>();
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string getpoquery = WMSResource.getPODetails.Replace("#manager", empno);

					var objPO = await pgsql.QueryAsync<PODetails>(
					   getpoquery, null, commandType: CommandType.Text);
					//objPO = pgsql.QueryAsync<List<PODetails>>(
					//			getpoquery, null, commandType: CommandType.Text);

					return objPO;
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "getPODetails", ex.StackTrace.ToString());
					return null;
				}
			}
		}

		/*
		Name of Function : <<gettestcrud>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get test crud>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<testcrud>> gettestcrud()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.testcrudget;


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<testcrud>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "gettestcrud", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstock(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockviewdata.Replace("#code",code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstock", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockall(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.getallinitialstockdata.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstock", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Exception>>  Author :<<Ramesh>>  
		Date of Creation <<11-11-2019>>
		Purpose : <<get currently uploaded initial stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockEX(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockExceptions.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "gettestcrud", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get Material in hand>>  Author :<<Ramesh>>  
		Date of Creation <<17-11-2019>>
		Purpose : <<get Materials in stock>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<MaterialinHand>> getmatinhand()
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.inhandmaterial;
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<MaterialinHand>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatinhand", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get Material in hand location>>  Author :<<Ramesh>>  
		Date of Creation <<17-11-2019>>
		Purpose : <<get Materials in stock locations>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<matlocations>> getmatinhandlocation(string material)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.inhandmateriallocation.Replace("#material",material);
					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<matlocations>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;

				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getmatinhandlocation", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Report>>  Author :<<Ramesh>>  
		Date of Creation <<13-11-2019>>
		Purpose : <<get initial stock uploaded by loggedin user>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockReport(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockreport.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data;



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstockReport", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<get initial stock Report Group by every insertion>>  Author :<<Ramesh>>  
		Date of Creation <<13-11-2019>>
		Purpose : <<get initial stock uploaded by loggedin user>>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<StockModel>> getinitialstockReportGroup(string code)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					string testgetquery = WMSResource.initialstockreportgroupby.Replace("#code", code);


					await pgsql.OpenAsync();
					var data = await pgsql.QueryAsync<StockModel>(
					  testgetquery, null, commandType: CommandType.Text);
					return data.OrderByDescending(o=>o.createddate);



				}
				catch (Exception Ex)
				{
					log.ErrorMessage("PODataProvider", "getinitialstockReport", Ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}


			}
		}

		/*
		Name of Function : <<posttestcrud>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<post test crud>>
		<param name="data"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string posttestcrud(testcrud data)
		{
			string result = "";
			int rslt = 0;
			try
			{


				if (data.id == 0)
				{
					string insertqry = WMSResource.posttestcrud;
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(insertqry, new
						{
							data.name,
							data.ismanager
						});

						if (rslt == 0)
						{
							result = "error";
						}
						else
						{
							result = "saved";
						}
					}


				}
				else
				{
					string insertqry = WMSResource.puttestcurd.Replace("#id", data.id.ToString());
					using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
						rslt = DB.Execute(insertqry, new
						{
							data.name,
							data.ismanager
						});

						if (rslt == 0)
						{
							result = "error";
						}
						else
						{
							result = "Updated";
						}

					}

				}


			}
			catch (Exception ex)
			{
				log.ErrorMessage("PODataProvider", "notifyputaway", ex.StackTrace.ToString());
				return "error";
			}

			return result;
		}

		/*
		Name of Function : <<deletetestcurd>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<delete test curd>>
		<param name="id"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string deletetestcurd(int id)
		{
			string result = "";
			int rslt = 0;
			try
			{

				string deleteqry = WMSResource.deletetestcurd.Replace("#id", id.ToString());
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
				{
					rslt = DB.Execute(deleteqry);

					if (rslt == 0)
					{
						result = "error";
					}
					else
					{
						result = "Deleted";
					}

				}

				return result;
			}

			catch (Exception ex)
			{

				Console.WriteLine(ex.Message);
				return "error";

			}


		}

		/*
		Name of Function : <<updateSecurityPrintHistory>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update Security Print History>>
		<param name="model"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateSecurityPrintHistory(PrintHistoryModel model)
		{


			var data = 0;
			model.reprintedon = DateTime.Now;
			string insertquery = WMSResource.insertSecurityPrintHistory;
			using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
			{
				//Get inwmasterid, print status from security inward table
				string secquery = "select inwmasterid,print  from wms.wms_securityinward where pono ='" + model.pono + "' and invoiceno ='" + model.invoiceNo + "'";
				var securityData = DB.QueryFirstOrDefault<inwardModel>(
						   secquery, null, commandType: CommandType.Text);

				//Check whether the reprint data for this PO and Invoice already exists in barcode table
				string barcodequery = "select barcodeid from wms.wms_barcode where  barcode ='" + model.po_invoice + "'";
				var barcodeData = DB.QueryFirstOrDefault<BarcodeModel>(
						   barcodequery, null, commandType: CommandType.Text);
				
				model.reprintcount = 1;
				model.inwmasterid = securityData.inwmasterid;
				model.barcodeid = barcodeData.barcodeid;

				//If print status is true in security inward table update data in reprint history table else update print status in security inward table
				if(securityData.print==true)
                {
					//updating data in reprint history table

					//Check if the data is already reprinted 
					string query = WMSResource.getbarcodereprintdata.Replace("#barcode", Convert.ToString(model.po_invoice));
					var barcodereprintData = DB.QueryFirstOrDefault<BarcodeModel>(
						   query, null, commandType: CommandType.Text);
					//if(barcodereprintData!=null)
					// {
					//	string updatequery=WMSResource.updatereprintdata.Replace("#reprintedby", Convert.ToString(model.reprintedby)).Replace("#reprintcount", Convert.ToString(barcodereprintData.reprintcount+1)).Replace("#barcodeid", Convert.ToString(barcodereprintData.barcodeid));
					//	data = DB.Execute(updatequery, new
					//	{

					//		model.reprintedby,

					//	});

					//}
					//else
					//   {
					int noofprint = 1;

						data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
						{
							model.inwmasterid,
							model.reprintedon,
							model.reprintedby,
							model.reprintcount,
							model.barcodeid,
							noofprint

						}));
					}
					
				//}
                else
                {
					securityData.print = true;
					securityData.printedby = model.reprintedby;
					securityData.printedon = model.reprintedon;
					string printedby = model.reprintedby;
					string updateqry = "update wms.wms_securityinward set print ="+ securityData.print+",  printedon = current_date, printedby = '"+ model.reprintedby + "' where pono = '"+ model.pono + "'  and invoiceno = '"+ model.invoiceNo+"'";
						//WMSResource.updateSecurityinwardprint.Replace("#print", Convert.ToString(securityData.print)).Replace("#printedby", Convert.ToString(printedby)).Replace("#pono", Convert.ToString(model.pono)).Replace("#invno", Convert.ToString(model.invoiceNo));
					data = DB.Execute(updateqry, new
					{
						securityData.print,
						printedby
					});

					
                }
			
			}
			if (data != 0)
				return "Sucess";
			else
				return "Error";


		}

		/*
		Name of Function : <<updateQRcodePrintHistory>>  Author :<<Gayathri>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<update QRcode Print History>>
		<param name="printMat"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public string updateQRcodePrintHistory(printMaterial printMat)
        {
			
				try
				{
				var data = 0;
				DateTime createddate = DateTime.Now;
				using (IDbConnection DB = new NpgsqlConnection(config.PostgresConnectionString))
					{
					//Once printing is successfull add data to barcode table
					string barcode = printMat.grnno + "-" + printMat.materialid;

					string barcodeqry = "select barcodeid from wms.wms_barcode where  barcode ='" + barcode + "' and pono='" + printMat.pono + "' and invoiceno='" + printMat.invoiceno + "'";
					var bar = DB.QueryFirstOrDefault<BarcodeModel>(
							   barcodeqry, null, commandType: CommandType.Text);
					if(bar==null)
                    {
						
							//insert bar code data

							bool deleteflag = false;
							string createdby = printMat.printedby;
							string insertbarcodequery = WMSResource.insertbarcodedata;//to insert bar code data
							var barcodeResult = DB.Execute(insertbarcodequery, new
							{
								barcode,
								createdby,
								createddate,
								deleteflag,
								printMat.pono,
								printMat.invoiceno
							});
						//bar.barcodeid = barcodeResult;
						 barcodeqry = "select barcodeid from wms.wms_barcode where  barcode ='" + barcode + "' and pono='" + printMat.pono + "' and invoiceno='" + printMat.invoiceno + "'";
						 bar = DB.QueryFirstOrDefault<BarcodeModel>(
								   barcodeqry, null, commandType: CommandType.Text);

					}
					

					string queryx = WMSResource.isgrnexistsquerybyinvoce.Replace("#pono", printMat.pono).Replace("#invno", printMat.invoiceno);
					var objx = DB.QuerySingle<inwardModel>(
					 queryx, null, commandType: CommandType.Text);
					if(objx.inwmasterid !=null && objx.inwmasterid != "")
                    {
						//check if print is true
						string barcodequery = "select isprint from wms.wms_printstatusmaterial where  inwmasterid ='" + objx.inwmasterid + "' and materialid='"+printMat.materialid+"'";
						var barcodeData = DB.QueryFirstOrDefault<printMaterial>(
								   barcodequery, null, commandType: CommandType.Text);
						if(barcodeData!=null)
                        {
							if (barcodeData.isprint == true)
							{

								//Add data in reprint history table
								string insertquery = WMSResource.insertSecurityPrintHistory;
								int reprintcount = 1;
									string reprintedby = printMat.printedby;
									data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
									{
										objx.inwmasterid,
										createddate,
										reprintedby,
										reprintcount,
										bar.barcodeid,
										printMat.noofprint

									}));
								

							}
							else
							{
								//Update data in print status material table -insertprintmaterial
								string insertquery = WMSResource.insertprintmaterial;
								int printcount = 1;
								bool isprint = true;
								string reprintedby = printMat.printedby;
								data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
								{
									objx.inwmasterid,
									createddate,
									printMat.printedby,
									printcount,
									printMat.noofprint,
									bar.barcodeid,
									isprint,
									printMat.materialid

								}));
							}

						}

						else
						{
						//Update data in print status material table -insertprintmaterial
							string insertquery = WMSResource.insertprintmaterial;
							int printcount = 1;
							bool isprint = true;
							string reprintedby = printMat.printedby;
							data = Convert.ToInt32(DB.ExecuteScalar(insertquery, new
							{
								objx.inwmasterid,
								createddate,
								printMat.printedby,
								printcount,
								printMat.noofprint,
								bar.barcodeid,
								isprint,
								printMat.materialid

							}));
						}







					}

					return "success";
					}
				}
				catch (Exception ex)
				{
					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}

			return "success";
        }

		/*
		Name of Function : <<getMaterialtransferdetails>>  Author :<<Ramesh>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get Material transfer details>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialtransferMain>> getMaterialtransferdetails(materilaTrasFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialTransferDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where ts.createdon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and ts.createdon::date >= '" + filterparams.FromDate + "'";
					query += " order by ts.transferid desc";
					var data = await pgsql.QueryAsync<materialtransferMain>(
					   query, null, commandType: CommandType.Text);

					if (data != null && data.Count() > 0)
					{
						foreach (materialtransferMain dt in data)
						{
							string query1 = WMSResource.gettransferiddetail.Replace("#tid", dt.transferid.ToString());
							var datadetail = await pgsql.QueryAsync<materialtransferTR>(
							   query1, null, commandType: CommandType.Text);

							if (datadetail != null && datadetail.Count() > 0)
							{
								dt.materialdata = datadetail.ToList();
							}
						}
					}
					return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getmaterialrequestdashboardList>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material request dashboardList>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<materialrequestMain>> getmaterialrequestdashboardList(materialRequestFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialRequestDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where mr.requesteddate::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and mr.requesteddate::date >= '" + filterparams.FromDate + "'";
					query += " order by mr.requesterid asc";
					var data = await pgsql.QueryAsync<materialrequestMain>(
					   query, null, commandType: CommandType.Text);

                    if (data != null && data.Count() > 0)
                    {
                        foreach (materialrequestMain dt in data)
                        {
							try
                            {
								string query1 = WMSResource.getrequestiddetail.Replace("#rid", dt.requestid.ToString());
								var datadetail = await pgsql.QueryAsync<materialrequestMR>(
								   query1, null, commandType: CommandType.Text);

								if (datadetail != null && datadetail.Count() > 0)
								{
									dt.materialdata = datadetail.ToList();
								}

							}
                           catch(Exception ex)
                            {
								return null;
                            }
                        }
                    }
                    return data.OrderByDescending(o=>o.requestid);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}

		/*
		Name of Function : <<getmaterialreservedashboardList>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material reserve dashboard List>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/
		public async Task<IEnumerable<materialreserveMain>> getmaterialreservedashboardList(materialResFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialReserveDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where mrs.reservedon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and mrs.reservedon::date >= '" + filterparams.FromDate + "'";
					//query += " order by mrs.requestedby asc";
					var data = await pgsql.QueryAsync<materialreserveMain>(
					   query, null, commandType: CommandType.Text);

                    if (data != null && data.Count() > 0)
                    {
                        foreach (materialreserveMain dt in data)
                        {
                            string query1 = WMSResource.getreserveiddetail.Replace("#rsid", dt.reserveid.ToString());
                            var datadetail = await pgsql.QueryAsync<materialreserveMS>(
                               query1, null, commandType: CommandType.Text);

                            if (datadetail != null && datadetail.Count() > 0)
                            {
                                dt.materialdata = datadetail.ToList();
                            }
                        }
                    }
                    return data.OrderByDescending(o=>o.reserveid);

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}


		/*
		Name of Function : <<getmaterialreturndashboardlist>>  Author :<<Amulya>>  
		Date of Creation <<12-12-2019>>
		Purpose : <<get material return dashboardlist>>
		<param name="filterparams"></param>
		Review Date :<<>>   Reviewed By :<<>>
		*/

		public async Task<IEnumerable<materialreturnMain>> getmaterialreturndashboardlist(materialRetFilterParams filterparams)
		{
			using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
			{
				try
				{
					await pgsql.OpenAsync();
					string query = WMSResource.getMaterialReturnDashboardDetails;
					if (!string.IsNullOrEmpty(filterparams.ToDate))
						query += " where mrt.createdon::date <= '" + filterparams.ToDate + "'";
					if (!string.IsNullOrEmpty(filterparams.FromDate))
						query += "  and mrt.createdon::date >= '" + filterparams.FromDate + "'";

					var data = await pgsql.QueryAsync<materialreturnMain>(
					   query, null, commandType: CommandType.Text);

                    if (data != null && data.Count() > 0)
                    {
                        foreach (materialreturnMain dt in data)
                        {
                            string query1 = WMSResource.getreturniddetail.Replace("#rtid", dt.returnid.ToString());
                            var datadetail = await pgsql.QueryAsync<materialreturnMT>(
                               query1, null, commandType: CommandType.Text);

                            if (datadetail != null && datadetail.Count() > 0)
                            {
                                dt.materialdata = datadetail.ToList();
                            }
                        }
                    }
                    return data;

				}
				catch (Exception ex)
				{

					log.ErrorMessage("PODataProvider", "gettransferdata", ex.StackTrace.ToString());
					return null;
				}
				finally
				{
					pgsql.Close();
				}
			}
		}



	}
}
