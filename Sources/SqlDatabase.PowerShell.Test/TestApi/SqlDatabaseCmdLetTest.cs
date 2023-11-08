using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Shouldly;
using SqlDatabase.Configuration;
using SqlDatabase.PowerShell.Internal;
using Command = System.Management.Automation.Runspaces.Command;

namespace SqlDatabase.PowerShell.TestApi;

public abstract class SqlDatabaseCmdLetTest<TSubject>
{
    private readonly IList<GenericCommandLine> _commandLines = new List<GenericCommandLine>();
    private Runspace _runSpace = null!;
    private System.Management.Automation.PowerShell _powerShell = null!;

    [SetUp]
    public void BeforeEachTest()
    {
        var sessionState = InitialSessionState.CreateDefault();

        foreach (var alias in ResolveAliases())
        {
            sessionState.Commands.Add(new SessionStateCmdletEntry(alias, typeof(TSubject), null));
        }

        _runSpace = RunspaceFactory.CreateRunspace(sessionState);
        _runSpace.Open();

        _powerShell = System.Management.Automation.PowerShell.Create();
        _powerShell.Runspace = _runSpace;

        var program = new Mock<ISqlDatabaseProgram>(MockBehavior.Strict);
        program
            .Setup(p => p.ExecuteCommand(It.IsNotNull<GenericCommandLine>()))
            .Callback<GenericCommandLine>(cmd => _commandLines.Add(cmd));

        _commandLines.Clear();
        PowerShellCommandBase.Program = program.Object;
    }

    [TearDown]
    public void AfterEachTest()
    {
        PowerShellCommandBase.Program = null;

        foreach (var row in _powerShell.Streams.Information)
        {
            Console.WriteLine(row);
        }

        _powerShell?.Dispose();
        _runSpace?.Dispose();
    }

    protected Collection<PSObject> InvokeCommand(string name)
    {
        var command = new Command(name);
        _powerShell.Commands.AddCommand(command);

        return _powerShell.Invoke();
    }

    protected GenericCommandLine[] InvokeSqlDatabase(string name, Action<Command> builder)
    {
        return InvokeInvokeSqlDatabasePipeLine(name, builder);
    }

    protected GenericCommandLine[] InvokeInvokeSqlDatabasePipeLine(string name, Action<Command> builder, params object[] args)
    {
        _commandLines.Clear();

        var command = new Command(name);
        _powerShell.Commands.AddCommand(command);

        builder(command);
        _powerShell.Invoke(args);

        return _commandLines.ToArray();
    }

    private static IEnumerable<string> ResolveAliases()
    {
        var cmdlet = (CmdletAttribute)typeof(TSubject).GetCustomAttribute(typeof(CmdletAttribute));
        cmdlet.ShouldNotBeNull();

        yield return $"{cmdlet.VerbName}-{cmdlet.NounName}";

        var alias = (AliasAttribute)typeof(TSubject).GetCustomAttribute(typeof(AliasAttribute));
        if (alias != null)
        {
            foreach (var i in alias.AliasNames)
            {
                yield return i;
            }
        }
    }
}