using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: Nuget(Title = "PS.Build API", ID = "PS.Build")]
[assembly: Nuget(Description = "Public API for PS.Build adaptation toolchain", ID = "PS.Build")]
[assembly: Nuget(Copyright = "Copyright © Vladimir Shkolka, 2017", ID = "PS.Build")]
[assembly: Nuget(ProjectUrl = "https://github.com/BlackGad/PS.Build", ID = "PS.Build")]
[assembly: Nuget(LicenseUrl = "https://github.com/BlackGad/PS.Build/blob/master/LICENSE", ID = "PS.Build")]
[assembly: Nuget(Tags = "PS.Build MSBuild Roslyn", ID = "PS.Build")]
[assembly: NugetAuthor("Vladimir Shkolka", ID = "PS.Build")]

[assembly: NugetFrameworkReference("System.ComponentModel.DataAnnotations", ID = "PS.Build")]
[assembly: NugetFiles(@"{dir.solution}PS.Build\bin\{prop.configuration}\PS.Build.dll", @"lib\{nuget.framework}", ID = "PS.Build")]
[assembly: NugetPackageAssemblyReference(@"PS.Build.dll", ID = "PS.Build")]

[assembly: NugetBuild(@"{dir.solution}_Artifacts\{prop.configuration}.{prop.platform}", ID = "PS.Build")]

#endif