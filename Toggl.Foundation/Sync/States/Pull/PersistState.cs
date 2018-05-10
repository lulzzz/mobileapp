﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class PersistState<TInterface, TDatabaseInterface, TThreadsafeInterface>
        where TDatabaseInterface : TInterface
        where TThreadsafeInterface : TDatabaseInterface, IThreadsafeModel
    {
        private readonly IDataSource<TThreadsafeInterface, TDatabaseInterface> dataSource;

        private readonly ISinceParameterRepository sinceParameterRepository;

        private readonly Func<TInterface, TThreadsafeInterface> convertToThreadsafeEntity;

        public StateResult<FetchObservables> FinishedPersisting { get; } = new StateResult<FetchObservables>();

        public StateResult<Exception> Failed { get; } = new StateResult<Exception>();

        public PersistState(
            IDataSource<TThreadsafeInterface, TDatabaseInterface> dataSource,
            ISinceParameterRepository sinceParameterRepository,
            Func<TInterface, TThreadsafeInterface> convertToThreadsafeEntity)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(sinceParameterRepository, nameof(sinceParameterRepository));
            Ensure.Argument.IsNotNull(convertToThreadsafeEntity, nameof(convertToThreadsafeEntity));
            
            this.dataSource = dataSource;
            this.sinceParameterRepository = sinceParameterRepository;
            this.convertToThreadsafeEntity = convertToThreadsafeEntity;
        }

        public IObservable<ITransition> Start(FetchObservables fetch)
            => fetch.GetByType<TInterface>()
                .SingleAsync()
                .Select(entities => entities?.ToList() ?? new List<TInterface>())
                .Select(entities => entities.Select(convertToThreadsafeEntity).ToList())
                .SelectMany(threadsafeEntities =>
                    dataSource.BatchUpdate(threadsafeEntities)
                        .IgnoreElements()
                        .OfType<List<TThreadsafeInterface>>()
                        .Concat(Observable.Return(threadsafeEntities)))
                .Do(maybeUpdateSinceDates)
                .Select(_ => FinishedPersisting.Transition(fetch))
                .Catch((Exception exception) => processError(exception));

        private IObservable<ITransition> processError(Exception exception)
            => shouldRethrow(exception)
                ? Observable.Throw<ITransition>(exception)
                : Observable.Return(Failed.Transition(exception));

        private bool shouldRethrow(Exception e)
            => e is ApiException == false || e is ApiDeprecatedException || e is ClientDeprecatedException || e is UnauthorizedException;

        private void maybeUpdateSinceDates(IEnumerable<TThreadsafeInterface> entities)
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
    }
}
