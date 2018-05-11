using System;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Sync.States
{
    public interface ITestModel : IIdentifiable, ILastChangedDatable, IDeletable
    {
    }

    public interface IDatabaseTestModel : ITestModel, IDatabaseSyncable
    {
    }

    public interface IThreadsafeTestModel : IDatabaseTestModel, IThreadsafeModel
    {
    }
    
    public sealed class TestModel : IThreadsafeTestModel
    {
        public long Id { get; set; }
        
        public DateTimeOffset At { get; set; }
        
        public DateTimeOffset? ServerDeletedAt { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public string LastSyncErrorMessage { get; set; }

        public bool IsDeleted { get; set; }

        public TestModel()
        {
        }

        public TestModel(long id, SyncStatus status, bool deleted = false)
        {
            Id = id;
            SyncStatus = status;
            IsDeleted = deleted;
        }

        public static TestModel Clean(ITestModel testModel)
            => new TestModel(testModel.Id, SyncStatus.InSync) { At = testModel.At };

        public static TestModel Dirty(long id)
            => new TestModel(id, SyncStatus.SyncNeeded);

        public static TestModel DirtyDeleted(long id)
            => new TestModel(id, SyncStatus.SyncNeeded, true);
    }
}
