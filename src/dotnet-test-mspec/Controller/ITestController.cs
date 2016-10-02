using System;
using System.Collections.Generic;
using System.Reflection;

namespace Machine.Specifications.Runner.DotNet.Controller
{
    public interface ITestController
    {
        void RunAssemblies(IEnumerable<Assembly> assemblies);
        void RunMembers(Assembly assembly, IEnumerable<MemberInfo> members);
        void RunTypes(Assembly assembly, IEnumerable<Type> types);
    }
}