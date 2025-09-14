using System;

namespace AquaMira.Core;

public interface IUnitizedSensingNode : ISensingNode
{
    double? ReadAsCanonicalUnit();
    Enum? CanonicalUnit { get; set; }
}
