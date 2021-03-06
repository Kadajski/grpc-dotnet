#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Greet;
using Grpc.AspNetCore.FunctionalTests.Infrastructure;
using Grpc.AspNetCore.Server.Internal;
using NUnit.Framework;

namespace Grpc.AspNetCore.FunctionalTests
{
    [TestFixture]
    public class DeadlineTests : FunctionalTestBase
    {
        [TestCase("Greet.Greeter/SayHellosDeadline")]
        [TestCase("Greet.Greeter/SayHellosDeadlineCancellationToken")]
        public async Task WriteUntilDeadline_SuccessResponsesStreamed(string requestUri)
        {
            // Arrange
            var requestMessage = new HelloRequest
            {
                Name = "World"
            };

            var requestStream = new MemoryStream();
            MessageHelpers.WriteMessage(requestStream, requestMessage);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequest.Headers.Add(GrpcProtocolConstants.TimeoutHeader, "200m");
            httpRequest.Content = new StreamContent(requestStream);

            // Act
            var response = await Fixture.Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).DefaultTimeout();

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("identity", response.Headers.GetValues("grpc-encoding").Single());
            Assert.AreEqual("application/grpc", response.Content.Headers.ContentType.MediaType);

            var responseStream = await response.Content.ReadAsStreamAsync().DefaultTimeout();
            var pipeReader = new StreamPipeReader(new PipeReaderFixStream(responseStream));

            var messageCount = 0;

            var readTask = Task.Run(async() =>
            {
                while (true)
                {
                    var greeting = await MessageHelpers.AssertReadStreamMessageAsync<HelloReply>(pipeReader).DefaultTimeout();

                    if (greeting != null)
                    {
                        Assert.AreEqual($"How are you World? {messageCount}", greeting.Message);
                        messageCount++;
                    }
                    else
                    {
                        break;
                    }
                }

            });

            await readTask.DefaultTimeout();

            Assert.AreNotEqual(0, messageCount);
        }
    }
}
