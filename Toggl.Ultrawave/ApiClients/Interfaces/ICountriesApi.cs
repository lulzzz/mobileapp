using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;

namespace Toggl.Ultrawave.ApiClients
{
    public interface ICountriesApi
    {
        IObservable<List<ICountry>> GetAll();
    }
}
