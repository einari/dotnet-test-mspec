using System;
using System.IO;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Execution.Console;
using Machine.Specifications.Runner.DotNet.Helpers;
using Machine.Specifications.Runner.DotNet.Controller;
using Machine.Specifications.Runner.DotNet.Execution;
using System.Collections.Generic;
using Machine.Specifications.Runner.DotNet.Controller.Model;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;

namespace Machine.Specifications.Runner.DotNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PrintVersionInfo(typeof(Program).GetTypeInfo().Assembly);

            CommandLine commandLine = CommandLine.Parse(args);

            if (commandLine.DesignTime) {
                throw new NotSupportedException("DesignTime mode is not supported yet.");
            }

            string assemblyPath = commandLine.AssemblyFile;

            Assembly testAssembly = AssemblyHelper.LoadTestAssemblyOrDependency(assemblyPath);
            Assembly mspecAssembly = AssemblyHelper.LoadTestAssemblyOrDependency(
                Path.Combine(Path.GetDirectoryName(assemblyPath), "Machine.Specifications.dll")
            );

            PrintVersionInfo(mspecAssembly);

            ConsoleOutputRunListener runListener = new ConsoleOutputRunListener();
            ISpecificationRunListener allListeneres = new AggregateRunListener(new ISpecificationRunListener[] {
                runListener,
                new AssemblyLocationAwareRunListener(new[] {testAssembly})
            });

            

            TestController testController = new TestController(mspecAssembly, allListeneres);
            if (commandLine.List) {
                Console.WriteLine(testController.DiscoverTestsRaw(testAssembly));
            } else {
                var assembliesToRun = new[] { testAssembly };
                var contexts = GetAllContextsFor(testController, assembliesToRun);

                if( commandLine.HasSpecificTestsToRun )
                    SpecificAssertionsHelper
                        .GetSpecificationsToRunPerAssemblyFromTestStrings(contexts, assembliesToRun, commandLine.Tests)
                            .Each(t=>testController.RunMembers(t.Assembly, t.Members));
                else testController.RunAssemblies(assembliesToRun);

                if (runListener.FailureOccurred)
                    Environment.Exit(-1);
            }
        }

        static IEnumerable<ContextInfo> GetAllContextsFor(TestController testController, IEnumerable<Assembly> assemblies)
        {
            var contexts = new List<ContextInfo>();

            foreach( var assembly in assemblies )
            {
                var contextsAsString = testController.DiscoverTestsRaw(assembly);
                var contextsXml = XDocument.Parse(contextsAsString);
                var contextsElements = contextsXml.XPathSelectElements("/contexts/contextinfo");
                contexts.AddRange(contextsElements.Select(e=>ContextInfo.GetFrom(e)));
            }

            return contexts;
        }

        private static void PrintVersionInfo(Assembly assembly)
        {
            Console.WriteLine(assembly.GetName().Name + ": " + assembly.GetVersion());
        }
    }
}