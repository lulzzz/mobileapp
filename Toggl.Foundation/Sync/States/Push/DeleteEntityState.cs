﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Toggl.Ultrawave.ApiClients.Interfaces;

namespace Toggl.Foundation.Sync.States.Push
{
    internal abstract class DeleteEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        : BasePushEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        where TModel : IIdentifiable
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        private readonly IDeletingApiClient<TModel> api;

        public StateResult DeletingFinished { get; } = new StateResult();

        public DeleteEntityState(ITogglApi api, IDataSource<TThreadsafeModel, TDatabaseModel> dataSource)
            : base(dataSource)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
        }

        public override IObservable<ITransition> Start(TThreadsafeModel entity)
            => delete(entity)
                .SelectMany(_ => DataSource.Delete(entity.Id))
                .Select(_ => DeletingFinished.Transition())
                .Catch(Fail(entity));

        private IObservable<Unit> delete(TModel entity)
            => entity == null
                ? Observable.Throw<Unit>(new ArgumentNullException(nameof(entity)))
                : api.Delete(entity);
    }
}
