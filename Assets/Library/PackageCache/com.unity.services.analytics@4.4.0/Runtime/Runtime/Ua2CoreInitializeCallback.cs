using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Analytics.Data;
using Unity.Services.Analytics.Internal;
using Unity.Services.Authentication.Internal;
using Unity.Services.Core.Analytics.Internal;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Device.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Internal;
using UnityEngine;

class Ua2CoreInitializeCallback : IInitializablePackage
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Register()
    {
        CoreRegistry.Instance.RegisterPackage(new Ua2CoreInitializeCallback())
            .DependsOn<IInstallationId>()
            .DependsOn<ICloudProjectId>()
            .DependsOn<IEnvironments>()
            .DependsOn<IExternalUserId>()
            .DependsOn<IProjectConfiguration>()
            .OptionallyDependsOn<IPlayerId>()
            .ProvidesComponent<IAnalyticsStandardEventComponent>();
    }

    public async Task Initialize(CoreRegistry registry)
    {
        var cloudProjectId = registry.GetServiceComponent<ICloudProjectId>();
        var installationId = registry.GetServiceComponent<IInstallationId>();
        var playerId = registry.GetServiceComponent<IPlayerId>();
        var environments = registry.GetServiceComponent<IEnvironments>();
        var customUserId = registry.GetServiceComponent<IExternalUserId>();

        var coreStatsHelper = new CoreStatsHelper();
        var consentTracker = new ConsentTracker(coreStatsHelper);

        var buffer = new BufferX(new BufferSystemCalls(), new DiskCache(new FileSystemCalls()));

        AnalyticsService.internalInstance = new AnalyticsServiceInstance(
            new DataGenerator(),
            buffer,
            new BufferRevoked(),
            coreStatsHelper,
            consentTracker,
            new Dispatcher(new WebRequestHelper(), consentTracker),
            new AnalyticsForgetter(consentTracker),
            cloudProjectId,
            installationId,
            playerId,
            environments.Current,
            customUserId,
            new AnalyticsServiceSystemCalls());

        StandardEventServiceComponent standardEventComponent = new StandardEventServiceComponent(
            registry.GetServiceComponent<IProjectConfiguration>(),
            AnalyticsService.internalInstance);
        registry.RegisterServiceComponent<IAnalyticsStandardEventComponent>(standardEventComponent);

        buffer.LoadFromDisk();

        await AnalyticsService.internalInstance.Initialize();

#if UNITY_ANALYTICS_DEVELOPMENT
        Debug.LogFormat("Core Initialize Callback\nInstall ID: {0}\nPlayer ID: {1}\nCustom Analytics ID: {2}",
            installationId.GetOrCreateIdentifier(),
            playerId?.PlayerId,
            customUserId.UserId
        );
#endif

        if (consentTracker.IsGeoIpChecked())
        {
            AnalyticsService.internalInstance.Flush();
        }
    }
}
