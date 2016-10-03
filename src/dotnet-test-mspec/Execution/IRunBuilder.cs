using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller.Model;

namespace Machine.Specifications.Runner.DotNet.Execution
{
    public interface IRunBuilder
    {
        IEnumerable<ICanRunSpecifications> BuildFrom(CommandLine commandLine, IEnumerable<Assembly> assemblies, IEnumerable<ContextInfo> contexts);
    }
}