using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    public abstract class BasePushEntityState<TModel, TDatabaseModel, TThreadsafeModel>
        where TDatabaseModel : class, TModel, IDatabaseSyncable
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        public StateResult<(Exception, TThreadsafeModel)> ServerError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        public StateResult<(Exception, TThreadsafeModel)> ClientError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        public StateResult<(Exception, TThreadsafeModel)> UnknownError { get; } = new StateResult<(Exception, TThreadsafeModel)>();

        protected IDataSource<TThreadsafeModel, TDatabaseModel> DataSource { get; }

        public BasePushEntityState(IDataSource<TThreadsafeModel, TDatabaseModel> dataSource)
        {
            DataSource = dataSource;
        }

        protected Func<TThreadsafeModel, IObservable<TThreadsafeModel>> Overwrite(TThreadsafeModel entity)
            => pushedEntity => DataSource.Overwrite(entity, pushedEntity);

        protected Func<Exception, IObservable<ITransition>> Fail(TThreadsafeModel entity)
            => exception => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(failTransition(entity, exception));

        private bool shouldRethrow(Exception e)
            => e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException || e is OfflineException;

        private ITransition failTransition(TThreadsafeModel entity, Exception e)
            => e is ServerErrorException
                ? ServerError.Transition((e, entity))
                : e is ClientErrorException
                    ? ClientError.Transition((e, entity))
                    : UnknownError.Transition((e, entity));

        public abstract IObservable<ITransition> Start(TThreadsafeModel entity);
    }
}
