using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: NugetFilesFromTarget]
[assembly: NugetPackageDependenciesFromConfiguration]
//[assembly: NugetPackageDependenciesFilter("NuGet.*")]
//[assembly: NugetPackageDependenciesFilter("PS.Build.*")]
//[assembly: NugetPackageDependenciesFilter("Newtonsoft.Json")]
[assembly: NugetBuild]

#endif