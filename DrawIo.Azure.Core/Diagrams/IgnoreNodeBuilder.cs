﻿using System.Collections.Generic;
using DrawIo.Azure.Core.Resources;
using Microsoft.Msagl.Core.Layout;

namespace DrawIo.Azure.Core.Diagrams;

internal class IgnoreNodeBuilder : AzureResourceNodeBuilder
{
    protected override IEnumerable<(AzureResource, Node)> CreateNodesInternal(IDictionary<AzureResource, AzureResourceNodeBuilder> resourceNodeBuilders)
    {
        yield break;
    }

    public IgnoreNodeBuilder(AzureResource resource) : base(resource)
    {
    }
}