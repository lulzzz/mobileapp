using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Tests.Mocks;
using Xunit;
using Toggl.PrimeRadiant;

namespace Toggl.Foundation.Tests.Interactors
{
    public class GetProjectsThatFailedToSyncInteractorTests
    {
        public sealed class TheGetProjectsThatFailedToSyncInteractor : BaseInteractorTests
        {
            [Fact, LogIfTooSlow]
            public async Task ReturnsOnlyProjectsThatFailedToSync()
            {
                MockProject[] projects = {
                    new MockProject { Id = 0, SyncStatus = SyncStatus.SyncFailed },
                    new MockProject { Id = 1, SyncStatus = SyncStatus.InSync },
                    new MockProject { Id = 2, SyncStatus = SyncStatus.SyncFailed },
                    new MockProject { Id = 3, SyncStatus = SyncStatus.SyncNeeded },
                    new MockProject { Id = 4, SyncStatus = SyncStatus.InSync },
                    new MockProject { Id = 5, SyncStatus = SyncStatus.SyncFailed }
                };

                int syncFailedCount = projects.Where(p => p.SyncStatus == SyncStatus.SyncFailed).Count();

                Database.Projects.GetAll().Returns(Observable.Return(projects));

                var returnedProjects = await InteractorFactory.GetProjectsThatFailedToSync().Execute();

                returnedProjects.Count().Should().Be(syncFailedCount);
            }
        }
    }
}
