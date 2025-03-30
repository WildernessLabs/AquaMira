using System;
using System.Threading.Tasks;

namespace Thurston_Monitor.Core
{
    public interface IOutputController
    {
        Task SetState(bool state);
    }
}