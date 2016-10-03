using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;

namespace Machine.Specifications.Runner.DotNet.Execution
{
    public class AssemblyRunner : ICanRunSpecifications
    {
        ITestController _testController;
        IEnumerable<Assembly> _assemblies;

        public AssemblyRunner(ITestController testController, IEnumerable<Assembly> assemblies)
        {
            _testController = testController;
            _assemblies = assemblies;
        }

        public void Run()
        {
            _testController.RunAssemblies(_assemblies);
        }
    }
}