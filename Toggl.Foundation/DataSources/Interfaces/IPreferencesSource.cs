﻿using System;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Models.Interfaces;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.DataSources
{
    public interface IPreferencesSource : IDataSource<IThreadsafePreferences, IDatabasePreferences>
    {
        IObservable<IThreadsafePreferences> Current { get; }

        IObservable<IThreadsafePreferences> Get();

        IObservable<IThreadsafePreferences> Update(EditPreferencesDTO dto);
    }
}
