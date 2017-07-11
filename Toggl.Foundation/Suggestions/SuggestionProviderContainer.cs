﻿using System;
using System.Collections.Immutable;
using Toggl.Multivac;

namespace Toggl.Foundation.Suggestions
{
    public class SuggestionProviderContainer : ISuggestionProviderContainer
    {
        public ImmutableList<ISuggestionProvider> Providers { get; }

        public SuggestionProviderContainer(params ISuggestionProvider[] providers)
        {
            Ensure.Argument.IsNotNull(providers, nameof(providers));
            if (providers.Length == 0)
                throw new ArgumentException("At least one ISuggestionProvider is required", nameof(providers));

            Providers = ImmutableList.Create(providers);
        }
    }
}