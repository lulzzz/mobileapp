﻿using System;
using System.Reactive.Linq;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Sync.States.Push
{
    internal sealed class PushOneEntityState<TThreadsafeModel>
        where TThreadsafeModel : class, IIdentifiable, IDatabaseSyncable, IThreadsafeModel
    {
        public StateResult<TThreadsafeModel> CreateEntity { get; } = new StateResult<TThreadsafeModel>();

        public StateResult<TThreadsafeModel> DeleteEntity { get; } = new StateResult<TThreadsafeModel>();

        public StateResult<TThreadsafeModel> UpdateEntity { get; } = new StateResult<TThreadsafeModel>();
        
        public StateResult<TThreadsafeModel> DeleteEntityLocally { get; } = new StateResult<TThreadsafeModel>();

        public IObservable<ITransition> Start(TThreadsafeModel entityToPush)
            => createObservable(entityToPush)
                .Select(entity =>
                    entity.IsDeleted
                        ? wasNotPublished(entity)
                            ? deleteLocally(entity)
                            : delete(entity)
                        : wasNotPublished(entity)
                            ? create(entity)
                            : update(entity));

        private IObservable<TThreadsafeModel> createObservable(TThreadsafeModel entity)
            => entity == null
                ? Observable.Throw<TThreadsafeModel>(new ArgumentNullException(nameof(entity)))
                : Observable.Return(entity);

        private bool wasNotPublished(TThreadsafeModel entity)
            => entity.Id < 0;

        private ITransition delete(TThreadsafeModel entity) => DeleteEntity.Transition(entity);

        private ITransition create(TThreadsafeModel entity) => CreateEntity.Transition(entity);

        private ITransition update(TThreadsafeModel entity) => UpdateEntity.Transition(entity);

        private ITransition deleteLocally(TThreadsafeModel entity) => DeleteEntityLocally.Transition(entity);
    }
}
