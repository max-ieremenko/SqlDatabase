﻿using NUnit.Framework;
using Shouldly;
using SqlDatabase.TestApi;

namespace SqlDatabase.Adapter.PowerShellScripts;

[TestFixture]
public class PowerShellLinuxTest
{
    [Test]
    public void ParseParentProcessId()
    {
        const string Content = "43 (dotnet) R 123 43 1 34816 43 4210688 1660 0 1 0 4 1 0 0 20 0 7 0 11192599 2821132288 5836 18446744073709551615 4194304 4261060 140729390742432 0 0 0 0 4096 17630 0 0 0 17 2 0 0 0 0 0 6360360 6362127 11538432 140729390743290 140729390743313 140729390743313 140729390743528 0";
        using (var file = new TempFile(".txt"))
        {
            File.WriteAllText(file.Location, Content);

            PowerShellLinux.ParseParentProcessId(file.Location).ShouldBe(123);
        }
    }
}