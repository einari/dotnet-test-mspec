using System.Collections.Generic;
using System.Reflection;

namespace Machine.Specifications.Runner.DotNet.Helpers
{
    public class TestsPerAssembly 
    {
        public TestsPerAssembly(Assembly assembly, IEnumerable<MemberInfo> members)
        {
            Assembly = assembly;
            Members = members;
        }
        
        public Assembly Assembly { get; private set; }
        public IEnumerable<MemberInfo> Members { get; private set; }
    }
}