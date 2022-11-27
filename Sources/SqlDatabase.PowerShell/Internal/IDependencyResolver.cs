using System;

namespace SqlDatabase.PowerShell.Internal;

internal interface IDependencyResolver : IDisposable
{
    void Initialize();
}