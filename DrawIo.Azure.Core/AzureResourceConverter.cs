﻿using System;
using System.Threading.Tasks;
using DrawIo.Azure.Core.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DrawIo.Azure.Core
{
    internal class AzureResourceConverter : JsonConverter
    {
        Func<AzureResource, string, Task<JObject>> _enricher;

        public AzureResourceConverter(Func<AzureResource, string, Task<JObject>> enricher)
        {
            _enricher = enricher;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(AzureResource).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            static AzureResource GetAzureResource(string type) => type.ToLowerInvariant() switch
            {
                "microsoft.network/virtualnetworks" => new VNet(),
                "microsoft.network/privateendpoints" => new PrivateEndpoint(),
                "microsoft.network/privatednszones" => new PrivateDnsZone(),
                "microsoft.network/privatednszones/virtualnetworklinks" => new PrivateDnsZoneVirtualNetworkLink(),
                "microsoft.network/networkinterfaces" => new Nic(),
                "microsoft.containerservice/managedclusters" => new AKS(),
                "microsoft.containerregistry/registries" => new ACR(),
                "microsoft.web/serverfarms" => new ASP(),
                "microsoft.web/sites" => new App(),
                "microsoft.apimanagement/service" => new APIm(),
                "microsoft.compute/virtualmachines" => new VM(),
                "microsoft.compute/disks" => new Disk(),
                "microsoft.operationalinsights/workspaces" => new LogAnalyticsWorkspace(),
                "microsoft.insights/components" => new AppInsights(),
                "microsoft.storage/storageaccounts" => new StorageAccount(),
                "microsoft.network/networksecuritygroups" => new NSG(),
                "microsoft.network/publicipaddresses" => new PIP(),
                "microsoft.compute/virtualmachines/extensions" => new VMExtension(),
                "microsoft.managedidentity/userassignedidentities" => new UserAssignedManagedIdentity(),
                "microsoft.keyvault/vaults" => new KeyVault(),
                _ => new IgnoreMeResource()
            };

            JObject jo = JObject.Load(reader);

            var type = (string) jo["type"];
            var item = GetAzureResource(type);
            if (item != null)
            {
                serializer.Populate(jo.CreateReader(), item);

                if (item.FetchFull)
                {
                    var fullItem = _enricher(item, item.ApiVersion).GetAwaiter().GetResult();
                    item.Enrich(fullItem);
                }
            }
            else
            {
                Console.WriteLine($"WARNING: No resource configured for {type}");
            }

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}