using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Machine.Specifications.Runner.DotNet.Helpers
{
    public class SpecificTestsHelper
    {
        public static IEnumerable<TestsPerAssembly> GetTestsToRunPerAssemblyFromTestStrings(IEnumerable<Assembly> assemblies, string[] testStrings)
        {
            var types = new List<Type>();
            assemblies.Each(a => types.AddRange(a.GetTypes()));

            var testPerAssemblies = new Dictionary<Assembly, List<MemberInfo>>(); 

            foreach( var testString in testStrings )
            {
                var testClassAndMethod = testString.Split('#');
                if( testClassAndMethod.Length > 2 ) 
                {
                    Console.WriteLine($"Format is wrong for '{testString}' - it should be a fully qualified namespace + name of class and optional #<assertion>");
                    continue;
                }
                
                if( testClassAndMethod.Length > 0 ) 
                {
                    var testClass = types.SingleOrDefault(t=>$"{t.Namespace}.{t.Name}" == testClassAndMethod[0]);
                    
                    if( testClass == null ) {
                        Console.WriteLine($"Could not resolve test class {testClassAndMethod[0]}");
                        break;
                    }

                    var testClassAssembly = testClass.GetTypeInfo().Assembly;
                    List<MemberInfo> members;
                    if( testPerAssemblies.ContainsKey(testClassAssembly)) members = testPerAssemblies[testClassAssembly];
                    else 
                    {
                        members = new List<MemberInfo>();
                        testPerAssemblies[testClassAssembly] = members;
                    }

                    var assertions = testClass.GetTypeInfo().GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Where(f=>f.FieldType.Name == "It");
                    if( testClassAndMethod.Length == 1 ) 
                    {
                        members.AddRange(assertions);
                    } else 
                    {
                        var assertionToAdd = assertions.SingleOrDefault(a=>a.Name == testClassAndMethod[1]);
                        if( assertionToAdd == null ) 
                        {
                            Console.WriteLine($"Couldn't locate assertion called {testClassAndMethod[1]} in {testClassAndMethod[0]}");
                            continue;
                        }
                        members.Add(assertionToAdd);
                    }
                } 
            }

            return testPerAssemblies.Select(t => new TestsPerAssembly(t.Key, t.Value));
        }
    }
}