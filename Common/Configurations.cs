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
		public readonly string _SCMUrl = string.Empty;
		public readonly string _POChecker = string.Empty;
		public readonly string _POApprover = string.Empty;
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
			_IntranetConnectionString = root.GetSection("ConnectionStrings").GetSection("IntranetConnectionString").Value;
			_SCMUrl = root.GetSection("ConnectionStrings").GetSection("SCMUrl").Value;
			_POChecker = root.GetSection("ConnectionStrings").GetSection("POChecker").Value;
			_POApprover = root.GetSection("ConnectionStrings").GetSection("POApprover").Value;

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
	}
}
