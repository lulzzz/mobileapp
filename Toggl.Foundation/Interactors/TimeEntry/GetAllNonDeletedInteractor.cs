using System;
using System.Collections.Generic;
using Toggl.Foundation.DataSources.Interfaces;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Interactors
{
    internal sealed class GetAllNonDeletedInteractor : IInteractor<IObservable<IEnumerable<IThreadsafeTimeEntry>>>
    {
        private readonly IDataSource<IThreadsafeTimeEntry, IDatabaseTimeEntry> dataSource;

        public GetAllNonDeletedInteractor(IDataSource<IThreadsafeTimeEntry, IDatabaseTimeEntry> dataSource)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            
            this.dataSource = dataSource;
        }
        
        public IObservable<IEnumerable<IThreadsafeTimeEntry>> Execute()
            => dataSource.GetAll(te => !te.IsDeleted);
    }
}
