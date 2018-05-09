using System.Reactive.Linq;
using FluentAssertions;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace Toggl.Ultrawave.Tests.Integration
{
    public sealed class CountriesApiTests
    {
        public sealed class TheGetAllMethod : EndpointTestBase
        {
            [Fact, LogTestInfo]
            public async ThreadingTask ReturnsAllCountries()
            {
                var togglClient = TogglApiWith(Credentials.None);
                var countries = await togglClient.Countries.GetAll();

                countries.Should().AllBeOfType<Country>();
                countries.Should().NotBeEmpty();
            }
        }
    }
}
