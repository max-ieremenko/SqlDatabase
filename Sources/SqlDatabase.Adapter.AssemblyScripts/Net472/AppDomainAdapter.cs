using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SqlDatabase.Adapter.AssemblyScripts.Net472;

internal static class AppDomainAdapter
{
    private static Func<string, string, AppDomain>? _createDomain;
    private static Func<AppDomain, DomainAgent>? _createInstanceFromAndUnwrap;

    public static void Initialize()
    {
        try
        {
            GetOrBuildCreateDomain();
            GetOrBuildCreateInstanceFromAndUnwrap();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"AppDomain host initialization failed, framework {RuntimeInformation.FrameworkDescription}", ex);
        }
    }

    public static AppDomain CreateDomain(string domainFriendlyName, string applicationBase)
    {
        var createDomain = GetOrBuildCreateDomain();
        return createDomain(domainFriendlyName, applicationBase);
    }

    public static DomainAgent CreateInstanceFromAndUnwrap(AppDomain domain)
    {
        var createInstanceFromAndUnwrap = GetOrBuildCreateInstanceFromAndUnwrap();
        return createInstanceFromAndUnwrap(domain);
    }

    private static Func<string, string, AppDomain> GetOrBuildCreateDomain()
    {
        if (_createDomain == null)
        {
            _createDomain = BuildCreateDomain();
        }

        return _createDomain;
    }

    private static Func<AppDomain, DomainAgent> GetOrBuildCreateInstanceFromAndUnwrap()
    {
        if (_createInstanceFromAndUnwrap == null)
        {
            _createInstanceFromAndUnwrap = BuildCreateInstanceFromAndUnwrap();
        }

        return _createInstanceFromAndUnwrap;
    }

    private static Func<AppDomain, DomainAgent> BuildCreateInstanceFromAndUnwrap()
    {
        // CreateInstanceFromAndUnwrap(GetType().Assembly.Location, typeof(DomainAgent).FullName)
        var domain = Expression.Parameter(typeof(AppDomain), "domain");

        var proxy = Expression.Call(
            domain,
            ResolveAppDomainCreateInstanceFromAndUnwrap(),
            Expression.Constant(typeof(AppDomainAdapter).Assembly.Location),
            Expression.Constant(typeof(DomainAgent).FullName));

        var body = Expression.Convert(proxy, typeof(DomainAgent));

        var result = Expression.Lambda<Func<AppDomain, DomainAgent>>(body, domain);
        return result.Compile();
    }

    private static Func<string, string, AppDomain> BuildCreateDomain()
    {
        var applicationBase = Expression.Parameter(typeof(string), "applicationBase");
        var domainFriendlyName = Expression.Parameter(typeof(string), "domainFriendlyName");

        ////var setup = new AppDomainSetup
        ////{
        ////    ApplicationBase = _appBase.Location,
        ////    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
        ////    LoaderOptimization = LoaderOptimization.MultiDomainHost
        ////};
        ////AppDomain.CreateDomain(appBaseName, null, setup)
        var createDomain = ResolveAppDomainCreateDomain();
        var createDomainParameters = createDomain.GetParameters();

        var configurationFile = Expression.Property(Expression.Constant(AppDomain.CurrentDomain), "SetupInformation");

        var setup = Expression.Variable(createDomainParameters[2].ParameterType, "setup");
        var body = Expression.Block(
            typeof(AppDomain),
            new ParameterExpression[] { setup },
            Expression.Assign(setup, Expression.New(createDomainParameters[2].ParameterType)),
            Expression.Assign(
                Expression.Property(setup, "ApplicationBase"),
                applicationBase),
            Expression.Assign(
                Expression.Property(setup, "ConfigurationFile"),
                Expression.Property(configurationFile, "ConfigurationFile")),
            Expression.Assign(
                Expression.Property(setup, "LoaderOptimization"),
                Expression.Constant(LoaderOptimization.MultiDomainHost)),
            Expression.Call(
                createDomain,
                domainFriendlyName,
                Expression.Constant(null, createDomainParameters[1].ParameterType),
                setup));

        var result = Expression.Lambda<Func<string, string, AppDomain>>(body, domainFriendlyName, applicationBase);
        return result.Compile();
    }

    private static MethodInfo ResolveAppDomainCreateInstanceFromAndUnwrap()
    {
        // CreateInstanceFromAndUnwrap(string assemblyName, string typeName)
        var result = typeof(AppDomain)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(i => "CreateInstanceFromAndUnwrap".Equals(i.Name, StringComparison.Ordinal))
            .Where(i =>
            {
                var parameters = i.GetParameters();
                return parameters.Length == 2
                       && parameters[0].ParameterType == typeof(string)
                       && parameters[1].ParameterType == typeof(string);
            })
            .FirstOrDefault();

        if (result == null)
        {
            throw new NotSupportedException("The method AppDomain.CreateInstanceFromAndUnwrap not found.");
        }

        return result;
    }

    private static MethodInfo ResolveAppDomainCreateDomain()
    {
        // CreateDomain(string friendlyName, System.Security.Policy.Evidence securityInfo, AppDomainSetup info)
        var result = typeof(AppDomain)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(i => nameof(AppDomain.CreateDomain).Equals(i.Name, StringComparison.Ordinal))
            .Where(i =>
            {
                var parameters = i.GetParameters();
                return parameters.Length == 3
                       && parameters[0].ParameterType == typeof(string)
                       && parameters[1].ParameterType.FullName!.Equals("System.Security.Policy.Evidence", StringComparison.Ordinal)
                       && parameters[2].ParameterType.FullName!.Equals("System.AppDomainSetup");
            })
            .FirstOrDefault();

        if (result == null)
        {
            throw new NotSupportedException("The method AppDomain.CreateDomain not found.");
        }

        return result;
    }
}