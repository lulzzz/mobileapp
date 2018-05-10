using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistStateTests
    {
        private readonly IDataSource<IThreadsafeTag, IDatabaseTag> dataSource;
        private readonly ISinceParameterRepository sinceParameterRepository;
        private readonly PersistState<ITag, IDatabaseTag, IThreadsafeTag> state;

        private DateTimeOffset now { get; } = new DateTimeOffset(2017, 04, 05, 12, 34, 56, TimeSpan.Zero);
        private DateTimeOffset at = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);

        public PersistStateTests()
        {
            dataSource = Substitute.For<IDataSource<IThreadsafeTag, IDatabaseTag>>();
            sinceParameterRepository = Substitute.For<ISinceParameterRepository>();
            state = new PersistState<ITag, IDatabaseTag, IThreadsafeTag>(dataSource, sinceParameterRepository, Tag.Clean);
        }

        [Fact, LogIfTooSlow]
        public void EmitsTransitionToPersistFinished()
        {
            var observables = createObservables();

            var transition = state.Start(observables).SingleAsync().Wait();

            transition.Result.Should().Be(state.FinishedPersisting);
        }

        [Fact, LogIfTooSlow]
        public void ThrowsIfFetchObservablePublishesTwice()
        {
            var fetchObservables = createFetchObservables(
                null,
                tags: Observable.Create<List<ITag>>(observer =>
                {
                    observer.OnNext(new List<ITag>());
                    observer.OnNext(new List<ITag>());
                    return () => { };
                }));

            Action fetchTwice = () => state.Start(fetchObservables).Wait();

            fetchTwice.ShouldThrow<InvalidOperationException>();
        }

        [Fact, LogIfTooSlow]
        public void TriggersBatchUpdate()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);

            state.Start(observables).SingleAsync().Wait();

            assertBatchUpdateWasCalled(entities);
        }

        [Fact, LogIfTooSlow]
        public void DoesNotUpdateSinceParametersWhenNothingIsFetched()
        {
            var observables = createObservables();

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.DidNotReceive().Set<IDatabaseTag>(Arg.Any<DateTimeOffset?>());
        }

        [Fact, LogIfTooSlow]
        public void UpdatesSinceParametersOfTheFetchedEntity()
        {
            var newAt = new DateTimeOffset(2017, 10, 01, 12, 34, 56, TimeSpan.Zero);
            var entities = new List<ITag> { new MockTag { At = newAt } };
            var observables = createObservables(entities);
            setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
            sinceParameterRepository.Supports<IDatabaseTag>().Returns(true);

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.Received().Set<IDatabaseTag>(Arg.Is(newAt));
        }

        [Fact, LogIfTooSlow]
        public void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
        {
            List<ITag> entities = null;
            var observables = createObservables(entities);

            var transition = (Transition<FetchObservables>)state.Start(observables).SingleAsync().Wait();

            transition.Result.Should().Be(state.FinishedPersisting);
            assertBatchUpdateWasCalled(new List<ITag>());
        }

        [Fact, LogIfTooSlow]
        public void SelectsTheLatestAtValue()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);
            setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
            sinceParameterRepository.Supports<IDatabaseTag>().Returns(true);

            state.Start(observables).SingleAsync().Wait();

            sinceParameterRepository.Received().Set<IDatabaseTag>(Arg.Is<DateTimeOffset?>(at));
        }   

        [Fact, LogIfTooSlow]
        public void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
        {
            var entities = createComplexListWhereTheLastUpdateEntityIsDeleted(at);
            var observables = createObservables(entities);
            setupBatchUpdateToThrow(new TestException());

            try
            {
                state.Start(observables).SingleAsync().Wait();
            }
            catch (TestException) { }

            sinceParameterRepository.DidNotReceiveWithAnyArgs().Set<IDatabaseTag>(null);
        }

        [Fact, LogIfTooSlow]
        public void ThrowsWhenBatchUpdateThrows()
        {
            var observables = createObservables();
            setupBatchUpdateToThrow(new TestException());

            Action startingState = () => state.Start(observables).SingleAsync().Wait();

            startingState.ShouldThrow<TestException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
        {
            var state = new PersistState<ITag, IDatabaseTag, IThreadsafeTag>(dataSource, sinceParameterRepository, Tag.Clean);
            var observables = createFetchObservablesWhichThrow(exception);

            var transition = state.Start(observables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ClientErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
        {
            var state = new PersistState<ITag, IDatabaseTag, IThreadsafeTag>(dataSource, sinceParameterRepository, Tag.Clean);
            var observables = createFetchObservablesWhichThrow(exception);

            var transition = state.Start(observables).SingleAsync().Wait();
            var reason = ((Transition<Exception>)transition).Parameter;

            transition.Result.Should().Be(state.Failed);
            reason.Should().BeAssignableTo<ServerErrorException>();
        }

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        public void ThrowsWhenCertainExceptionsAreCaught(Exception exception)
        {
            var state = new PersistState<ITag, IDatabaseTag, IThreadsafeTag>(dataSource, sinceParameterRepository, Tag.Clean);
            Exception caughtException = null;
            var observables = createFetchObservablesWhichThrow(exception);

            try
            {
                state.Start(observables).Wait();
            }
            catch (Exception e)
            {
                caughtException = e;
            }

            caughtException.Should().NotBeNull();
            caughtException.Should().BeAssignableTo(exception.GetType());
        }

        private FetchObservables createFetchObservables(
            FetchObservables old,
            IObservable<List<IWorkspace>> workspaces = null,
            IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures = null,
            IObservable<IUser> user = null,
            IObservable<List<IClient>> clients = null,
            IObservable<List<IProject>> projects = null,
            IObservable<List<ITimeEntry>> timeEntries = null,
            IObservable<List<ITag>> tags = null,
            IObservable<List<ITask>> tasks = null,
            IObservable<IPreferences> preferences = null)
        => new FetchObservables(
            old?.Workspaces ?? workspaces,
            old?.WorkspaceFeatures ?? workspaceFeatures,
            old?.User ?? user,
            old?.Clients ?? clients,
            old?.Projects ?? projects,
            old?.TimeEntries ?? timeEntries,
            old?.Tags ?? tags,
            old?.Tasks ?? tasks,
            old?.Preferences ?? preferences);

        private List<ITag> createComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset? maybeAt)
        {
            var at = maybeAt ?? now;
            return new List<ITag>
            {
                new MockTag { At = at.AddDays(-1), Name = Guid.NewGuid().ToString() },
                new MockTag { At = at.AddDays(-3), Name = Guid.NewGuid().ToString() },
                new MockTag { At = at, Name = Guid.NewGuid().ToString(), ServerDeletedAt = at.AddDays(-1) },
                new MockTag { At = at.AddDays(-2), Name = Guid.NewGuid().ToString() }
            };
        }

        private FetchObservables createObservables(List<ITag> entities = null)
            => new FetchObservables(
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(Substitute.For<IUser>()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(entities),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

        private void setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(List<ITag> entities = null)
        {
            var foundationEntities = entities?.Select(entity => entity.ServerDeletedAt.HasValue
                ? (IConflictResolutionResult<IThreadsafeTag>)new DeleteResult<IThreadsafeTag>(0)
                : new UpdateResult<IThreadsafeTag>(0, Tag.Clean(entity)));
            dataSource.BatchUpdate(null)
                .ReturnsForAnyArgs(Observable.Return(foundationEntities));
        }

        private void setupBatchUpdateToThrow(Exception exception)
            => dataSource.BatchUpdate(null).ReturnsForAnyArgs(_ => throw exception);

        private void assertBatchUpdateWasCalled(List<ITag> entities = null)
        {
            dataSource.Received().BatchUpdate(Arg.Is<IEnumerable<IThreadsafeTag>>(
                list => list.Count() == entities.Count && list.All(
                    persisted => persisted.SyncStatus == SyncStatus.InSync && entities.Any(te => te.Name == persisted.Name))));
        }

        private FetchObservables createFetchObservablesWhichThrow(Exception exception)
            => createFetchObservables(null,
                Observable.Throw<List<IWorkspace>>(exception),
                Observable.Throw<List<IWorkspaceFeatureCollection>>(exception),
                Observable.Throw<IUser>(exception),
                Observable.Throw<List<IClient>>(exception),
                Observable.Throw<List<IProject>>(exception),
                Observable.Throw<List<ITimeEntry>>(exception),
                Observable.Throw<List<ITag>>(exception),
                Observable.Throw<List<ITask>>(exception),
                Observable.Throw<IPreferences>(exception));
    }
}
