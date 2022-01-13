﻿using System;
using System.Collections.Generic;
using System.Linq;
using DrawIo.Azure.Core.Diagrams;
using DrawIo.Azure.Core.Resources.Retrievers;

namespace DrawIo.Azure.Core.Resources;

public class ASP : AzureResource
{
    public override string Image => "img/lib/azure2/app_services/App_Service_Plans.svg";

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new AppServicePlanAppNodeBuilder(this);
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        var apps = allResources.OfType<App>().Where(x =>
            string.Equals(Id, x.ServerFarmId, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        apps.ForEach(OwnsResource);
        base.BuildRelationships(allResources);
    }
}