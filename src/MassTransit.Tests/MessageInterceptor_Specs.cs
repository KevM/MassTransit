// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Tests
{
    using System.Threading;
    using Configuration;
    using MassTransit.Transports;
    using Messages;
    using NUnit.Framework;
    using TextFixtures;

    [TestFixture]
    public class MessageInterceptor_Specs
        : EndpointTestFixture<LoopbackEndpoint>
    {
        public IServiceBus LocalBus { get; private set; }

        private ManualResetEvent _before;
        private ManualResetEvent _after;

        [Test]
        public void An_interceptor_should_be_called_when_a_message_is_received()
        {
            LocalBus.Subscribe<PingMessage>(x => { Assert.IsFalse(_after.WaitOne(0, false), "Should not have called after yet"); });

            LocalBus.Publish(new PingMessage());

            _before.WaitOne(1000, false).ShouldBeTrue();
            _after.WaitOne(1000, false).ShouldBeTrue();
        }

        protected override void EstablishContext()
        {
            base.EstablishContext();

            _before = new ManualResetEvent(false);
            _after = new ManualResetEvent(false);

            LocalBus = ServiceBusConfigurator.New(x =>
                {
                    x.ReceiveFrom("loopback://localhost/mt_client");

                    x.BeforeConsumingMessage(() => { _before.Set(); });
                    x.AfterConsumingMessage(() => { _after.Set(); });
                });
        }

        protected override void TeardownContext()
        {
            LocalBus.Dispose();
            LocalBus = null;

            base.TeardownContext();
        }
    }
}