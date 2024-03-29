﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SqlDatabase.Adapter.AssemblyScripts;

internal abstract class ExecuteMethodResolverBase
{
    public abstract bool IsMatch(MethodInfo method);

    public abstract Action<IDbCommand, IReadOnlyDictionary<string, string?>> CreateDelegate(object instance, MethodInfo method);
}