using System;
using System.IO;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Execution.Console;
using Machine.Specifications.Runner.DotNet.Helpers;
using Machine.Specifications.Runner.DotNet.Controller;
using Machine.Specifications.Runner.DotNet.Execution;

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

            

            var testController = new TestController(mspecAssembly, allListeneres);
            if (commandLine.List) {
                Console.WriteLine(testController.DiscoverTestsRaw(testAssembly));
            } else {
                var runManager = new RunManager(testController);
                var assembliesToRun = new[] { testAssembly };
                runManager.Run(commandLine, assembliesToRun);

                if (runListener.FailureOccurred)
                    Environment.Exit(-1);
            }
        }

        private static void PrintVersionInfo(Assembly assembly)
        {
            Console.WriteLine(assembly.GetName().Name + ": " + assembly.GetVersion());
        }
    }
}