﻿namespace Toggl.PrimeRadiant
{
    public sealed class CreateResult<T> : IConflictResolutionResult<T>
    {
        public T Entity { get; }

        public CreateResult(T entity)
        {
            Entity = entity;
        }
    }
}
