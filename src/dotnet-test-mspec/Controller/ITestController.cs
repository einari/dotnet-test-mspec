using System;
using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller.Model;

namespace Machine.Specifications.Runner.DotNet.Controller
{
    public interface ITestController
    {
        void RunAssemblies(IEnumerable<Assembly> assemblies);
        void RunMembers(Assembly assembly, IEnumerable<MemberInfo> members);
        void RunTypes(Assembly assembly, IEnumerable<Type> types);

        string DiscoverTestsRaw(Assembly assembly);

        IEnumerable<ContextInfo> GetAllContextsFor(IEnumerable<Assembly> assemblies);
    }
}