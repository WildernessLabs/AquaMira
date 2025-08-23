using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaMira.Core;

public interface ISensingNodeController
{
    Task<IEnumerable<ISensingNode>> ConfigureInputs(IEnumerable<ExtendedChannelConfig> channels);
}
