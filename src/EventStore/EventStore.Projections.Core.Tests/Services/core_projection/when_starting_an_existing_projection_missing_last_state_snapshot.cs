// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Text;
using EventStore.Core.Data;
using EventStore.Projections.Core.Messages;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.core_projection
{
    [TestFixture]
    public class when_starting_an_existing_projection_missing_last_state_snapshot : TestFixtureWithCoreProjection
    {
        private readonly Guid _causedByEventId = Guid.NewGuid();

        protected override void Given()
        {
            ExistingEvent(
                "$projections-projection-state", "StateUpdated",
                @"{""CommitPosition"": 100, ""PreparePosition"": 50, ""LastSeenEvent"": """
                + Guid.NewGuid().ToString("D") + @"""}", "{}");
            ExistingEvent(
                "$projections-projection-checkpoint", "ProjectionCheckpoint",
                @"{""CommitPosition"": 100, ""PreparePoistion"": 50, ""LastSeenEvent"": """
                + Guid.NewGuid().ToString("D") + @"""}", "{}");
            ExistingEvent(
                FakeProjectionStateHandler._emit1StreamId, FakeProjectionStateHandler._emit1EventType,
                @"{""CommitPosition"": 120, ""PreparePosition"": 110}", FakeProjectionStateHandler._emit1Data);
        }

        protected override void When()
        {
            //projection subscribes here
            _coreProjection.Handle(
                new ProjectionMessage.Projections.CommittedEventReceived(
                    Guid.Empty, new EventPosition(120, 110), "/event_category/1", -1, false,
                    new Event(
                        _causedByEventId, "emit1_type", false, Encoding.UTF8.GetBytes("data"),
                        Encoding.UTF8.GetBytes("metadata"))));
        }

        [Test]
        public void should_not_emit_events_but_write_the_new_state_snapshot()
        {
            Assert.AreEqual(1, _writeEventHandler.HandledMessages.Count);

            var data = Encoding.UTF8.GetString(_writeEventHandler.HandledMessages[0].Events[0].Data);
            Assert.AreEqual("data", data);
        }
    }
}
