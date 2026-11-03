// Settings of MSTest for the whole assembly: the tests share no state, so they run in parallel
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
