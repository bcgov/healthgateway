using HealthGateway.HNClient.Delegates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthGateway.HNClient.Mocks
{
    internal class SocketHNClientDelegateMock : SocketHNClientDelegate
    {
        Mock<ISocketProxy> mock;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketHNClientDelegate"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="logger">The logger provider.</param>
        public SocketHNClientDelegateMock(Mock<ISocketProxy> socketMock, IConfiguration config, ILogger<SocketHNClientDelegate> logger) : base(config, logger)
        {
            this.mock = socketMock;
        }

        protected override ISocketProxy CreateSocket()
        {
            return mock.Object;
        }
    }
}
