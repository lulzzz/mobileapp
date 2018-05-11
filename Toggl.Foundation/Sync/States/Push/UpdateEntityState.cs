using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.States.Push.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.ApiClients;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class UpdateEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        : BasePushEntityState<TModel, TDatabaseModel, TThreadsafeModel>, IUpdateEntityState<TModel, TThreadsafeModel>
        where TModel : IApiModel
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        private readonly IUpdatingApiClient<TModel> api;

        private readonly Func<TModel, TThreadsafeModel> convertToThreadsafeModel;
        
        public StateResult<TModel> EntityChanged { get; } = new StateResult<TModel>();

        public StateResult<TModel> UpdatingSucceeded { get; } = new StateResult<TModel>();

        public UpdateEntityState(
            IUpdatingApiClient<TModel> api,
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
            => updatedEntity => DataSource.UpdateWithConflictResolution(
                convertToThreadsafeModel(entity), convertToThreadsafeModel(updatedEntity), overwriteIfLocalEntityDidNotChange(entity));

        private Func<TModel, TModel, ConflictResolutionMode> overwriteIfLocalEntityDidNotChange(TModel local)
            => (currentLocal, _) => local.HasChanged(currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;
        
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
