﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Foundation.Tests.Helpers;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public abstract class PersistStateTests
    {
        private readonly ITheStartMethodHelper testHelper;

        protected PersistStateTests(ITheStartMethodHelper helper)
        {
            testHelper = helper;
        }

        [Fact, LogIfTooSlow]
        public void EmitsTransitionToPersistFinished()
            => testHelper.EmitsTransitionToPersistFinished();

        [Fact, LogIfTooSlow]
        public void ThrowsIfFetchObservablePublishesTwice()
            => testHelper.ThrowsIfFetchObservablePublishesTwice();

        [Fact, LogIfTooSlow]
        public void TriggersBatchUpdate()
            => testHelper.TriggersBatchUpdate();

        [Fact, LogIfTooSlow]
        public void DoesNotUpdateSinceParametersWhenNothingIsFetched()
            => testHelper.DoesNotUpdateSinceParametersWhenNothingIsFetched();

        [Fact, LogIfTooSlow]
        public void UpdatesSinceParametersOfTheFetchedEntity()
            => testHelper.UpdatesSinceParametersOfTheFetchedEntity();

        [Fact, LogIfTooSlow]
        public void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
            => testHelper.HandlesNullValueReceivedFromTheServerAsAnEmptyList();

        [Fact, LogIfTooSlow]
        public void SelectsTheLatestAtValue()
            => testHelper.SelectsTheLatestAtValue();    

        [Fact, LogIfTooSlow]
        public void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
            => testHelper.SinceDatesAreNotUpdatedWhenBatchUpdateThrows();

        [Fact, LogIfTooSlow]
        public void ThrowsWhenBatchUpdateThrows()
            => testHelper.ThrowsWhenBatchUpdateThrows();

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ClientExceptionsWhichAreNotReThrownInSyncStates), MemberType = typeof(ApiExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
            => testHelper.ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(exception);

        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ServerExceptions), MemberType = typeof(ApiExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException reason)
            => testHelper.ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(reason);


        [Theory, LogIfTooSlow]
        [MemberData(nameof(ApiExceptions.ExceptionsWhichCauseRethrow), MemberType = typeof(ApiExceptions))]
        public void ThrowsWhenCertainExceptionsAreCaught(Exception exception)
            => testHelper.ThrowsWhenCertainExceptionsAreCaught(exception);

        public interface ITheStartMethodHelper
        {
            void EmitsTransitionToPersistFinished();

            void ThrowsIfFetchObservablePublishesTwice();

            void TriggersBatchUpdate();

            void ThrowsWhenBatchUpdateThrows();

            void DoesNotUpdateSinceParametersWhenNothingIsFetched();

            void UpdatesSinceParametersOfTheFetchedEntity();

            void HandlesNullValueReceivedFromTheServerAsAnEmptyList();

            void SelectsTheLatestAtValue();

            void SinceDatesAreNotUpdatedWhenBatchUpdateThrows();

            void ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception);

            void ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception);

            void ThrowsWhenCertainExceptionsAreCaught(Exception exception);
        }

        internal abstract class TheStartMethod<TState, TInterface, TDatabaseInterface> : ITheStartMethodHelper
            where TDatabaseInterface : class, TInterface
            where TState : BasePersistState<TInterface, TDatabaseInterface>
        {
            private readonly IRepository<TDatabaseInterface> repository;
            private readonly ISinceParameterRepository sinceParameterRepository;
            private readonly TState state;

            protected DateTimeOffset Now { get; } = new DateTimeOffset(2017, 04, 05, 12, 34, 56, TimeSpan.Zero);
            protected DateTimeOffset at = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);

            protected TheStartMethod()
            {
                repository = Substitute.For<IRepository<TDatabaseInterface>>();
                sinceParameterRepository = Substitute.For<ISinceParameterRepository>();
                state = CreateState(repository, sinceParameterRepository);
            }

            public virtual void EmitsTransitionToPersistFinished()
            {
                var observables = CreateObservables();

                var transition = state.Start(observables).SingleAsync().Wait();

                transition.Result.Should().Be(state.FinishedPersisting);
            }

            public virtual void ThrowsIfFetchObservablePublishesTwice()
            {
                var fetchObservables = CreateObservablesWhichFetchesTwice();

                Action fetchTwice = () => state.Start(fetchObservables).Wait();

                fetchTwice.ShouldThrow<InvalidOperationException>();
            }

            public virtual void TriggersBatchUpdate()
            {
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(entities);

                state.Start(observables).SingleAsync().Wait();

                assertBatchUpdateWasCalled(entities);
            }

            public virtual void ThrowsWhenBatchUpdateThrows()
            {
                var observables = CreateObservables();
                setupBatchUpdateToThrow(new TestException());

                Action startingState = () => state.Start(observables).SingleAsync().Wait();

                startingState.ShouldThrow<TestException>();
            }

            public virtual void DoesNotUpdateSinceParametersWhenNothingIsFetched()
            {
                var observables = CreateObservables();

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.DidNotReceive().Set<TDatabaseInterface>(Arg.Any<DateTimeOffset?>());
            }

            public virtual void UpdatesSinceParametersOfTheFetchedEntity()
            {
                var oldAt = new DateTimeOffset(2017, 09, 01, 12, 34, 56, TimeSpan.Zero);
                var newAt = new DateTimeOffset(2017, 10, 01, 12, 34, 56, TimeSpan.Zero);
                var entities = CreateListWithOneItem(newAt);
                var observables = CreateObservables(entities);
                setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
                sinceParameterRepository.Supports<TDatabaseInterface>().Returns(true);

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.Received().Set<TDatabaseInterface>(Arg.Is(newAt));
            }

            public virtual void HandlesNullValueReceivedFromTheServerAsAnEmptyList()
            {
                List<TInterface> entities = null;
                var observables = CreateObservables(entities);

                var transition = (Transition<FetchObservables>)state.Start(observables).SingleAsync().Wait();

                transition.Result.Should().Be(state.FinishedPersisting);
                assertBatchUpdateWasCalled(new List<TInterface>());
            }

            public virtual void SelectsTheLatestAtValue()
            {
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(entities);
                setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(entities);
                sinceParameterRepository.Supports<TDatabaseInterface>().Returns(true);

                state.Start(observables).SingleAsync().Wait();

                sinceParameterRepository.Received().Set<TDatabaseInterface>(Arg.Is<DateTimeOffset?>(at));
            }

            public virtual void SinceDatesAreNotUpdatedWhenBatchUpdateThrows()
            {
                var entities = CreateComplexListWhereTheLastUpdateEntityIsDeleted(at);
                var observables = CreateObservables(entities);
                setupBatchUpdateToThrow(new TestException());

                try
                {
                    state.Start(observables).SingleAsync().Wait();
                }
                catch (TestException) { }

                sinceParameterRepository.DidNotReceiveWithAnyArgs().Set<TDatabaseInterface>(null);
            }

            public virtual void ReturnsFailedTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
                var observables = createFetchObservablesWhichThrow(exception);

                var transition = state.Start(observables).SingleAsync().Wait();
                var reason = ((Transition<Exception>)transition).Parameter;

                transition.Result.Should().Be(state.Failed);
                reason.Should().BeAssignableTo<ClientErrorException>();
            }

            public virtual void ReturnsFailedTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
                var observables = createFetchObservablesWhichThrow(exception);

                var transition = state.Start(observables).SingleAsync().Wait();
                var reason = ((Transition<Exception>)transition).Parameter;

                transition.Result.Should().Be(state.Failed);
                reason.Should().BeAssignableTo<ServerErrorException>();
            }

            public virtual void ThrowsWhenCertainExceptionsAreCaught(Exception exception)
            {
                var state = CreateState(repository, sinceParameterRepository);
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

            protected FetchObservables CreateFetchObservables(
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

            protected abstract TState CreateState(IRepository<TDatabaseInterface> repository, ISinceParameterRepository sinceParameterRepository);

            protected abstract List<TInterface> CreateListWithOneItem(DateTimeOffset? at = null);

            protected abstract List<TInterface> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset? at);

            protected abstract FetchObservables CreateObservablesWhichFetchesTwice();

            protected abstract FetchObservables CreateObservables(List<TInterface> entities = null);

            protected abstract bool IsDeletedOnServer(TInterface entity);

            protected abstract TDatabaseInterface Clean(TInterface entity);

            protected abstract Func<TDatabaseInterface, bool> ArePersistedAndClean(List<TInterface> entities);

            private void setupDatabaseBatchUpdateMocksToReturnUpdatedDatabaseEntitiesAndSimulateDeletionOfEntities(List<TInterface> entities = null)
            {
                var foundationEntities = entities?.Select(entity => IsDeletedOnServer(entity)
                    ? (IConflictResolutionResult<TDatabaseInterface>)new DeleteResult<TDatabaseInterface>(0)
                    : new UpdateResult<TDatabaseInterface>(0, Clean(entity)));
                repository.BatchUpdate(null, null)
                    .ReturnsForAnyArgs(Observable.Return(foundationEntities));
            }

            private void setupBatchUpdateToThrow(Exception exception)
                => repository.BatchUpdate(null, null).ReturnsForAnyArgs(_ => throw exception);

            private void assertBatchUpdateWasCalled(List<TInterface> entities = null)
            {
                repository.Received().BatchUpdate(Arg.Is<IEnumerable<(long, TDatabaseInterface entity)>>(
                        list => list.Count() == entities.Count && list.Select(pair => pair.Item2).All(ArePersistedAndClean(entities))),
                    Arg.Any<Func<TDatabaseInterface, TDatabaseInterface, ConflictResolutionMode>>(),
                    Arg.Any<IRivalsResolver<TDatabaseInterface>>());
            }

            private FetchObservables createFetchObservablesWhichThrow(Exception exception)
                => CreateFetchObservables(null,
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
}
