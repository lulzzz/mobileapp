using System;
using Toggl.Multivac.Models;

namespace Toggl.Multivac.Extensions
{
    public static class ApiModelExtensions
    {
        public static bool HasChanged(this IApiModel apiModel, IApiModel originalApiModel)
            => apiModel is ILastChangedDatable lastChangeDatable
               && originalApiModel is ILastChangedDatable originalLastChangeDatable
                ? originalLastChangeDatable.At > lastChangeDatable.At
                : !propertiesMatch(apiModel, originalApiModel);

        private static bool propertiesMatch(IApiModel first, IApiModel second)
        {
            if (first.GetType() != second.GetType())
                throw new ArgumentException($"Cannot check if the properties are the same for objects of different types ({first.GetType()}, {second.GetType()})");

            foreach (var property in first.GetType().GetProperties())
            {
                if (!property.GetValue(first, null).Equals(property.GetValue(second, null)))
                    return false;
            }
            
            return true;
        }
    }
}
