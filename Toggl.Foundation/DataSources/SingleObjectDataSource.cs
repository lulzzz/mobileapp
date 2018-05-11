using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.Sync.ConflictResolution;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.DataSources
{
    public abstract class SingleObjectDataSource<TThreadsafe, TDatabase> : ISingleObjectDataSource<TThreadsafe, TDatabase>
        where TDatabase : IDatabaseSyncable
        where TThreadsafe : IThreadsafeModel, IIdentifiable, TDatabase
    {
        private readonly ISingleObjectStorage<TDatabase> storage;
        private readonly ISubject<TThreadsafe> currentSubject;

        private IDisposable initializationDisposable;
    
        public IObservable<TThreadsafe> Current { get; }

        public SingleObjectDataSource(ISingleObjectStorage<TDatabase> storage, TThreadsafe defaultCurrentValue)
        {
            Ensure.Argument.IsNotNull(storage, nameof(storage));

            this.storage = storage;

            currentSubject = new BehaviorSubject<TThreadsafe>(defaultCurrentValue);

            Current = currentSubject.AsObservable();

            // right after login/signup the database does not contain the preferences and retreiving
            // it will fail we can ignore this error because it will be immediately fetched and until
            // then the default preferences will be used
            initializationDisposable = storage.Single()
                .Select(Convert)
                .Subscribe(currentSubject.OnNext, (Exception _) => { });
        }

        public virtual IObservable<TThreadsafe> Get()
            => storage.Single().Select(Convert);

        public virtual IObservable<TThreadsafe> GetById(long id)
            => storage.GetById(id).Select(Convert);

        public virtual IObservable<TThreadsafe> Create(TThreadsafe entity)
            => storage.Create(entity)
                .Select(Convert)
                .Do(currentSubject.OnNext);

        public virtual IObservable<TThreadsafe> Update(TThreadsafe entity)
            => storage.Update(entity)
                .Select(Convert)
                .Do(currentSubject.OnNext);

        public virtual IObservable<TThreadsafe> Overwrite(TThreadsafe original, TThreadsafe entity)
            => storage.Update(original.Id, entity)
                .Select(Convert)
                .Do(currentSubject.OnNext);

        public virtual IObservable<Unit> Delete(long id)
            => storage.Delete();

        public IObservable<IConflictResolutionResult<TThreadsafe>> UpdateWithConflictResolution(
            TThreadsafe original,
            TThreadsafe entity,
            IConflictResolver<TDatabase> conflictResolver = null,
            IRivalsResolver<TDatabase> rivalsResolver = null)
        {
            var conflictResolution = conflictResolver != null ? (Func<TDatabase, TDatabase, ConflictResolutionMode>)conflictResolver.Resolve : ConflictResolution;
            return storage.UpdateWithConflictResolution(original.Id, entity, conflictResolution, rivalsResolver)
                .Select(result => result.ToThreadSafeResult(Convert))
                .Do(processConflictResultionResult);
        }

        public IObservable<IEnumerable<IConflictResolutionResult<TThreadsafe>>> BatchUpdate(IEnumerable<TThreadsafe> entities)
            => storage.BatchUpdate(
                    entities.Select(entity => (entity.Id, (TDatabase)entity)),
                    ConflictResolution)
                .ToThreadSafeResult(Convert)
                .Select(results => results.FirstOrDefault())
                .Do(processConflictResultionResult);

        public IObservable<IEnumerable<TThreadsafe>> GetAll()
            => storage.GetAll().Select(preferences => preferences.Select(Convert));

        public IObservable<IEnumerable<TThreadsafe>> GetAll(Func<TDatabase, bool> predicate)
            => storage.GetAll(predicate).Select(entities => entities.Select(Convert));

        protected abstract TThreadsafe Convert(TDatabase entity);

        protected abstract ConflictResolutionMode ConflictResolution(TDatabase first, TDatabase second);
        
        private void processConflictResultionResult(IConflictResolutionResult<TThreadsafe> result)
        {
            if (result == null) return;

            switch (result)
            {
                case CreateResult<TThreadsafe> created:
                    currentSubject.OnNext(created.Entity);
                    break;

                case UpdateResult<TThreadsafe> updated:
                    currentSubject.OnNext(updated.Entity);
                    break;
            }
        }
    }
}
