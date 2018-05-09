using System;

namespace Toggl.Multivac.Models
{
    public interface ICountry : IBaseModel
    {
        string Name { get; }
        string CountryCode { get; }
    }
}
