using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources
{
    public abstract class DataSource<T, U> : IDataSource<T, U>
        where U : IDatabaseSyncable
        where T : IThreadsafeModel, IIdentifiable, U
    {
        protected readonly IRepository<U> Repository;

        protected virtual IRivalsResolver<U> RivalsResolver { get; } = null;

        protected DataSource(IRepository<U> repository)
        {
            Ensure.Argument.IsNotNull(repository, nameof(repository));

            Repository = repository;
        }

        public virtual IObservable<IEnumerable<T>> GetAll()
            => Repository.GetAll().Select(entities => entities.Select(Convert));

        public virtual IObservable<IEnumerable<T>> GetAll(Func<U, bool> predicate)
            => Repository.GetAll(predicate).Select(entities => entities.Select(Convert));

        public virtual IObservable<T> Create(T entity)
            => Repository.Create(entity).Select(Convert);

        public virtual IObservable<T> Update(T entity)
            => Update(entity.Id, entity);
        
        public virtual IObservable<T> Update(long id, T entity)
            => Repository.Update(id, entity).Select(Convert);

        public virtual IObservable<Unit> Delete(long id)
            => Repository.Delete(id);

        public virtual IObservable<IConflictResolutionResult<T>> UpdateWithConflictResolution(
            long id,
            T entity,
            IConflictResolver<U> conflictResolver = null,
            IRivalsResolver<U> rivalsResolver = null)
        {
            var conflictResolution = conflictResolver != null ? (Func<U, U, ConflictResolutionMode>)conflictResolver.Resolve : ResolveConflicts;
            return Repository.UpdateWithConflictResolution(id, entity, conflictResolution, rivalsResolver);
        }

        public virtual IObservable<IEnumerable<IConflictResolutionResult<T>>> BatchUpdate(IEnumerable<T> entities)
            => Repository.BatchUpdate(
                    convertEntitiesForBatchUpdate(entities),
                    ResolveConflicts,
                    RivalsResolver)
                .ToThreadSafeResult(Convert);

        public IObservable<T> GetById(long id)
            => Repository.GetById(id).Select(Convert);

        private IEnumerable<(long, U)> convertEntitiesForBatchUpdate(
            IEnumerable<T> entities)
            => entities.Select(entity => (entity.Id, (U)entity));

        protected abstract T Convert(U entity);

        protected abstract ConflictResolutionMode ResolveConflicts(U first, U second);
    }
}
