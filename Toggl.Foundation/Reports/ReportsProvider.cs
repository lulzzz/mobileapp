using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Foundation.Helper;
using Toggl.Foundation.Models;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Models;
using Toggl.Multivac.Models.Reports;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave;
using Toggl.Ultrawave.ApiClients;

namespace Toggl.Foundation.Reports
{
    public sealed class ReportsProvider : IReportsProvider
    {
        private readonly object listLock = new object();

        private readonly IProjectsApi projectsApi;
        private readonly IProjectsSummaryApi projectSummaryApi;
        private readonly IRepository<IDatabaseProject> projectsRepository;
        private readonly IRepository<IDatabaseClient> clientsRepository;

        private readonly IList<IProject> memoryCache = new List<IProject>();

        public ReportsProvider(ITogglApi api, ITogglDatabase database)
        {
            Ensure.Argument.IsNotNull(api, nameof(api));
            Ensure.Argument.IsNotNull(database, nameof(database));

            projectsApi = api.Projects;
            projectsRepository = database.Projects;
            clientsRepository = database.Clients;
            projectSummaryApi = api.ProjectsSummary;
        }

        public IObservable<ProjectSummaryReport> GetProjectSummary(
            long workspaceId, DateTimeOffset startDate, DateTimeOffset? endDate)
            => projectSummaryApi
                .GetByWorkspace(workspaceId, startDate, endDate)
                .SelectMany(response => summaryReportFromResponse(response, workspaceId));

        private IObservable<ProjectSummaryReport> summaryReportFromResponse(IProjectsSummary response, long workspaceId)
            => response.ProjectsSummaries
                    .Select(s => s.ProjectId)
                    .SelectNonNulls()
                    .Apply(ids => searchProjects(workspaceId, ids.ToArray()))
                    .Select(projectsInReport => 
                    {
                        var totalSeconds = response.ProjectsSummaries.Select(s => s.TrackedSeconds).Sum();
                        return response.ProjectsSummaries
                            .Select(summary =>
                            {
                                var project = projectsInReport.FirstOrDefault(p => p.Id == summary.ProjectId);
                                return findClient(project)
                                    .Select(client => chartFromSummary(summary, project, client, totalSeconds));
                            });
                    })
                    .SelectMany(segmentsObservable => segmentsObservable.Merge())
                    .ToArray()
                    .Select(segments => segments.OrderByDescending(c => c.Percentage).ToArray())
                    .Select(segments => new ProjectSummaryReport(segments));

        private IObservable<IClient> findClient(IProject project)
            => project != null && project.ClientId.HasValue
                ? clientsRepository.GetAll()
                    .SelectMany(clients => clients)
                    .Where(c => c.Id == project.ClientId.Value)
                    .FirstOrDefaultAsync()
                : Observable.Return<IClient>(null);

        private ChartSegment chartFromSummary(
            IProjectSummary summary, 
            IProject project,
            IClient client,
            long totalSeconds)
        {
            var percentage = totalSeconds == 0 ? 0 : (summary.TrackedSeconds / (float)totalSeconds) * 100;
            var billableSeconds = summary.BillableSeconds ?? 0;

            return project == null
                ? new ChartSegment(Resources.NoProject, null, percentage,  summary.TrackedSeconds, billableSeconds, Color.NoProject)
                : new ChartSegment(project.Name, client?.Name, percentage, summary.TrackedSeconds, billableSeconds, project.Color);
        }

        private IObservable<IList<IProject>> searchProjects(long workspaceId, long[] projectIds) =>
            Observable.DeferAsync(async cancellationToken =>
            {
                if (projectIds.Length == 0)
                    return Observable.Return(new List<IProject>());

                var projectsInDatabase = await projectIds
                    .Select(tryGetProjectFromDatabase)
                    .Aggregate(Observable.Merge)
                    .ToList();

                var notInDatabaseIds = projectsInDatabase.SelectAllLeft().ToArray();
                var databaseProjectsObservable =
                    Observable.Return(projectsInDatabase.SelectAllRight().ToList());

                return notInDatabaseIds.Length == 0
                    ? databaseProjectsObservable
                    : databaseProjectsObservable
                        .Merge(searchMemoryAndApi(workspaceId, notInDatabaseIds))
                        .SelectMany(list => list)
                        .ToList();
            });

        private IObservable<Either<long, IProject>> tryGetProjectFromDatabase(long id)
            => projectsRepository.GetById(id)
                .Select(Project.From)
                .Select(Either<long, IProject>.WithRight)
                .Catch(Observable.Return(Either<long, IProject>.WithLeft(id)));

        private IObservable<IList<IProject>> searchMemoryAndApi(long workspaceId, long[] projectIds) =>
            Observable.Defer<IList<IProject>>(() =>
            {
                lock (listLock)
                {
                    var projectsInMemory = projectIds.Select(tryGetProjectFromMemoryCache);

                    var notInMemoryIds = projectsInMemory.SelectAllLeft().ToArray();
                    var memoryProjectsObservable =
                        Observable.Return(projectsInMemory.SelectAllRight().ToList());

                    return notInMemoryIds.Length == 0
                        ? memoryProjectsObservable
                        : memoryProjectsObservable
                            .Merge(searchApi(workspaceId, notInMemoryIds))
                            .SelectMany(list => list)
                            .ToList();
                }
            });

        private Either<long, IProject> tryGetProjectFromMemoryCache(long id)
        {
            var project = memoryCache.FirstOrDefault(p => p.Id == id);
            return project == null
                ? Either<long, IProject>.WithLeft(id)
                : Either<long, IProject>.WithRight(project);
        }

        private IObservable<List<IProject>> searchApi(long workspaceId, long[] projectIds)
            => projectsApi.Search(workspaceId, projectIds)
                .Do(persistInMemoryCache);

        private void persistInMemoryCache(List<IProject> apiProjects)
        {
            lock (listLock)
            {
                apiProjects.ForEach(memoryCache.Add);
            }
        }
    }
}
