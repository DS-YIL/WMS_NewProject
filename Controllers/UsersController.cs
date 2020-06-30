using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Interfaces;
using WMS.Models;

namespace WMS.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UsersController : ControllerBase
	{
		private IUserService _userService;

		public UsersController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost("authenticate")]
		public IActionResult Authenticate([FromBody]AuthenticateModel model)
		{
			
			var user = _userService.Authenticate(model.Username, model.Password);
			// var data = _userService.validatelogincredentials((model.Username, model.Password);
			if (user == null)
				return Ok(new { message = "Username or password is incorrect" });

			return Ok(user);
		}

		//[HttpGet]
		//public IActionResult GetAll()
		//{
		//    var users = _userService.GetAll();
		//    return Ok(users);
		//}
	}
}