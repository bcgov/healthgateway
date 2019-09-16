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
namespace HealthGateway.HNClient
{
    using System.Text;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.IO;
    using System.Threading;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class iimplementatiion of <see cref="IHNClientDelegate"/> that uses sockets to connect to HNClient.
    /// </summary>
    public class SocketHNClientDelegate :IHNClientDelegate
    {
        private readonly string DATA_INDICATOR = "DT";
        // data indicator length
        private readonly int DATA_INDICATOR_LENGTH = 2;
        // length indicator length
        private readonly int LENGTH_INDICATOR_LENGTH = 12;
        // message header indicator
        private readonly string HEADER_INDICATOR = "MSH";
        private readonly int SOCKET_READ_SLEEP_TIME = 100;  // milliseconds
        private readonly int MAX_SOCKET_READ_TRIES = 100;
        private readonly string TEN_ZEROS = "0000000000";

        private readonly int port;

        private readonly ILogger<SocketHNClientDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketHNClientDelegate"/> class.
        /// </summary>
        /// <param name="config">The configuration provider.</param>
        /// <param name="logger">The logger provider.</param>
        public SocketHNClientDelegate(IConfiguration config, ILogger<SocketHNClientDelegate> logger)
        {
            this.port = config.GetSection("HNClient").GetValue<int>("Port");
            this.logger = logger;
        }

        /// <summary>
        /// <see cref="IHNClientDelegate.SendReceive(string)"/>.
        /// </summary>
        public string SendReceive(string message)
        {
            string receivedMessage = string.Empty;

            using (Socket socket = this.createSocket())
            {
                int numSocketReadTries = 0;
                while (socket.Available < 1 && numSocketReadTries < MAX_SOCKET_READ_TRIES)
                {
                    numSocketReadTries++;
                    Thread.Sleep(SOCKET_READ_SLEEP_TIME);
                }

                // Verify that the initial connection has data (handshake)
                if (socket.Available > 0)
                {
                    int bytes = 0;
                    byte[] handShakeSegment = new byte[12];
                    byte[] handShakeData = new byte[8];

                    // read 12 bytes of HandShake Segment and 8 bytes of HandShake data
                    bytes = socket.Receive(handShakeSegment, 0, 12, 0);
                    bytes = socket.Receive(handShakeData, 0, 8, 0);

                    byte initialSeed = 0;
                    byte[] encodedHandshake = encodeData(handShakeData, initialSeed);
                    byte seed = encodedHandshake[7];
                    MemoryStream encodedMessage = createEncodedMessage(encodedHandshake, message, seed);

                    // send message to HNClient
                    socket.Send(encodedMessage.ToArray(), (int)encodedMessage.Length, 0);

                    numSocketReadTries = 0;
                    while (socket.Available < 1 && numSocketReadTries < MAX_SOCKET_READ_TRIES)
                    {
                        numSocketReadTries++;
                        Thread.Sleep(SOCKET_READ_SLEEP_TIME);
                    }

                    // Receive the data
                    byte[] bytesReceived = new byte[12];
                    bytes = socket.Receive(bytesReceived, 0, 12, 0);
                    if (bytesReceived.Length <= 0)
                    {
                        throw new InvalidDataException("No response message");
                    }

                    // Read the message payload and attempt to load the message;
                    string headerIn = decodeData(bytesReceived, seed);
                    int messageLength = int.Parse(headerIn.Substring(DATA_INDICATOR_LENGTH, LENGTH_INDICATOR_LENGTH - DATA_INDICATOR_LENGTH));
                    numSocketReadTries = 0;
                    while (socket.Available < messageLength)
                    {
                        if (numSocketReadTries < MAX_SOCKET_READ_TRIES)
                        {
                            numSocketReadTries++;
                            Thread.Sleep(SOCKET_READ_SLEEP_TIME);
                        }
                        else
                        {
                            throw new InvalidDataException("Could not find entire message.");
                        }
                    }

                    // Receive the HL7 Data
                    byte[] dataHL7in = new byte[messageLength];
                    socket.Receive(dataHL7in, 0, messageLength, 0);
                    string hl7Message = decodeData(dataHL7in, seed);

                    // Validate the hl7 message
                    int indexOfMSG = hl7Message.IndexOf(HEADER_INDICATOR);
                    if (indexOfMSG != -1)
                    {
                        // Read after the MSH segment.
                        receivedMessage = hl7Message.Substring(indexOfMSG) + "\r";
                    }
                    else
                    {
                        throw new InvalidDataException("Could not find MSG header");
                    }
                }
            }

            return receivedMessage;
        }

        private MemoryStream createEncodedMessage(byte[] encodedHandShakeData, string message, byte encodingByte)
        {
            MemoryStream memStream = new MemoryStream();

            // write 12 bytes of HS response to BufferedOutputStream
            byte[] bytesSent = Encoding.ASCII.GetBytes("HS0000000008");
            memStream.Write(bytesSent);

            // write 8 bytes of scrambled HandShake data to BufferedOutputStream
            memStream.Write(encodedHandShakeData);

            string siHeader = getHostName();
            byte[] siHeaderByte = Encoding.ASCII.GetBytes(siHeader.Substring(0, 12));
            byte[] siHostName = Encoding.ASCII.GetBytes(siHeader.Substring(12));

            string dtSegment = insertHeader(message);

            // separate DT Segment from HL7 message (aMessage)
            byte[] dataSegmentout = Encoding.ASCII.GetBytes(dtSegment.Substring(0, 12));
            byte[] dataHL7out = Encoding.ASCII.GetBytes(dtSegment.Substring(12));

            // Encode the segments and add them to the byte steam
            memStream.Write(encodeData(siHeaderByte, encodingByte));
            memStream.Write(encodeData(siHostName, encodingByte));
            memStream.Write(encodeData(dataSegmentout, encodingByte));
            memStream.Write(encodeData(dataHL7out, encodingByte));
            return memStream;
        }

        private Socket createSocket()
        {
            // Get host related information.
            IPAddress address = IPAddress.Loopback;
            IPEndPoint endpoint = new IPEndPoint(address, this.port);

            Socket socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);

            if (!socket.Connected)
            {
                throw new InvalidOperationException("Could not connect to the socket");
            }
            return socket;
        }

        private string getHostName()
        {
            IPAddress address = IPAddress.Loopback;
            string localIP = address.ToString();

            StringBuilder ipAddress = new StringBuilder("");
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

        private string insertHeader(string message)
        {
            string lengthOfMessage = message.Length.ToString();
            message = "DT" + this.TEN_ZEROS.Substring(0, this.TEN_ZEROS.Length - lengthOfMessage.Length) + lengthOfMessage + message;
            return message;
        }

        private byte[] encodeData(byte[] data, byte encodingSeed)
        {
            byte[] byteData = data.ToArray();
            byteData[0] ^= encodingSeed;
            for (int x = 1; x < byteData.Length; x++)
            {
                byteData[x] ^= byteData[x - 1];
            }
            return byteData;
        }

        private string decodeData(byte[] data, byte encodingSeed)
        {
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