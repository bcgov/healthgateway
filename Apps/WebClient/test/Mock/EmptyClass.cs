using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;

namespace HealthGateway.Mock
{
    public class MockHttpContext : HttpContext
    {
        private readonly IPrincipal user;

        public MockHttpContext(IPrincipal principal)
        {
            this.user = principal;
        }

        public override ClaimsPrincipal User
        {
            get { return (System.Security.Claims.ClaimsPrincipal)user; }
            set => throw new NotImplementedException();
        }

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => throw new NotImplementedException();

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override AuthenticationManager Authentication => throw new NotImplementedException();

        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
