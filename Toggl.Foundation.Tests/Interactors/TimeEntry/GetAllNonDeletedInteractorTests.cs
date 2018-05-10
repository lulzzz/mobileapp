﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Tests.Mocks;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.Interactors
{
    public sealed class GetAllNonDeletedInteractorTests
    {
        public sealed class TheExecuteMethod : BaseInteractorTests
        {
            private static readonly IEnumerable<IThreadsafeTimeEntry> timeEntries = new IThreadsafeTimeEntry[]
            {
                new MockTimeEntry { Id = 0, IsDeleted = true },
                new MockTimeEntry { Id = 1, IsDeleted = false },
                new MockTimeEntry { Id = 2, IsDeleted = true },
                new MockTimeEntry { Id = 4, IsDeleted = false },
                new MockTimeEntry { Id = 5, IsDeleted = false }
            };
            
            [Fact]
            public async Task RemovesAllDeletedTimeEntries()
            {
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(callInfo => Observable.Return(timeEntries.Where<IThreadsafeTimeEntry>(
                        callInfo.Arg<Func<IDatabaseTimeEntry, bool>>())));

                var finteredTimeEntries = await InteractorFactory.GetAllNonDeletedTimeEntries().Execute();

                finteredTimeEntries.Should().HaveCount(3);
                finteredTimeEntries.Select(te => te.IsDeleted).ShouldAllBeEquivalentTo(false);
            }
            
            [Fact]
            public async Task ThrowsIfDataSourceThrows()
            {
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>())
                    .Returns(Observable.Throw<IEnumerable<IThreadsafeTimeEntry>>(new Exception()));

                Action tryGetAll = () => InteractorFactory.GetAllNonDeletedTimeEntries().Execute().Wait();

                tryGetAll.ShouldThrow<Exception>();
            }
        }
    }
}