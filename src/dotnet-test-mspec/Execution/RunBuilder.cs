using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;
using Machine.Specifications.Runner.DotNet.Controller.Model;

namespace Machine.Specifications.Runner.DotNet.Execution
{
    public class RunBuilder : IRunBuilder
    {
        ITestController _testController;

        public RunBuilder(ITestController testController)
        {
            _testController = testController;
        }


        public IEnumerable<ICanRunSpecifications> BuildFrom(CommandLine commandLine, IEnumerable<Assembly> assemblies, IEnumerable<ContextInfo> contexts)
        {
            var runners = new List<ICanRunSpecifications>();

            if (commandLine.HasSpecificTestsToRun)
            {
                var testStrings = commandLine.Tests;
                var types = new List<Type>();
                var testPerAssemblies = new Dictionary<Assembly, List<MemberInfo>>();

                foreach (var testString in testStrings)
                {
                    try
                    {
                        var testClassAndSpecification = testString.Split('#');
                        ThrowIfFormatIsWrong(testString, testClassAndSpecification);
                        if (testClassAndSpecification.Length > 0)
                        {
                            var testClassString = testClassAndSpecification[0];
                            var specificationNameString = testClassAndSpecification[1];

                            var context = contexts.SingleOrDefault(c => c.TypeName == testClassString);
                            ThrowIfContextNoFound(context, testClassString);

                            var specification = context.Specifications.SingleOrDefault(s => s.FieldName == specificationNameString);
                            ThrowIfSpecificationNotFound(specification, specificationNameString);

                            var testClassType = GetClassFromContext(assemblies, context);
                            ThrowIfTestClassIsNull(testClassType, testClassString);

                            var members = GetOrCreateMembersForTestClass(testPerAssemblies, testClassType);
                            var specifications = GetAllSpecificationsFor(testClassType, context);

                            if (testClassAndSpecification.Length == 1) members.AddRange(specifications);
                            else members.Add(GetMemberForSpecificationFrom(specifications, specification));
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                }

                runners.AddRange(testPerAssemblies.Select(t=>new SpecificSpecificationRunner(_testController, t.Key, t.Value)));
            }
            else runners.Add(new AssemblyRunner(_testController, assemblies));

            return runners;
        }



        private static void ThrowIfSpecificationNotFound(SpecificationInfo specification, string specificationNameString)
        {
            if (specification == null)
                throw new ArgumentException($"Specifiction called '{specificationNameString}' not found");
        }

        private static void ThrowIfContextNoFound(ContextInfo context, string testClassString)
        {
            if (context == null)
                throw new ArgumentException($"Context for '{testClassString}' not found");

        }

        static void ThrowIfFormatIsWrong(string testString, string[] testClassAndAssertion)
        {
            if (testClassAndAssertion.Length > 2)
                throw new ArgumentException($"Format is wrong for '{testString}' - it should be a fully qualified namespace + name of class and optional #<assertion>");
        }

        static void ThrowIfTestClassIsNull(Type testClass, string testClassString)
        {
            if (testClass == null)
                throw new ArgumentException($"Could not resolve test class {testClassString}");
        }

        static IEnumerable<MemberInfo> GetAllSpecificationsFor(Type testClassType, ContextInfo context)
        {
            return context.Specifications.Select(s => testClassType.GetField(s.FieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance));
        }

        static MemberInfo GetMemberForSpecificationFrom(IEnumerable<MemberInfo> members, SpecificationInfo specification)
        {
            var member = members.SingleOrDefault(m => m.Name == specification.FieldName);
            ThrowIfMemberNotFoundForSpecification(member, specification);
            return member;
        }

        private static void ThrowIfMemberNotFoundForSpecification(MemberInfo member, SpecificationInfo specification)
        {
            if (member == null)
                throw new ArgumentException($"Couldn't locate member for {specification.Name}");
        }

        static List<MemberInfo> GetOrCreateMembersForTestClass(Dictionary<Assembly, List<MemberInfo>> testPerAssemblies, Type testClassType)
        {
            var testClassAssembly = testClassType.GetTypeInfo().Assembly;
            List<MemberInfo> members;
            if (testPerAssemblies.ContainsKey(testClassAssembly)) members = testPerAssemblies[testClassAssembly];
            else
            {
                members = new List<MemberInfo>();
                testPerAssemblies[testClassAssembly] = members;
            }

            return members;
        }

        static Type GetClassFromContext(IEnumerable<Assembly> assemblies, ContextInfo context)
        {
            Type testClass = null;
            foreach (var assembly in assemblies)
            {
                testClass = assembly.GetType(context.TypeName);
                if (testClass != null) break;
            }

            return testClass;
        }
    }
}