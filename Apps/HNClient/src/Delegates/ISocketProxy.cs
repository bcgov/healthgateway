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

    /// <summary>
    /// The socket proxy interface.
    /// </summary>
    public interface ISocketProxy : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the socket is connected or not.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the amount of data that has been received and is available to read.
        /// </summary>
        int Available { get; }

        /// <summary>
        /// Sends data to the connected socket.
        /// </summary>
        /// <param name="buffer">The data to be sent.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <returns>The number of bytes sent.</returns>
        int Send(byte[] buffer, int size);

        /// <summary>
        /// Receives the specified number of bytes from the socket.
        /// </summary>
        /// <param name="buffer">An array of type System.Byte that is the storage location for received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <returns>The number of bytes received.</returns>
        int Receive(byte[] buffer, int offset, int size);
    }
}
