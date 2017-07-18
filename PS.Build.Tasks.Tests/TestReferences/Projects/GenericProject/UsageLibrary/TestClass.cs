using System;

namespace UsageLibrary
{
    [DefinitionLibrary.Class.Empty]
    [DefinitionLibrary.Class.All]
    [DefinitionLibrary.Class.PreBuild]
    [DefinitionLibrary.Class.PostBuild]
    class TestClass
    {
        [DefinitionLibrary.Field.Empty]
        [DefinitionLibrary.Field.All]
        [DefinitionLibrary.Field.PreBuild]
        [DefinitionLibrary.Field.PostBuild]
        int _testField;

        #region Constructors

        [DefinitionLibrary.Constructor.Empty]
        [DefinitionLibrary.Constructor.All]
        [DefinitionLibrary.Constructor.PreBuild]
        [DefinitionLibrary.Constructor.PostBuild]
        public TestClass()
        {
        }

        #endregion

        #region Properties

        [DefinitionLibrary.Property.Empty]
        [DefinitionLibrary.Property.All]
        [DefinitionLibrary.Property.PreBuild]
        [DefinitionLibrary.Property.PostBuild]
        public int TestProperty { get; set; }

        #endregion

        #region Events

        [DefinitionLibrary.Event.Empty]
        [DefinitionLibrary.Event.All]
        [DefinitionLibrary.Event.PreBuild]
        [DefinitionLibrary.Event.PostBuild]
        public event EventHandler TestEventField;

        #endregion

        #region Members

        [DefinitionLibrary.Method.Empty]
        [DefinitionLibrary.Method.All]
        [DefinitionLibrary.Method.PreBuild]
        [DefinitionLibrary.Method.PostBuild]
        [return: DefinitionLibrary.ReturnValue.Empty]
        [return: DefinitionLibrary.ReturnValue.All]
        [return: DefinitionLibrary.ReturnValue.PreBuild]
        [return: DefinitionLibrary.ReturnValue.PostBuild]
        public int TestMethod<
            [DefinitionLibrary.GenericParameter.Empty]
            [DefinitionLibrary.GenericParameter.All]
            [DefinitionLibrary.GenericParameter.PreBuild]
            [DefinitionLibrary.GenericParameter.PostBuild] T>(
            [DefinitionLibrary.Parameter.Empty] [DefinitionLibrary.Parameter.All] [DefinitionLibrary.Parameter.PreBuild] [DefinitionLibrary.Parameter.PostBuild] int argument)
        {
            return 0;
        }

        #endregion
    }
}