using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PS.Build.Tasks
{
    public class SandboxAssemblyResolver : MarshalByRefObject,
                                           IDisposable
    {
        private readonly string[] _assemblyReferences;

        #region Constructors

        public SandboxAssemblyResolver(string[] assemblyReferences)
        {
            if (assemblyReferences == null) throw new ArgumentNullException(nameof(assemblyReferences));
            _assemblyReferences = assemblyReferences;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        #endregion

        #region Event handlers

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var queryAssemblyName = args.Name.Split(',').FirstOrDefault();
            var resolved = _assemblyReferences.FirstOrDefault(r => string.Equals(Path.GetFileNameWithoutExtension(r),
                                                                                 queryAssemblyName,
                                                                                 StringComparison.InvariantCultureIgnoreCase));

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            resolved = resolved ??
                       FindAtLocation(queryAssemblyName, assemblyLocation) ??
                       FindAtLocation(queryAssemblyName, domainBaseDirectory);

            return string.IsNullOrWhiteSpace(resolved)
                ? null
                : Assembly.LoadFile(resolved);
        }

        #endregion

        #region Members

        private string FindAtLocation(string queryAssemblyName, string location)
        {
            if (string.IsNullOrEmpty(location)) return null;
            return Directory.GetFiles(location, "*.dll")
                            .FirstOrDefault(r => string.Equals(Path.GetFileNameWithoutExtension(r),
                                                               queryAssemblyName,
                                                               StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion
    }
}