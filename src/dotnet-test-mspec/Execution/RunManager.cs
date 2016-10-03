using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;
using Machine.Specifications.Runner.DotNet.Helpers;

namespace Machine.Specifications.Runner.DotNet.Execution
{
    public class RunManager
    {
        ITestController _testController;
        IRunBuilder _runBuilder;

        public RunManager(ITestController testController, IRunBuilder runBuilder)
        {
            _testController = testController;
            _runBuilder = runBuilder;
        }

        public void Run(CommandLine commandLine, IEnumerable<Assembly> assemblies)
        {
            var contexts = _testController.GetAllContextsFor(assemblies);
            var runners = _runBuilder.BuildFrom(commandLine, assemblies, contexts);
            runners.Each(r=> r.Run());
        }
    }
}