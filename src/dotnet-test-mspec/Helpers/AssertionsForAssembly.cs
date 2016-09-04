using System.Collections.Generic;
using System.Reflection;

namespace Machine.Specifications.Runner.DotNet.Helpers
{
    public class AssertionsForAssembly 
    {
        public AssertionsForAssembly(Assembly assembly, IEnumerable<MemberInfo> members)
        {
            Assembly = assembly;
            Members = members;
        }
        
        public Assembly Assembly { get; private set; }
        public IEnumerable<MemberInfo> Members { get; private set; }
    }
}