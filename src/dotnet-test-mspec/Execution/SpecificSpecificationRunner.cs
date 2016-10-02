using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;

namespace Machine.Specifications.Runner.DotNet
{
    public class SpecificSpecificationRunner : ICanRunSpecifications
    {
        ITestController _testController;
        Assembly _assembly; 
        IEnumerable<MemberInfo> _members;


        public SpecificSpecificationRunner(ITestController testController, Assembly assembly, IEnumerable<MemberInfo> members)
        {
            _testController = testController;
            _assembly = assembly;
            _members = members;
        }

        public void Run()
        {
            _testController.RunMembers(_assembly,_members);
        }
    }
}