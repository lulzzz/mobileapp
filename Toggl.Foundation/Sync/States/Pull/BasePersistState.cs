using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal abstract class BasePersistState<TInterface, TDatabaseInterface>
        where TDatabaseInterface : TInterface
    {
        private readonly IRepository<TDatabaseInterface> repository;

        private readonly ISinceParameterRepository sinceParameterRepository;

        private readonly IConflictResolver<TDatabaseInterface> conflictResolver;

        private readonly IRivalsResolver<TDatabaseInterface> rivalsResolver;

        public StateResult<FetchObservables> FinishedPersisting { get; } = new StateResult<FetchObservables>();

        public StateResult<Exception> Failed { get; } = new StateResult<Exception>();

        protected BasePersistState(
            IRepository<TDatabaseInterface> repository,
            ISinceParameterRepository sinceParameterRepository,
            IConflictResolver<TDatabaseInterface> conflictResolver,
            IRivalsResolver<TDatabaseInterface> rivalsResolver = null)
        {
            this.repository = repository;
            this.sinceParameterRepository = sinceParameterRepository;
            this.conflictResolver = conflictResolver;
            this.rivalsResolver = rivalsResolver;
        }

        public IObservable<ITransition> Start(FetchObservables fetch)
        {
            return FetchObservable(fetch)
                .SingleAsync()
                .Select(entities => entities ?? new List<TInterface>())
                .Select(entities => entities.Select(ConvertToDatabaseEntity).ToList())
                .SelectMany(databaseEntities =>
                    repository.BatchUpdate(databaseEntities.Select(entity => (GetId(entity), entity)), conflictResolver.Resolve, rivalsResolver)
                        .IgnoreElements()
                        .OfType<List<TDatabaseInterface>>()
                        .Concat(Observable.Return(databaseEntities)))
                .Do(maybeUpdateSinceDates)
                .Select(_ => FinishedPersisting.Transition(fetch))
                .Catch((Exception exception) => processError(exception));
        }

        private IObservable<ITransition> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(Failed.Transition(exception));

        private bool shouldRethrow(Exception e)
            => e is ApiException == false || e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException;

        private void maybeUpdateSinceDates(IEnumerable<TDatabaseInterface> entities)
        {
            if (sinceParameterRepository.Supports<TDatabaseInterface>())
            {   
                var since = entities.OfType<ILastChangedDatable>()
                    .DefaultIfEmpty(null)
                    .Max(entity => entity?.At);

                if (since.HasValue)
                {
                    sinceParameterRepository.Set<TDatabaseInterface>(since);
                }
            }
        }

        protected abstract long GetId(TDatabaseInterface entity);

        protected abstract IObservable<IEnumerable<TInterface>> FetchObservable(FetchObservables fetch);

        protected abstract TDatabaseInterface ConvertToDatabaseEntity(TInterface entity);
    }
}
