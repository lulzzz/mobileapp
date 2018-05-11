﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Foundation.Sync.States.Push.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class UpdateEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        : BasePushEntityState<TThreadsafeModel>, IUpdateEntityState<TModel, TThreadsafeModel>
        where TModel : class, IApiModel
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        private readonly IUpdatingApiClient<TModel> api;

        private readonly IConflictResolutionUpdatingDataSource<TThreadsafeModel, TDatabaseModel> dataSource;
        
        private readonly Func<TModel, TThreadsafeModel> convertToThreadsafeModel;
        
        public StateResult<TModel> EntityChanged { get; } = new StateResult<TModel>();

        public StateResult<TModel> UpdatingSucceeded { get; } = new StateResult<TModel>();

        public UpdateEntityState(
            IUpdatingApiClient<TModel> api,
            IConflictResolutionUpdatingDataSource<TThreadsafeModel, TDatabaseModel> dataSource,
            IOverridingDataSource<TThreadsafeModel> deletingDataSource,
            Func<TModel, TThreadsafeModel> convertToThreadsafeModel)
            : base(deletingDataSource)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(convertToThreadsafeModel, nameof(convertToThreadsafeModel));

            this.api = api;
            this.dataSource = dataSource;
            this.convertToThreadsafeModel = convertToThreadsafeModel;
        }

        public override IObservable<ITransition> Start(TThreadsafeModel entity)
            => update(entity)
                .SelectMany(tryOverwrite(entity))
                .SelectMany(result => result is IgnoreResult<TModel>
                    ? entityChanged(entity)
                    : succeeded(extractFrom(result)))
                .Catch(Fail(entity));

        private IObservable<TModel> update(TModel entity)
            => entity == null
                ? Observable.Throw<TModel>(new ArgumentNullException(nameof(entity)))
                : api.Update(entity);

        private IObservable<ITransition> entityChanged(TModel entity)
            => Observable.Return(EntityChanged.Transition(entity));

        private Func<TModel, IObservable<IConflictResolutionResult<TThreadsafeModel>>> tryOverwrite(TModel entity)
            => updatedEntity => dataSource.UpdateWithConflictResolution(
            convertToThreadsafeModel(entity), convertToThreadsafeModel(updatedEntity), new OverwriteIfLocalDidNotChange<TModel, TDatabaseModel>(entity));
        
        private TThreadsafeModel extractFrom(IConflictResolutionResult<TThreadsafeModel> result)
        {
            switch (result)
            {
                case CreateResult<TThreadsafeModel> c:
                    return c.Entity;
                case UpdateResult<TThreadsafeModel> u:
                    return u.Entity;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        private IObservable<ITransition> succeeded(TModel entity)
            => Observable.Return((ITransition)UpdatingSucceeded.Transition(entity));
    }
}
