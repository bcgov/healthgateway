//-------------------------------------------------------------------------
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
namespace HealthGateway.HNClient.Delegates
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// The Socket proxy class to encapsulate the socket object.
    /// </summary>
    public sealed class SocketProxy : IDisposable, ISocketProxy
    {
        private readonly Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketProxy"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint to connect the socket to.</param>
        public SocketProxy(IPEndPoint endpoint)
        {
            if (endpoint is null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            this.socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(endpoint);
        }

        /// <inheritdoc/>
        public bool IsConnected => this.socket.Connected;

        /// <inheritdoc/>
        public int Available => this.socket.Available;

        /// <inheritdoc/>
        public int Send(byte[] buffer, int size) => this.socket.Send(buffer, size, SocketFlags.None);

        /// <inheritdoc/>
        public int Receive(byte[] buffer, int offset, int size) => this.socket.Receive(buffer, offset, size, SocketFlags.None);

        /// <inheritdoc/>
        public void Dispose() => this.socket.Dispose();
    }
}
