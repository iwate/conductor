using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conductor.Core.Models;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;

namespace Conductor.Core.Services
{
    public class ACIService : IACIService
    {
        private readonly Region _region;
        private readonly string _resourceGroup;
        private readonly string _registry;
        private readonly IAzure _azure;
        public ACIService(IConfiguration config)
        {
            var principal = config.GetSection("AzureServicePrincipal");
            var clientId = principal["AppId"];
            var clientSecret = principal["Password"];
            var tenantId = principal["TenantId"];
            var aci =  config.GetSection("ACI");
            var region = aci["Region"];
            _resourceGroup = aci["ResourceGroup"];
            _registry = aci["Registry"];

            if (!principal.Exists() || !aci.Exists())
                throw new ArgumentException(nameof(config));

            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException(nameof(config), "'AppId' is null.");

            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentException(nameof(config), "'Password' is null.");

            if (string.IsNullOrEmpty(tenantId))
                throw new ArgumentException(nameof(config), "'TenantId' is null.");
            
            if (string.IsNullOrEmpty(region))
                throw new ArgumentException(nameof(config), "'Region' is null.");

            if (string.IsNullOrEmpty(_resourceGroup))
                throw new ArgumentException(nameof(config), "'ResourceGroup' is null.");
            
            if (string.IsNullOrEmpty(_registry))
                throw new ArgumentException(nameof(config), "'Registry' is null.");
                
            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
            _azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            _region = Region.Create(region);
        }
        public async Task CreateAsync(string containerName, string imageName, bool @private, string os, double cpu, double memory, IDictionary<string, string> env)
        {
            var registry = await _azure.ContainerRegistries.GetByResourceGroupAsync(_resourceGroup, _registry);
            var credentials = await registry.GetCredentialsAsync();
            var group = _azure.ContainerGroups
                            .Define(containerName)
                            .WithRegion(_region)
                            .WithExistingResourceGroup(_resourceGroup);
            var g1 = os.ToLower() == "windows" ? group.WithWindows() : group.WithLinux();
            var g2 = @private? g1.WithPrivateImageRegistry(registry.LoginServerUrl, credentials.Username, credentials.AccessKeys.First().Value) : g1.WithPublicImageRegistryOnly();
            var g3 = g2.WithoutVolume();

            await g3.DefineContainerInstance(containerName)
                        .WithImage(imageName)
                        .WithoutPorts()
                        .WithCpuCoreCount(cpu)
                        .WithMemorySizeInGB(memory)
                        .WithEnvironmentVariables(env)
                        .Attach()
                    .WithRestartPolicy(ContainerGroupRestartPolicy.OnFailure)
                    .CreateAsync();
        }

        public async Task DeleteAsync(string containerName)
        {
            await _azure.ContainerGroups.DeleteByResourceGroupAsync(_resourceGroup, containerName);
        }

        public async Task<bool> ExitsRunning(string imageName)
        {
            var groups =  await _azure.ContainerGroups.ListByResourceGroupAsync(_resourceGroup);
            return groups.SelectMany(g => g.Containers).Any(c => c.Value.Image == imageName);
        }

        public Task<IEnumerable<string>> GetImagesAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> GetLogAsync(string containerName)
        {
            var group = await _azure.ContainerGroups.GetByResourceGroupAsync(_resourceGroup, containerName);
            return await group.GetLogContentAsync(containerName);
        }

        public async Task<ACIStatus> GetStatusAsync(string containerName)
        {
            var group = await _azure.ContainerGroups.GetByResourceGroupAsync(_resourceGroup, containerName);

            if (group == null)
                return ACIStatus.None;

            if (group.ProvisioningState == "Failed")
                return ACIStatus.ProvisioningFailed;

            ACIStatus  status = Enum.TryParse(group.State, out status) ? status : ACIStatus.Other;
            
            return status;
        }
    }
}