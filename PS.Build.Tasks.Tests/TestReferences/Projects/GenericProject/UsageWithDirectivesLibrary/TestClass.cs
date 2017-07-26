using System;

namespace UsageLibrary
{
#if DEBUG
    [DefinitionLibrary.Class.Empty]
    [DefinitionLibrary.Class.All]
    [DefinitionLibrary.Class.PreBuild]
    [DefinitionLibrary.Class.PostBuild]
#endif
        class TestClass
    {
#if DEBUG
        [DefinitionLibrary.Field.Empty]
        [DefinitionLibrary.Field.All]
        [DefinitionLibrary.Field.PreBuild]
        [DefinitionLibrary.Field.PostBuild]
#endif
            int _testField;

        #region Constructors

#if DEBUG
        [DefinitionLibrary.Constructor.Empty]
        [DefinitionLibrary.Constructor.All]
        [DefinitionLibrary.Constructor.PreBuild]
        [DefinitionLibrary.Constructor.PostBuild]
#endif
        public TestClass()
        {
        }

        #endregion

        #region Properties

#if DEBUG
        [DefinitionLibrary.Property.Empty]
        [DefinitionLibrary.Property.All]
        [DefinitionLibrary.Property.PreBuild]
        [DefinitionLibrary.Property.PostBuild]
#endif
            public int TestProperty { get; set; }

        #endregion

        #region Events

#if DEBUG
        [DefinitionLibrary.Event.Empty]
        [DefinitionLibrary.Event.All]
        [DefinitionLibrary.Event.PreBuild]
        [DefinitionLibrary.Event.PostBuild]
#endif
            public event EventHandler TestEventField;

        #endregion

        #region Members

#if DEBUG
        [DefinitionLibrary.Method.Empty]
        [DefinitionLibrary.Method.All]
        [DefinitionLibrary.Method.PreBuild]
        [DefinitionLibrary.Method.PostBuild]
        [return: DefinitionLibrary.ReturnValue.Empty]
        [return: DefinitionLibrary.ReturnValue.All]
        [return: DefinitionLibrary.ReturnValue.PreBuild]
        [return: DefinitionLibrary.ReturnValue.PostBuild]
#endif
        public int TestMethod<
#if DEBUG
            [DefinitionLibrary.GenericParameter.Empty]
            [DefinitionLibrary.GenericParameter.All]
            [DefinitionLibrary.GenericParameter.PreBuild]
            [DefinitionLibrary.GenericParameter.PostBuild]
#endif
                T>(
#if DEBUG
            [DefinitionLibrary.Parameter.Empty] [DefinitionLibrary.Parameter.All] [DefinitionLibrary.Parameter.PreBuild] [DefinitionLibrary.Parameter.PostBuild]
#endif
                int argument)
        {
            return 0;
        }

        #endregion
    }
}