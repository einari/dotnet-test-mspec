using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;

namespace Machine.Specifications.Runner.DotNet.Execution
{
    public class AssemblyRunner : ICanRunSpecifications
    {
        ITestController _testController;
        Assembly _assembly;

        public AssemblyRunner(ITestController testController, Assembly assembly)
        {
            _testController = testController;
            _assembly = assembly;
        }

        public void Run()
        {
            _testController.RunAssemblies(new[] {_assembly});
        }
    }
}