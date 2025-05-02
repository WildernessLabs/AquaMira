using Meadow;
using Meadow.Cloud;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thurston_Monitor.Core;

public class CloudController
{
    public enum EventIds
    {
        DeviceStarted = 101,
        DeviceData = 201,
    }

    private readonly IMeadowCloudService cloudService;
    private readonly ICommandService commandService;
    private readonly StorageController storageController;
    private readonly INetworkController networkController;

    public CloudController(
        IMeadowCloudService cloudService,
        ICommandService commandService,
        StorageController storageController,
        INetworkController networkController)
    {
        this.cloudService = cloudService;
        this.commandService = commandService;
        this.storageController = storageController;
        this.networkController = networkController;

        storageController.Records.ItemAdded += Records_ItemAdded;
    }

    private void Records_ItemAdded(object sender, EventArgs e)
    {
        // TODO: use a periodic timer instead to send at a slower rate?

        var batch = storageController.Records.Peek();

        while (batch != null)
        {
            var evt = new CloudEvent
            {
                EventId = (int)EventIds.DeviceData,
                Description = "Device Data",
                Measurements = batch.Values,
                Timestamp = batch.BatchTime
            };
            try
            {
                cloudService.SendEvent(evt);
                storageController.Records.Remove(1);

                batch = storageController.Records.Peek();
            }
            catch (Exception ex)
            {
                // error sending or some such
                Resolver.Log.Info($"Failed to send to cloud: {ex.Message}");
                break;
            }
        }

    }

    public Task LogError()
    {
        var log = new CloudLog
        {

        };
        return cloudService.SendLog(log);
    }

    public Task ReportDeviceStartup()
    {
        var deviceInfo = new Dictionary<string, object>
        {
            { "DeviceName", Resolver.Device.Information.DeviceName },
            { "Device ID", Resolver.Device.Information.UniqueID }
        };

        var evt = new CloudEvent
        {
            EventId = (int)EventIds.DeviceStarted,
            Description = "Device Startup",
            Measurements = deviceInfo,
            Timestamp = DateTime.UtcNow
        };

        return cloudService.SendEvent(evt);
    }
}