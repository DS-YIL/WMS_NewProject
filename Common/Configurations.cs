using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Common
{
	public class Configurations
	{
		public readonly string _SQLconnectionString = string.Empty;
		public readonly string _PostgreconnectionString = string.Empty;
		public readonly string _EmailLinkUrl = string.Empty;
		public readonly string _FilePath = string.Empty;
		public readonly string _IntranetConnectionString = string.Empty;
		public readonly string _GateEntryPrintIP = string.Empty;
		public readonly string _MaterialLabelPrintIP = string.Empty;
		public readonly string _OnholdmaterialprintIP = string.Empty;
		public readonly string _rackprintIP = string.Empty;
		public readonly string _binprintIP = string.Empty;
			public readonly string _SCMUrl = string.Empty;
		public readonly string _POChecker = string.Empty;
		public readonly string _POApprover = string.Empty;
		public readonly string _EmailType = string.Empty;
		public readonly string _FromEmailId = string.Empty;
		public readonly string _FinanceEmployeeNo = string.Empty;
		public readonly string _FinanceEmployeeName = string.Empty;
		public readonly string _plantid = string.Empty;
		public readonly string _FinanceApproverEmail = string.Empty;
		
		public Configurations()
		{
			var configurationBuilder = new ConfigurationBuilder();
			var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
			configurationBuilder.AddJsonFile(path, false);

			var root = configurationBuilder.Build();
			_SQLconnectionString = root.GetSection("ConnectionStrings").GetSection("DataConnection").Value;

			_PostgreconnectionString = root.GetSection("ConnectionStrings").GetSection("PostgresConnection").Value;
			_EmailLinkUrl = root.GetSection("ConnectionStrings").GetSection("EmailLinkUrl").Value;
			_FilePath = root.GetSection("ConnectionStrings").GetSection("LiveFilePath").Value;
			_GateEntryPrintIP = root.GetSection("ConnectionStrings").GetSection("GateEntryPrintIP").Value;
			_MaterialLabelPrintIP = root.GetSection("ConnectionStrings").GetSection("MaterialLabelPrintIP").Value;
			_OnholdmaterialprintIP = root.GetSection("ConnectionStrings").GetSection("OnholdmaterialprintIP").Value;
			_rackprintIP = root.GetSection("ConnectionStrings").GetSection("rackprintIP").Value;
			_binprintIP = root.GetSection("ConnectionStrings").GetSection("binprintIP").Value;
			_IntranetConnectionString = root.GetSection("ConnectionStrings").GetSection("IntranetConnectionString").Value;
			_SCMUrl = root.GetSection("ConnectionStrings").GetSection("SCMUrl").Value;
			_POChecker = root.GetSection("ConnectionStrings").GetSection("POChecker").Value;
			_POApprover = root.GetSection("ConnectionStrings").GetSection("POApprover").Value;
			_EmailType = root.GetSection("ConnectionStrings").GetSection("EmailType").Value;
			_FromEmailId = root.GetSection("ConnectionStrings").GetSection("FromEmailId").Value;
			_FinanceEmployeeNo = root.GetSection("ConnectionStrings").GetSection("FinanceEmployeeNo").Value;
			_FinanceEmployeeName = root.GetSection("ConnectionStrings").GetSection("FinanceEmployeeName").Value;
			_plantid = root.GetSection("ConnectionStrings").GetSection("plantid").Value;
			_FinanceApproverEmail = root.GetSection("ConnectionStrings").GetSection("FinanceApproverEmail").Value;

			var appSetting = root.GetSection("ApplicationSettings");

		}

		public string SQLConnectionString
		{
			get => _SQLconnectionString;
		}

		public string PostgresConnectionString
		{
			get => _PostgreconnectionString;
		}
		public string EmailLinkUrl
		{
			get => _EmailLinkUrl;
		}

		public string FilePath
		{
			get => _FilePath;
		}
		public string IntranetConnectionString
		{
			get => _IntranetConnectionString;
		}
		public string SCMUrl
		{
			get => _SCMUrl;
		}
		public string POChecker
		{
			get => _POChecker;
		}
		public string POApprover
		{
			get => _POApprover;
		}
		public string EmailType
		{
			get => _EmailType;
		}
		public string FromEmailId
		{
			get => _FromEmailId;
		}
		public string FinanceEmployeeNo
		{
			get => _FinanceEmployeeNo;
		}
		public string FinanceEmployeeName
		{
			get => _FinanceEmployeeName;
		}
		public string plantid
		{
			get => _plantid;
		}
		public string FinanceApproverEmail
		{
			get => _FinanceApproverEmail;
		}
	}
}
