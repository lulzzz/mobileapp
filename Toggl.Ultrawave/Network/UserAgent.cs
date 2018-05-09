using Toggl.Multivac;

namespace Toggl.Ultrawave.Network
{
    public sealed class UserAgent
    {
        public string Name { get; }

        public string Version { get; }

        public UserAgent(string agentName, string version)
        {
            Ensure.Argument.IsNotNullOrWhiteSpaceString(version, nameof(version));
            Ensure.Argument.IsNotNullOrWhiteSpaceString(agentName, nameof(agentName));

            Name = agentName;
            Version = version;
        }

        public override string ToString() => $"{Name}/{Version}";
    }
}
