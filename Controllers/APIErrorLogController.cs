using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Interfaces;
using WMS.Models;

namespace WMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIErrorLogController : ControllerBase
    {
        private readonly IErrorLog<ErroLogModel> _appDataProvider;
        public APIErrorLogController(IErrorLog<ErroLogModel> _appDataProvider)
        {
            this._appDataProvider = _appDataProvider;
        }
        [HttpGet]
        [ActionName("GetErrorLogdata")]
        public async Task<IEnumerable<ErroLogModel>> GetErrorLogdata()
        {
            return await this._appDataProvider.GetLogData();
        }
    }
}