using System;

namespace UsageOrderingLibrary
{
    [DefinitionLibrary.Class.All]
    public class Class<[DefinitionLibrary.GenericParameter.All] TClass>
    {
        [DefinitionLibrary.Field.All]
        int _field;

        #region Constructors

        [DefinitionLibrary.Constructor.All]
        public Class([DefinitionLibrary.Parameter.All] int constructorArgument)
        {
        }

        #endregion

        #region Properties

        [DefinitionLibrary.Property.All]
        public int this[[DefinitionLibrary.Parameter.All] int index]
        {
            [return: DefinitionLibrary.ReturnValue.All]
            get { throw new NotImplementedException(); }

            [DefinitionLibrary.Method.All]
            set { throw new NotImplementedException(); }
        }

        [DefinitionLibrary.Property.All]
        public int Property
        {
            [return: DefinitionLibrary.ReturnValue.All]
            get { throw new NotImplementedException(); }

            [DefinitionLibrary.Method.All]
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region Events

        [DefinitionLibrary.Event.All]
        event EventHandler EventField;

        [DefinitionLibrary.Event.All]
        event EventHandler EventProperty
        {
            [DefinitionLibrary.Method.All]
            add { throw new NotImplementedException(); }
            [DefinitionLibrary.Method.All]
            remove { throw new NotImplementedException(); }
        }

        #endregion

        #region Members

        [DefinitionLibrary.Method.All]
        [return: DefinitionLibrary.ReturnValue.All]
        int Method<[DefinitionLibrary.GenericParameter.All] TMethod>([DefinitionLibrary.Parameter.All] int parameter)
        {
            return parameter;
        }

        #endregion
    }
}