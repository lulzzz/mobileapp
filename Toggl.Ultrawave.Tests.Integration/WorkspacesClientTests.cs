﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Tests.Integration.Helper;
using Xunit;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Toggl.Ultrawave.Models;

namespace Toggl.Ultrawave.Tests.Integration
{
    public class WorkspacesClientTests
    {
        public class TheGetMethod : AuthenticatedEndpointBaseTests<List<Workspace>>
        {
            protected override IObservable<List<Workspace>> CallEndpointWith(ITogglApi togglApi)
                => togglApi.Workspaces.GetAll();

            [Fact]
            public async Task ReturnsAllWorkspaces()
            {
                var (togglClient, user) = await SetupTestUser();
                var secondWorkspace = await WorkspaceHelper.CreateFor(user);

                var workspaces = await CallEndpointWith(togglClient);

                workspaces.Should().HaveCount(2);
                workspaces.Should().Contain(ws => ws.Id == user.DefaultWorkspaceId);
                workspaces.Should().Contain(ws => ws.Id == secondWorkspace.Id);
            }
        }

        public class TheGetByIdMethod : AuthenticatedGetEndpointBaseTests<Workspace>
        {
            protected override IObservable<Workspace> CallEndpointWith(ITogglApi togglApi)
                => Observable.Defer(async () =>
                {
                    var user = await togglApi.User.Get();
                    return CallEndpointWith(togglApi, user.DefaultWorkspaceId);
                });

            private Func<Task> CallingEndpointWith(ITogglApi togglApi, int id)
                => async () => await CallEndpointWith(togglApi, id);

            private IObservable<Workspace> CallEndpointWith(ITogglApi togglApi, int id)
                => togglApi.Workspaces.GetById(id);

            [Fact]
            public async Task ReturnsDefaultWorkspace()
            {
                var (togglClient, user) = await SetupTestUser();

                var workspace = await CallEndpointWith(togglClient, user.DefaultWorkspaceId);

                workspace.Id.Should().Be(user.DefaultWorkspaceId);
            }

            [Fact]
            public async Task ReturnsCreatedWorkspace()
            {
                var (togglClient, user) = await SetupTestUser();
                var secondWorkspace = await WorkspaceHelper.CreateFor(user);

                var workspace = await CallEndpointWith(togglClient, secondWorkspace.Id);

                workspace.Id.Should().Be(secondWorkspace.Id);
                workspace.Name.Should().Be(secondWorkspace.Name);
            }

            [Fact]
            public async Task FailsForWrongWorkspaceId()
            {
                var (togglClient, user) = await SetupTestUser();

                CallingEndpointWith(togglClient, user.DefaultWorkspaceId - 1).ShouldThrow<ApiException>();
            }
        }
    }
}
