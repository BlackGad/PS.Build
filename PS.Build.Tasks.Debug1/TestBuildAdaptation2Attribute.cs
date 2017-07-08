using System;
using System.ComponentModel;
using PS.Build.Services;

namespace PS.Build.Tasks.Debug1
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    //[Serializable]
    [Designer("PS.Build.Adaptation")]
    public sealed class TestBuildAdaptation2Attribute : Attribute
    {
        private readonly Type _baseType;

        #region Constructors

        public TestBuildAdaptation2Attribute(Type baseType)
        {
            _baseType = baseType;
        }

        #endregion

        #region Properties

        public string Prop { get; set; }

        #endregion

        #region Members

        public void PostBuild(IServiceProvider serviceProvider)
        {
            var logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
			logger.Info("----------------------- Hi from here");
			// var item = new CompileItem(Path.Combine(context.GetFolder(FolderType.IntermediateFolder), _baseType.FullName + ".test.cs"));
            // File.WriteAllText(item.Location, "namespace PS.Build.Tasks.Debug { class "+ _baseType.Name + "test { public test(int a, string asd){ }} }");
            // context.AddItem(item);
        }

        #endregion
    }
}