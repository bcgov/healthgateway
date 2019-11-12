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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class iimplementatiion of <see cref="IHNClientDelegate"/> that uses sockets to connect to HNClient.
    /// </summary>
    public class SocketHNClientDelegate : IHNClientDelegate
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        private const string TEN_ZEROS = "0000000000";
        private const string HEADER_INDICATOR = "MSH"; // message header indicator
        private const int LENGTH_INDICATOR_LENGTH = 12; // length indicator length
        private const int DATA_INDICATOR_LENGTH = 2; // data indicator length
        private const int SOCKET_READ_SLEEP_TIME = 100;  // milliseconds
        private const int MAX_SOCKET_READ_TRIES = 100;
        private const string ERROR_MAX_RETRIES_MESSAGE_FORMAT = "Exceeded maximum retries to retrieve {0} from HNClient.";
        private const string ERROR_INVALID_MESSAGE_FORMAT = "Invalid HL7 message from HNClient: {0}.";
#pragma warning restore SA1310 // Field names should not contain underscore

        private readonly int port;

        private readonly ILogger<SocketHNClientDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketHNClientDelegate"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="logger">The logger provider.</param>
        public SocketHNClientDelegate(IConfiguration config, ILogger<SocketHNClientDelegate> logger)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.port = config.GetSection("HNClient").GetValue<int>("Port");
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string SendReceive(string message)
        {
            string receivedMessage = string.Empty;

            using (ISocketProxy socket = this.CreateSocket())
            {
                int numSocketReadTries = 0;
                while (socket.Available < 1)
                {
                    if (numSocketReadTries >= MAX_SOCKET_READ_TRIES)
                    {
                        throw new SystemException(string.Format(CultureInfo.CurrentCulture, ERROR_MAX_RETRIES_MESSAGE_FORMAT, "HANDSHAKE"));
                    }

                    numSocketReadTries++;
                    Thread.Sleep(SOCKET_READ_SLEEP_TIME);
                }

                // Verify that the initial connection has data (handshake)
                byte[] handShakeSegment = new byte[12];
                byte[] handShakeData = new byte[8];

                // Read 12 bytes of HandShake Segment and 8 bytes of HandShake data
                socket.Receive(handShakeSegment, 0, 12);
                socket.Receive(handShakeData, 0, 8);

                byte initialSeed = 0;
                byte[] encodedHandshake = this.EncodeData(handShakeData, initialSeed);
                byte seed = encodedHandshake[7];
                using (MemoryStream encodedMessage = this.CreateEncodedMessage(encodedHandshake, message, seed))
                {
                    // send message to HNClient
                    socket.Send(encodedMessage.ToArray(), (int)encodedMessage.Length);
                }

                numSocketReadTries = 0;
                while (socket.Available < 12 && numSocketReadTries < MAX_SOCKET_READ_TRIES)
                {
                    if (numSocketReadTries >= MAX_SOCKET_READ_TRIES)
                    {
                        throw new SystemException(string.Format(CultureInfo.CurrentCulture, ERROR_MAX_RETRIES_MESSAGE_FORMAT, "HEADER"));
                    }

                    numSocketReadTries++;
                    Thread.Sleep(SOCKET_READ_SLEEP_TIME);
                }

                int messageLength = 0;
                numSocketReadTries = 0;
                while (messageLength == 0)
                {
                    if (numSocketReadTries >= MAX_SOCKET_READ_TRIES)
                    {
                        throw new SystemException(string.Format(CultureInfo.CurrentCulture, ERROR_MAX_RETRIES_MESSAGE_FORMAT, "MESSAGE SIZE"));
                    }

                    // Receive the message header (size) data
                    byte[] bytesReceived = new byte[12];
                    socket.Receive(bytesReceived, 0, 12);

                    // Decode the message header and retrieve the total message length;
                    string headerIn = this.DecodeData(bytesReceived, seed);
                    messageLength = int.Parse(headerIn.Substring(DATA_INDICATOR_LENGTH, LENGTH_INDICATOR_LENGTH - DATA_INDICATOR_LENGTH), CultureInfo.InvariantCulture);
                }

                numSocketReadTries = 0;
                byte[] dataHL7in = new byte[messageLength];
                int totalReceived = 0;
                while (socket.Available > 0 && totalReceived < messageLength)
                {
                    if (numSocketReadTries >= MAX_SOCKET_READ_TRIES)
                    {
                        throw new SystemException(string.Format(CultureInfo.CurrentCulture, ERROR_MAX_RETRIES_MESSAGE_FORMAT, "MESSAGE"));
                    }

                    int partialMessageSize = socket.Available;
                    byte[] partialMessage = new byte[partialMessageSize];
                    socket.Receive(partialMessage, 0, partialMessageSize);
                    partialMessage.CopyTo(dataHL7in, totalReceived);
                    totalReceived += partialMessage.Length;
                    numSocketReadTries++;
                }

                // decode the HL7 Data
                string hl7Message = this.DecodeData(dataHL7in, seed);

                // Validate the hl7 message
                int indexOfMSG = hl7Message.IndexOf(HEADER_INDICATOR, StringComparison.InvariantCulture);
                if (indexOfMSG != -1)
                {
                    // Read after the MSH segment.
                    receivedMessage = hl7Message.Substring(indexOfMSG) + "\r";
                }
                else
                {
                    throw new SystemException(string.Format(CultureInfo.CurrentCulture, ERROR_INVALID_MESSAGE_FORMAT, "MISSING HEADER"));
                }
            }

            return receivedMessage;
        }

        /// <summary>
        /// Creates a socket proxy object.
        /// </summary>
        /// <returns>The socket proxy.</returns>
        protected virtual ISocketProxy CreateSocket()
        {
            // Get host related information.
            IPAddress address = IPAddress.Loopback;
            IPEndPoint endpoint = new IPEndPoint(address, this.port);

            ISocketProxy socket = new SocketProxy(endpoint);
            if (!socket.IsConnected)
            {
                throw new SystemException(string.Format(CultureInfo.CurrentCulture, "Could not connect to the socket. {0}:{1}", address.ToString(), this.port));
            }

            return socket;
        }

        private MemoryStream CreateEncodedMessage(byte[] encodedHandShakeData, string message, byte encodingByte)
        {
            MemoryStream memStream = new MemoryStream();

            // write 12 bytes of HS response to BufferedOutputStream
            byte[] bytesSent = Encoding.ASCII.GetBytes("HS0000000008");
            memStream.Write(bytesSent);

            // write 8 bytes of scrambled HandShake data to BufferedOutputStream
            memStream.Write(encodedHandShakeData);

            string siHeader = this.GetHostName();
            byte[] siHeaderByte = Encoding.ASCII.GetBytes(siHeader.Substring(0, 12));
            byte[] siHostName = Encoding.ASCII.GetBytes(siHeader.Substring(12));

            string dtSegment = this.InsertHeader(message);

            // separate DT Segment from HL7 message (aMessage)
            byte[] dataSegmentout = Encoding.ASCII.GetBytes(dtSegment.Substring(0, 12));
            byte[] dataHL7out = Encoding.ASCII.GetBytes(dtSegment.Substring(12));

            // Encode the segments and add them to the byte steam
            memStream.Write(this.EncodeData(siHeaderByte, encodingByte));
            memStream.Write(this.EncodeData(siHostName, encodingByte));
            memStream.Write(this.EncodeData(dataSegmentout, encodingByte));
            memStream.Write(this.EncodeData(dataHL7out, encodingByte));
            return memStream;
        }

        private string GetHostName()
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("GetHostName...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            IPAddress address = IPAddress.Loopback;
            string localIP = address.ToString();

            StringBuilder ipAddress = new StringBuilder(string.Empty);
            ipAddress.Append("SI0000000032A");

            string machineName = System.Net.Dns.GetHostName();
            if (machineName.Length > 16)
            {
                ipAddress.Append(machineName.Substring(0, 16));
            }
            else
            {
                ipAddress.Append(machineName.PadRight(16));
            }

            ipAddress.Append(localIP.PadRight(15));

            // This guarantees that the hostname will have a length of 44 characters
            string hostname = ipAddress.ToString().PadRight(44);
            return hostname;
        }

        private string InsertHeader(string message)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("InsertHeader...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            string lengthOfMessage = message.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
            message = "DT" + TEN_ZEROS.Substring(0, TEN_ZEROS.Length - lengthOfMessage.Length) + lengthOfMessage + message;
            return message;
        }

        private byte[] EncodeData(byte[] data, byte encodingSeed)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("EncodeData...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            byte[] byteData = data.ToArray();
            byteData[0] ^= encodingSeed;
            for (int x = 1; x < byteData.Length; x++)
            {
                byteData[x] ^= byteData[x - 1];
            }

            return byteData;
        }

        private string DecodeData(byte[] data, byte encodingSeed)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            this.logger.LogDebug("DecodeData...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            byte[] encodedBytes = data.ToArray();
            byte prevByte = encodedBytes[0];
            encodedBytes[0] ^= encodingSeed;
            for (int x = 1; x < encodedBytes.Length; x++)
            {
                byte currByte = encodedBytes[x];
                encodedBytes[x] ^= prevByte;
                prevByte = currByte;
            }

            return Encoding.UTF8.GetString(encodedBytes, 0, encodedBytes.Length);
        }
    }
}