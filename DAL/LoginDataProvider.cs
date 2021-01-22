using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WMS.Models;
using System.Threading.Tasks;
using WMS.Helpers;
using Microsoft.Extensions.Options;
using WMS.Interfaces;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Authentication;
using WMS.Common;
using System.Data;
using Dapper;
using Npgsql;

namespace WMS.DAL
{
    public class LoginDataProvider : IUserService
    {
        Configurations config = new Configurations();
        ErrorLogTrace log = new ErrorLogTrace();

        /*
        Name of Function : <<validatelogincredentials>>  Author :<<>>  
        Date of Creation <<>>
        Purpose : <<validation of user>>
        <param: DomainId,pwd></param>
        Review Date :<<>>   Reviewed By :<<>>
        */
        public List<User> validatelogincredentials(string DomainId, string pwd)
        {

            string id = DomainId;
            // PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            PrincipalContext ctx = null;
            List<User> userdata = new List<User>();
            User data = new User();
            //UserPrincipal user = UserPrincipal.FindByIdentity(ctx, id);
            UserPrincipal user = null;
            if (user != null)
            {
                if (ctx.ValidateCredentials(DomainId, pwd))
                {

                    using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
                    {

                        try
                        {
                            pgsql.Open();
                            string query = "select  * from wms.employee where domainid='" + id.ToUpper() + "'";
                            data = pgsql.QuerySingle<User>(
                               query, null, commandType: CommandType.Text);
                            data.Password = pwd;
                            userdata.Add(data);
                        }
                        catch (Exception Ex)
                        {
                            log.ErrorMessage("LoginDataProvider", "validatelogincredentials", Ex.StackTrace.ToString(), Ex.Message.ToString());
                            userdata = null;
                        }
                        finally
                        {
                            pgsql.Close();
                        }
                    }
                }

            }
            else if (user == null)
            {
                using (var pgsql = new NpgsqlConnection(config.PostgresConnectionString))
                {

                    try
                    {
                        pgsql.Open();
                        string query = "select  * from wms.employee where domainid='" + id.ToUpper() + "'";
                        data = pgsql.QueryFirstOrDefault<User>(
                           query, null, commandType: CommandType.Text);
                        data.Password = pwd;
                        userdata.Add(data);
                    }
                    catch (Exception Ex)
                    {
                        log.ErrorMessage("LoginDataProvider", "validatelogincredentials", Ex.StackTrace.ToString(),Ex.Message.ToString());
                        userdata = null;
                    }
                    finally
                    {
                        pgsql.Close();
                    }
                }
            }
            return userdata;

        }


        private readonly AppSettings _appSettings;

        public LoginDataProvider(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }


        /*
        Name of Function : <<Authenticate>>  Author :<<>>  
        Date of Creation <<>>
        Purpose : <<Authenticate of userid and password>>
        <param: username,password></param>
        Review Date :<<>>   Reviewed By :<<>>
        */
        public User Authenticate(string username, string password)
        {
            var userss = validatelogincredentials(username, password);
            if (userss.Count != 0)
            {
                var user = userss.SingleOrDefault(x => x.domainid == username.ToUpper() && x.Password == password);
                User use = new User();
                // return null if user not found
                if (user == null)
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.domainid.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                user.Token = tokenHandler.WriteToken(token);

                // remove password before returning
                user.pwd = null;

                return user;
            }
            else
            {
                return null;
            }

        }

    }
}
