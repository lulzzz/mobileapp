﻿using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;

namespace Toggl.Ultrawave.ApiClients
{
    internal sealed class TimeEntriesApi : BaseApi, ITimeEntriesApi
    {
        private readonly TimeEntryEndpoints endPoints;

        public TimeEntriesApi(TimeEntryEndpoints endPoints, IApiClient apiClient, IJsonSerializer serializer, Credentials credentials)
            : base(apiClient, serializer, credentials)
        {
            this.endPoints = endPoints;
        }

        public IObservable<List<ITimeEntry>> GetAll()
            => CreateListObservable<TimeEntry, ITimeEntry>(endPoints.Get, AuthHeader);

        public IObservable<List<ITimeEntry>> GetAllSince(DateTimeOffset threshold)
            => CreateListObservable<TimeEntry, ITimeEntry>(endPoints.GetSince(threshold), AuthHeader);

        public IObservable<ITimeEntry> Create(ITimeEntry timeEntry)
            => pushTimeEntry(endPoints.Post(timeEntry.WorkspaceId), timeEntry, SerializationReason.Post);

        public IObservable<ITimeEntry> Update(ITimeEntry timeEntry)
            => pushTimeEntry(endPoints.Put(timeEntry.WorkspaceId, timeEntry.Id), timeEntry, SerializationReason.Default);

        private IObservable<ITimeEntry> pushTimeEntry(Endpoint endPoint, ITimeEntry timeEntry, SerializationReason reason)
        {
            var timeEntryCopy = timeEntry as TimeEntry ?? new TimeEntry(timeEntry);
            var observable = CreateObservable(endPoint, AuthHeader, timeEntryCopy, reason);
            return observable;
        }
    }
}
