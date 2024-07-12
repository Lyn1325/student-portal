using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using StudentPortalApplication.Models;
using Microsoft.Owin.Security;
using System.Net;

namespace StudentPortalApplication
{
    public class ApiAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (var dbContext = new StudentPortalEntities())
            {
                //check username and password
                var user = await dbContext.StudentCredential
                    .FirstOrDefaultAsync(u => u.studentUsername == context.UserName && u.studentPassword == context.Password);
                if (user == null) {
                    context.SetError("invalid username or password");
                    return;
                }else
                {
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.studentID.ToString())); //added student ID as a claim
                    identity.AddClaim(new Claim("username", context.UserName));
                    identity.AddClaim(new Claim(ClaimTypes.Name, user.studentUsername));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
                }
              

                context.Validated(identity);
            }


            
           
        }
    }
}