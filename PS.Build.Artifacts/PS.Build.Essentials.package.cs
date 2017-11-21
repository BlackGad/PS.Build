using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: Nuget(Title = "PS.Build Essentials", ID = "PS.Build.Essentials")]
[assembly: Nuget(Description = "Essential adaptations implementation for PS.Build adaptation toolchain", ID = "PS.Build.Essentials")]
[assembly: Nuget(Copyright = "Copyright © Vladimir Shkolka, 2017", ID = "PS.Build.Essentials")]
[assembly: Nuget(ProjectUrl = "https://github.com/BlackGad/PS.Build", ID = "PS.Build.Essentials")]
[assembly: Nuget(LicenseUrl = "https://github.com/BlackGad/PS.Build/blob/master/LICENSE", ID = "PS.Build.Essentials")]
[assembly: Nuget(Tags = "PS.Build MSBuild Roslyn", ID = "PS.Build.Essentials")]
[assembly: NugetAuthor("Vladimir Shkolka", ID = "PS.Build.Essentials")]

[assembly: NugetFiles(@"{dir.solution}PS.Build.Essentials\bin\{prop.configuration}\PS.Build.Essentials.dll", @"lib\{nuget.framework}", ID = "PS.Build.Essentials")]
[assembly: NugetPackageAssemblyReference(@"PS.Build.Essentials.dll", ID = "PS.Build.Essentials")]

[assembly: NugetBuild(@"{dir.solution}_Artifacts\{prop.configuration}.{prop.platform}", ID = "PS.Build.Essentials")]
[assembly: NugetDebugSubstitution(ID = "PS.Build.Essentials")]

#endif