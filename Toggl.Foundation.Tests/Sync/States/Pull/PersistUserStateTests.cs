using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NSubstitute;
using Toggl.Foundation.Models;
using Toggl.Foundation.Sync.States;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Tests.Sync.States
{
    public sealed class PersistUserStateTests : PersistStateTests
    {
        public PersistUserStateTests()
            : base(new TheStartMethod())
        {
        }

        private sealed class TheStartMethod
            : TheStartMethod<PersistUserState, IUser, IDatabaseUser>
        {
            public override void SelectsTheLatestAtValue() // skip this test - it doesn't make sense
            {
            }
            
            public override void UpdatesSinceParametersOfTheFetchedEntity() // skip this test - it doesn't make sense
            {
            }

            protected override PersistUserState CreateState(IRepository<IDatabaseUser> repository, ISinceParameterRepository sinceParameterRepository)
                => new PersistUserState(repository, sinceParameterRepository);

            protected override List<IUser> CreateListWithOneItem(DateTimeOffset? at = null)
                => new List<IUser> { Substitute.For<IUser>() };

            protected override FetchObservables CreateObservablesWhichFetchesTwice()
                => CreateFetchObservables(
                    null,
                    user: Observable.Create<IUser>(observer =>
                    {
                        observer.OnNext(Substitute.For<IUser>());
                        observer.OnNext(Substitute.For<IUser>());
                        return () => { };
                    }));

            protected override FetchObservables CreateObservables(List<IUser> entity = null)
            => new FetchObservables(
                Observable.Return(new List<IWorkspace>()),
                Observable.Return(new List<IWorkspaceFeatureCollection>()),
                Observable.Return(entity?.First()),
                Observable.Return(new List<IClient>()),
                Observable.Return(new List<IProject>()),
                Observable.Return(new List<ITimeEntry>()),
                Observable.Return(new List<ITag>()),
                Observable.Return(new List<ITask>()),
                Observable.Return(Substitute.For<IPreferences>()));

            protected override List<IUser> CreateComplexListWhereTheLastUpdateEntityIsDeleted(DateTimeOffset? at)
                => new List<IUser>
                {
                    Substitute.For<IUser>()
                };

            protected override bool IsDeletedOnServer(IUser entity) => false;

            protected override IDatabaseUser Clean(IUser entity) => User.Clean(entity);

            protected override Func<IDatabaseUser, bool> ArePersistedAndClean(List<IUser> entities)
                => persisted => persisted.SyncStatus == SyncStatus.InSync;
        }
    }
}
