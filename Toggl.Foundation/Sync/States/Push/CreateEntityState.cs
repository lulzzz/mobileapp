﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class CreateEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        : BasePushEntityState<TThreadsafeModel>
        where TModel : IIdentifiable
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        private readonly ICreatingApiClient<TModel> api;

        private readonly Func<TModel, TThreadsafeModel> convertToThreadsafeModel;

        public StateResult<TThreadsafeModel> CreatingFinished { get; } = new StateResult<TThreadsafeModel>();

        public CreateEntityState(
            ICreatingApiClient<TModel> api,
            IDataSource<TThreadsafeModel, TDatabaseModel> dataSource,
            Func<TModel, TThreadsafeModel> convertToThreadsafeModel)
            : base(dataSource)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(convertToThreadsafeModel, nameof(convertToThreadsafeModel));

            this.api = api;
            this.convertToThreadsafeModel = convertToThreadsafeModel;
        }

        public override IObservable<ITransition> Start(TThreadsafeModel entity)
            => create(entity)
                .SelectMany(Overwrite(entity))
                .Select(CreatingFinished.Transition)
                .Catch(Fail(entity));

        private IObservable<TThreadsafeModel> create(TThreadsafeModel entity)
            => entity == null
                ? Observable.Throw<TThreadsafeModel>(new ArgumentNullException(nameof(entity)))
                : api.Create(entity).Select(convertToThreadsafeModel);
    }
}
