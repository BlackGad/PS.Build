namespace UsageLibrary
{
#if DEBUG
    [DefinitionLibrary.Delegate.Empty]
    [DefinitionLibrary.Delegate.All]
    [DefinitionLibrary.Delegate.PreBuild]
    [DefinitionLibrary.Delegate.PostBuild]
#endif
    delegate void TestDelegate();
}