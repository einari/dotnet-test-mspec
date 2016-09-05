using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Machine.Specifications.Runner.DotNet.Helpers
{
    public class SpecificAssertionsHelper
    {
        public static IEnumerable<AssertionsForAssembly> GetAssertionsToRunPerAssemblyFromTestStrings(IEnumerable<Assembly> assemblies, string[] testStrings)
        {
            var types = new List<Type>();
            var testPerAssemblies = new Dictionary<Assembly, List<MemberInfo>>();

            foreach (var testString in testStrings)
            {
                try
                {
                    var testClassAndAssertion = testString.Split('#');
                    ThrowIfFormatIsWrong(testString, testClassAndAssertion);
                    if (testClassAndAssertion.Length > 0)
                    {
                        var testClassString = testClassAndAssertion[0];
                        var assertionNameString = testClassAndAssertion[1];

                        var testClass = GetTestClassFromString(assemblies, testClassString);
                        ThrowIfTestClassIsNull(testClass, testClassString);

                        var members = GetOrCreateMembersForTestClass(testPerAssemblies, testClass);
                        var assertions = GetAllAssertionsFor(testClass);
                        if (testClassAndAssertion.Length == 1) members.AddRange(assertions);
                        else members.Add(GetSpecificAssertion(assertions, testClassString, assertionNameString));
                    }
                }
                catch (ArgumentException ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return testPerAssemblies.Select(t => new AssertionsForAssembly(t.Key, t.Value));
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

        static IEnumerable<FieldInfo> GetAllAssertionsFor(Type testClass)
        {
            return testClass.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(f => f.FieldType.Name == "It");
        }


        static MemberInfo GetSpecificAssertion(IEnumerable<MemberInfo> assertions, string testClassString, string assertionNameString)
        {
            var assertionToAdd = assertions.SingleOrDefault(a => a.Name == assertionNameString);
            if (assertionToAdd == null)
                throw new ArgumentException($"Couldn't locate assertion called {testClassString} in {assertionNameString}");

            return assertionToAdd;
        }

        static List<MemberInfo> GetOrCreateMembersForTestClass(Dictionary<Assembly, List<MemberInfo>> testPerAssemblies, Type testClass)
        {
            var testClassAssembly = testClass.GetTypeInfo().Assembly;
            List<MemberInfo> members;
            if (testPerAssemblies.ContainsKey(testClassAssembly)) members = testPerAssemblies[testClassAssembly];
            else
            {
                members = new List<MemberInfo>();
                testPerAssemblies[testClassAssembly] = members;
            }

            return members;
        }

        static Type GetTestClassFromString(IEnumerable<Assembly> assemblies, string testClassString)
        {
            Type testClass = null;
            foreach (var assembly in assemblies)
            {
                testClass = assembly.GetType(testClassString);
                if (testClass != null) break;
            }

            return testClass;
        }
    }
}