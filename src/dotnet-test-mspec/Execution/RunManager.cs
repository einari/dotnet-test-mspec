using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;
using Machine.Specifications.Runner.DotNet.Helpers;

namespace Machine.Specifications.Runner.DotNet
{
    public class RunManager
    {
        ITestController _testController;

        public RunManager(ITestController testController)
        {
            _testController = testController;
        }

        public void Run(CommandLine commandLine, IEnumerable<Assembly> assemblies)
        {
            var contexts = _testController.GetAllContextsFor(assemblies);

            if( commandLine.HasSpecificTestsToRun )
                SpecificAssertionsHelper
                    .GetSpecificationsToRunPerAssemblyFromTestStrings(contexts, assemblies, commandLine.Tests)
                        .Each(t=>_testController.RunMembers(t.Assembly, t.Members));
            else _testController.RunAssemblies(assemblies);
        }
    }
}