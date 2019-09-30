﻿//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.HNClient.Services
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using HealthGateway.HNClient.Delegates;
    using HealthGateway.HNClient.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A simple proxy service to HNClient.
    /// </summary>
    public class HNClientService : IHNClientService
    {
        private readonly IHNClientDelegate hnclient;
        private readonly IConfiguration configuration;
        private readonly string timeRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="HNClientService"/> class.
        /// </summary>
        /// <param name="configuration">The injected IConfiguration object.</param>
        /// <param name="hnclient">The injected HNClient delegate.</param>
        public HNClientService(IConfiguration configuration, IHNClientDelegate hnclient)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.hnclient = hnclient;
            this.configuration = configuration;
            this.timeRequest = configuration.GetSection("HNClient").GetValue<string>("TimeMessage");
        }

        /// <inheritdoc/>
        public HNMessage GetTime()
        {
            HNMessage request = new HNMessage(this.timeRequest);
            return this.SendMessage(request);
        }

        /// <inheritdoc/>
        public HNMessage SendMessage(HNMessage msg)
        {
            if (msg is null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            HNMessage retMessage = new HNMessage();
            try
            {
                retMessage.Message = this.hnclient.SendReceive(msg.Message);
            }
            catch (Exception e)
            {
                if (e is InvalidDataException ||
                    e is InvalidOperationException ||
                    e is SocketException)
                {
                    retMessage.IsErr = true;
                    retMessage.Error = e.Message;
                }
                else
                {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
                    throw new Exception("Failed to send message.", e);
#pragma warning restore CA1303 // Do not pass literals as localized parameters
                }
            }

            return retMessage;
        }
    }
}
