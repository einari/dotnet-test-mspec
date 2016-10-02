using System;
using System.Collections.Generic;
using System.Reflection;
using Machine.Specifications.Runner.DotNet.Controller;

namespace Machine.Specifications.Runner.DotNet
{
    public class TypeRunner : ICanRunSpecifications
    {
        ITestController _testController; 
        Assembly _assembly; 
        IEnumerable<Type> _types;

        public TypeRunner(ITestController testController, Assembly assembly, IEnumerable<Type> types)
        {            
            _testController = testController;
            _assembly = assembly;
            _types = types;
        }

        public void Run()
        {
            _testController.RunTypes(_assembly, _types);
        }
    }
}