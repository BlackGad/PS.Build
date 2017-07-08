using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using PS.Build.Services;
using PS.Build.Types;

namespace PS.Build.Tasks.Debug1
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    //[Serializable]
    [Designer("PS.Build.Adaptation")]
    public sealed class TestBuildAdaptationAttribute : Attribute
    {
        private readonly string _className;
        private readonly string _ns;

        #region Constructors

        public TestBuildAdaptationAttribute(string ns, string className)
        {
            if (ns == null) throw new ArgumentNullException("ns");
            if (className == null) throw new ArgumentNullException("className");
            _ns = ns;
            _className = className;
        }

        #endregion

        #region Members

		private void PostBuild(IServiceProvider serviceProvider)
		{
			var logger = (ILogger)serviceProvider.GetService(typeof(ILogger));	
		}
		
        private void PreBuild(IServiceProvider serviceProvider)
        {
            var logger = (ILogger)serviceProvider.GetService(typeof(ILogger));
            var explorer = (IExplorer)serviceProvider.GetService(typeof(IExplorer));
            var nugetExplorer = (INugetExplorer)serviceProvider.GetService(typeof(INugetExplorer));
            var artifactory = (IArtifactory)serviceProvider.GetService(typeof(IArtifactory));

            logger.Info(" * Target folder: " + explorer.Directories[BuildDirectory.Target]);
            logger.Info(" * Intermediate folder: " + explorer.Directories[BuildDirectory.Intermediate]);
            logger.Info(" * Project folder: " + explorer.Directories[BuildDirectory.Project]);
			
            logger.Info(" * Package: " + nugetExplorer.FindPackage("AutoMapper"));
			
			a = 1;
			
            foreach (var item in explorer.Items[BuildItem.None])
            {
                logger.Info(" * " + item.ModifiedTime + ": " + item.FullPath);
                foreach (var pair in item.Metadata)
                {
                    logger.Info("   - " + pair.Key + ": " + pair.Value);
                } 
            }
            var filePath = Path.Combine(explorer.Directories[BuildDirectory.Intermediate],
                                        "Generated",
                                        string.Join("_", _ns, _className) + ".cs");
             
            var artifact = artifactory.Artifact(filePath, BuildItem.Compile)
                                      .Content(() =>
                                      {
                                          var code = "namespace " + _ns + " { class " + _className + " {} }";
                                          return Encoding.UTF8.GetBytes(code);
                                      });
            artifact.Dependencies()
                    .TagDependency("NS: " + _ns)
                    .TagDependency("ClassName: " + _className);
        }

        #endregion
    }
}