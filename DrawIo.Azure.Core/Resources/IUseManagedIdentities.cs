﻿namespace DrawIo.Azure.Core.Resources;

public interface IUseManagedIdentities
{
    bool DoYouUseThisUserAssignedClientId(string clientId);
    void CreateFlowBackToMe(UserAssignedManagedIdentity userAssignedManagedIdentity);
}

public interface ICanInjectIntoASubnet
{
    string[] SubnetIdsIAmInjectedInto { get; }
}