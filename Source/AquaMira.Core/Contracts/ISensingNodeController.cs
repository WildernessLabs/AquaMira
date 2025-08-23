using AquaMira.Core.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaMira.Core;

public interface ISensingNodeController
{
    Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware);
}
