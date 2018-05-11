using System;

namespace Toggl.Multivac.Models
{
    public interface ICountry : IApiModel, IIdentifiable
    {
        string Name { get; }
        string CountryCode { get; }
    }
}
