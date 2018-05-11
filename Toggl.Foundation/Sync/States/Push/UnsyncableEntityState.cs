﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class UnsyncableEntityState<TDatabaseModel, TThreadsafeModel>
        where TDatabaseModel : IDatabaseSyncable, IApiModel
        where TThreadsafeModel : TDatabaseModel, IThreadsafeModel
    {
        private readonly IDataSource<TThreadsafeModel, TDatabaseModel> dataSource;

        private readonly Func<TThreadsafeModel, string, TThreadsafeModel> createUnsyncableFrom;
        
        public StateResult<TThreadsafeModel> MarkedAsUnsyncable { get; } = new StateResult<TThreadsafeModel>();

        public UnsyncableEntityState(
            IDataSource<TThreadsafeModel, TDatabaseModel> dataSource,
            Func<TThreadsafeModel, string, TThreadsafeModel> createUnsyncableFrom)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(createUnsyncableFrom, nameof(createUnsyncableFrom));
            
            this.dataSource = dataSource;
            this.createUnsyncableFrom = createUnsyncableFrom;
        }

        public IObservable<ITransition> Start((Exception Reason, TThreadsafeModel Entity) failedPush)
            => failedPush.Reason == null || failedPush.Entity == null
                ? failBecauseOfNullArguments(failedPush)
                : failedPush.Reason is ApiException apiException
                    ? markAsUnsyncable(failedPush.Entity, apiException.LocalizedApiErrorMessage)
                    : failBecauseOfUnexpectedError(failedPush.Reason);

        private IObservable<ITransition> failBecauseOfNullArguments((Exception Reason, TThreadsafeModel Entity) failedPush)
            => Observable.Throw<Transition<TThreadsafeModel>>(new ArgumentNullException(
                failedPush.Reason == null
                    ? nameof(failedPush.Reason)
                    : nameof(failedPush.Entity)));

        private IObservable<ITransition> failBecauseOfUnexpectedError(Exception reason)
            => Observable.Throw<Transition<TThreadsafeModel>>(reason);

        private IObservable<ITransition> markAsUnsyncable(TThreadsafeModel entity, string reason)
            => dataSource
                .UpdateWithConflictResolution(entity, createUnsyncableFrom(entity, reason), overwriteIfLocalEntityDidNotChange(entity))
                .Select(updated => updated is UpdateResult<TThreadsafeModel> updateResult ? updateResult.Entity : entity)
                .Select(unsyncable => MarkedAsUnsyncable.Transition(unsyncable));

        private Func<TThreadsafeModel, TThreadsafeModel, ConflictResolutionMode> overwriteIfLocalEntityDidNotChange(TThreadsafeModel local)
            => (currentLocal, _) => local.HasChanged(currentLocal)
                ? ConflictResolutionMode.Ignore
                : ConflictResolutionMode.Update;
    }
}
