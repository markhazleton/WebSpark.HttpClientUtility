using Microsoft.VisualStudio.TestTools.UnitTesting;

// Configure test parallelization
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
