using System;
using Machine.Specifications.Core.Runner.DotNet.Execution.Console.Outputs;
using Machine.Specifications.Runner;

namespace Machine.Specifications.Core.Runner.DotNet.Execution.Console
{
    public class ConsoleOutputRunListener : ISpecificationRunListener, ISpecificationResultProvider
    {
        readonly IConsole _console;
        readonly RunStats _timer;
        readonly IOutput _output;
        int _contextCount;
        int _specificationCount;
        int _passedSpecificationCount;
        int _failedSpecificationCount;
        bool _failureOccurred;
        int _ignoredSpecificationCount;
        int _unimplementedSpecificationCount;
        ContextInfo _currentContext;
        readonly FailedSpecificationsSummary _summary;

        public ConsoleOutputRunListener()
        {
            _console = new DefaultConsole();
            _timer = new RunStats();
            _output = new VerboseOutput(_console);
            _summary = new FailedSpecificationsSummary(new VerboseOutput(_console), _console);
        }

        public bool FailureOccurred
        {
            get { return _failureOccurred || _failedSpecificationCount > 0; }
        }

        public void OnAssemblyStart(AssemblyInfo assembly)
        {
            _output.AssemblyStart(assembly);
        }

        public void OnAssemblyEnd(AssemblyInfo assembly)
        {
            _output.AssemblyEnd(assembly);
        }

        public void OnRunStart()
        {
            _output.RunStart();

            _contextCount = 0;
            _specificationCount = 0;
            _failedSpecificationCount = 0;
            _unimplementedSpecificationCount = 0;
            _ignoredSpecificationCount = 0;
            _passedSpecificationCount = 0;
        }

        public void OnRunEnd()
        {
            _output.RunEnd();

            _summary.WriteSummary();

            _timer.Stop();

            var line = String.Format("Contexts: {0}, Specifications: {1}, Time: {2:s\\.ff} seconds",
                                     _contextCount,
                                     _specificationCount,
                                     FormattableTimeSpan((long)_timer.Duration.TotalMilliseconds));

            if (_failedSpecificationCount > 0 || _unimplementedSpecificationCount > 0)
            {
                line += String.Format(Environment.NewLine + "  {0} passed, {1} failed", _passedSpecificationCount, _failedSpecificationCount);
                if (_unimplementedSpecificationCount > 0)
                {
                    line += String.Format(", {0} not implemented", _unimplementedSpecificationCount);
                }
                if (_ignoredSpecificationCount > 0)
                {
                    line += String.Format(", {0} ignored", _ignoredSpecificationCount);
                }
            }

            _console.WriteLine("");
            _console.WriteLine(line);
        }

        public void OnContextStart(ContextInfo context)
        {
            _currentContext = context;
            _output.ContextStart(context);
        }

        public void OnContextEnd(ContextInfo context)
        {
            _output.ContextEnd(context);
            _contextCount += 1;
        }

        public void OnSpecificationStart(SpecificationInfo specification)
        {
            _output.SpecificationStart(specification);
        }

        public void OnSpecificationEnd(SpecificationInfo specification, Result result)
        {
            _specificationCount += 1;
            switch (result.Status)
            {
                case Status.Passing:
                    _passedSpecificationCount += 1;
                    _output.Passing(specification);
                    break;
                case Status.NotImplemented:
                    _unimplementedSpecificationCount += 1;
                    _output.NotImplemented(specification);
                    break;
                case Status.Ignored:
                    _ignoredSpecificationCount += 1;
                    _output.Ignored(specification);
                    break;
                default:
                    _failedSpecificationCount += 1;
                    _summary.RecordFailure(_currentContext, specification, result);
                    _output.Failed(specification, result);
                    break;
            }
        }

        public void OnFatalError(ExceptionResult exception)
        {
            _failureOccurred = true;
            _console.WriteLine("");
            _console.WriteLine("Fatal Error");
            _console.WriteLine(exception.ToString());

            _summary.RecordFatalError(exception);
        }

        static DateTime FormattableTimeSpan(long milliseconds)
        {
            return DateTime.MinValue + TimeSpan.FromMilliseconds(milliseconds);
        }
    }
}