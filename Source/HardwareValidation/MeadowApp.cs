using Meadow.Devices;
using System.Threading.Tasks;

namespace HardwareValidation;

public class MeadowApp : ProjectLabCoreComputeApp
{
    private MainController mainController;

    public override Task Initialize()
    {
        mainController = new MainController();
        mainController.Initialize(Hardware);

        return base.Initialize();
    }

    public override Task Run()
    {
        return mainController.Run();
    }

}