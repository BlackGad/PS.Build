using System;

namespace UsageOrderingLibrary
{
    [DefinitionLibrary.Interface.All]
    public interface IInterface<[DefinitionLibrary.GenericParameter.All] TInterface>
    {
        #region Properties

        [DefinitionLibrary.Property.All]
        int this[[DefinitionLibrary.Parameter.All] int index]
        {
            [return: DefinitionLibrary.ReturnValue.All]
            get;
            [DefinitionLibrary.Method.All]
            set;
        }

        [DefinitionLibrary.Property.All]
        int Property { get; set; }

        #endregion

        #region Events

        [DefinitionLibrary.Event.All]
        event EventHandler Event;

        #endregion

        #region Members

        [DefinitionLibrary.Method.All]
        [return: DefinitionLibrary.ReturnValue.All]
        int Method<[DefinitionLibrary.GenericParameter.All] TMethod>([DefinitionLibrary.Parameter.All] int parameter);

        #endregion
    }
}