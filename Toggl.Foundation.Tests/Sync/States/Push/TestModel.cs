using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Sync.States
{
    public interface ITestModel : IIdentifiable
    {
    }
    
    public interface IDatabaseTestModel : ITestModel, IDatabaseSyncable
    {
    }

    public interface IThreadsafeTestModel : IThreadsafeModel, IDatabaseTestModel
    {
    }
    
    public sealed class TestModel : IDatabaseTestModel
    {
        public long Id { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public TestModel(long id, SyncStatus status, bool deleted = false)
        {
            Id = id;
            SyncStatus = status;
            IsDeleted = deleted;
        }

        public static TestModel Dirty(long id)
            => new TestModel(id, SyncStatus.SyncNeeded);

        public static TestModel DirtyDeleted(long id)
            => new TestModel(id, SyncStatus.SyncNeeded, true);
    }
    
    public sealed class ThreadsafeTestModel : IThreadsafeTestModel
    {
        public long Id { get; set; }
    
        public SyncStatus SyncStatus { get; set; }
    
        public string LastSyncErrorMessage { get; set; }
    
        public bool IsDeleted { get; set; }
    
        public ThreadsafeTestModel(long id, SyncStatus status, bool deleted = false)
        {
            Id = id;
            SyncStatus = status;
            IsDeleted = deleted;
        }
    
        public static IThreadsafeTestModel Clean(ITestModel model)
            => new ThreadsafeTestModel(model.Id, SyncStatus.InSync, false);
    }
}
