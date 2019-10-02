using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace HealthGateway.Common.Authorization
{
    public class UserAuthorizationHandler : AuthorizationHandler<UserIsPatientRequirement, System.String>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                    UserIsPatientRequirement requirement,
                                    System.String hDid)
        {

            //var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            //using (JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler())
            //{
             //   var tokens = handler.ReadJwtToken(accessToken);
             //   var subId = tokens.Claims.First(claim => claim.Type == "sub").Value;


            if (System.String.Equals(context.User.Identity?.Name, hDid ))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class UserIsPatientRequirement : IAuthorizationRequirement { }

    public static class Operations
    {
        public static OperationAuthorizationRequirement Read = 
            new OperationAuthorizationRequirement { Name = nameof(Read) };
    }
}