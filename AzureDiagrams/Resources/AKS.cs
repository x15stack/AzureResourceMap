﻿using System;
using System.Linq;

namespace DrawIo.Azure.Core.Resources;

internal class AKS : AzureResource, IUseManagedIdentities
{
    public Identity? Identity { get; set; }
    public override string Image => "img/lib/azure2/containers/Kubernetes_Services.svg";

    public bool DoYouUseThisUserAssignedClientId(string id)
    {
        return Identity?.UserAssignedIdentities?.Keys.Any(k =>
            string.Compare(k, id, StringComparison.InvariantCultureIgnoreCase) == 0) ?? false;
    }

    public void CreateManagedIdentityFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity)
    {
        CreateFlowTo(userAssignedManagedIdentity, "AAD Identity", FlowEmphasis.LessImportant);
    }
}