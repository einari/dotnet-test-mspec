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

            foreach( var testString in testStrings )
            {
                var testClassAndAssertion = testString.Split('#');
                if( testClassAndAssertion.Length > 2 ) 
                {
                    Console.WriteLine($"Format is wrong for '{testString}' - it should be a fully qualified namespace + name of class and optional #<assertion>");
                    continue;
                }
                
                if( testClassAndAssertion.Length > 0 )
                {
                    var testClassString = testClassAndAssertion[0];
                    var testClass = GetTestClassFromString(assemblies, testClassString);
                    if (testClass == null)
                    {
                        Console.WriteLine($"Could not resolve test class {testClassAndAssertion[0]}");
                        break;
                    }

                    var members = GetOrCreateMembersForTestClass(testPerAssemblies, testClass);

                    var assertions = testClass.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(f => f.FieldType.Name == "It");
                    if (testClassAndAssertion.Length == 1)
                    {
                        members.AddRange(assertions);
                    }
                    else
                    {
                        var assertionToAdd = assertions.SingleOrDefault(a => a.Name == testClassAndAssertion[1]);
                        if (assertionToAdd == null)
                        {
                            Console.WriteLine($"Couldn't locate assertion called {testClassAndAssertion[1]} in {testClassAndAssertion[0]}");
                            continue;
                        }
                        members.Add(assertionToAdd);
                    }
                }
            }

            return testPerAssemblies.Select(t => new AssertionsForAssembly(t.Key, t.Value));
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
            foreach( var assembly in assemblies ) 
            {
                testClass = assembly.GetType(testClassString);
                if( testClass != null ) break;
            }

            return testClass;
        }
    }
}