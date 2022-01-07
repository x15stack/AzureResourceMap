﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Diagrams;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core.Resources;

public class VNet : AzureResource
{
    public Subnet[] Subnets { get; private set; } = default!;
    public List<PrivateDnsZone> PrivateDnsZones { get; } = new();

    public override string Image => "img/lib/azure2/networking/Virtual_Networks.svg";

    public override AzureResourceNodeBuilder CreateNodeBuilder()
    {
        return new VNetDiagramResourceBuilder(this);
    }

    public override Task Enrich(JObject full, Dictionary<string, JObject> additionalResources)
    {
        Subnets = full["properties"]!["subnets"]!.Select(x => new Subnet
        {
            Name = x.Value<string>("name")!,
            AddressPrefix = x["properties"]!.Value<string>("addressPrefix")!
        }).ToArray();

        return Task.CompletedTask;
    }

    public void AssignPrivateDnsZone(PrivateDnsZone resource)
    {
        PrivateDnsZones.Add(resource);
    }

    private void InjectResourceInto(AzureResource resource, string subnet)
    {
        Subnets.Single(x => string.Compare(x.Name, subnet, StringComparison.InvariantCultureIgnoreCase) == 0).ContainedResources.Add(resource);
        resource.ContainedByAnotherResource = true;
    }

    public void AssignNsg(NSG nsg, string subnet)
    {
        Subnets.Single(x => x.Name == subnet).NSGs.Add(nsg);
        nsg.ContainedByAnotherResource = true;
    }

    public override void BuildRelationships(IEnumerable<AzureResource> allResources)
    {
        allResources
            .OfType<ICanInjectIntoASubnet>()
            .Select(x => (resource: (AzureResource)x, subnets: SubnetsInsideThisVNet(x.SubnetIdsIAmInjectedInto)))
            .ForEach(r =>
                r.subnets.ForEach(s => InjectResourceInto(r.resource, s)));
    }

    private IEnumerable<string> SubnetsInsideThisVNet(string[] subnetIdsIAmInjectedInto)
    {
        return subnetIdsIAmInjectedInto.Where(x =>
            string.Compare(Id, string.Join('/', x.Split('/')[..^2]), StringComparison.InvariantCultureIgnoreCase) == 0)
            .Select(x => x.Split('/')[^1]);
    }

    public class Subnet
    {
        public string Name { get; init; } = default!;
        public string AddressPrefix { get; init; } = default!;
        internal List<AzureResource> ContainedResources { get; } = new();

        public List<NSG> NSGs { get; } = new();
    }

    /// <summary>
    /// VMs can be associated to multiple nics, in different subnets. So instead of homing it in a subnet, I home it inside the vnet.
    /// </summary>
    /// <param name="vm"></param>
    public void GiveHomeToVirtualMachine(VM vm)
    {
        OwnsResource(vm);
    }
}