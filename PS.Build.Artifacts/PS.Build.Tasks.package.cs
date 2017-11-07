using PS.Build.Nuget.Attributes;

#if DEBUG

[assembly: Nuget(Title = "PS.Build Engine", ID = "PS.Build.Tasks")]
[assembly: Nuget(Description = "PS.Build adaptation toolchain engine", ID = "PS.Build.Tasks")]
[assembly: Nuget(Copyright = "Copyright © Vladimir Shkolka, 2017", ID = "PS.Build.Tasks")]
[assembly: Nuget(ProjectUrl = "https://github.com/BlackGad/PS.Build", ID = "PS.Build.Tasks")]
[assembly: Nuget(LicenseUrl = "https://github.com/BlackGad/PS.Build/blob/master/LICENSE", ID = "PS.Build.Tasks")]
[assembly: Nuget(Tags = "PS.Build MSBuild Roslyn", ID = "PS.Build.Tasks")]
[assembly: NugetAuthor("Vladimir Shkolka", ID = "PS.Build.Tasks")]

[assembly: NugetFiles(@"{dir.project}PS.Build.Tasks.props", @"build", ID = "PS.Build.Tasks")]
[assembly: NugetFiles(@"{dir.project}PS.Build.Tasks.targets", @"build", ID = "PS.Build.Tasks")]

[assembly: NugetFiles(@"{dir.solution}PS.Build.Tasks\bin\{prop.configuration}\*.dll", @"tasks\MSBuild", ID = "PS.Build.Tasks")]
[assembly: NugetFiles(@"{dir.solution}PS.Build.Tasks\bin\{prop.configuration}\PS.Build.Tasks.dll.config", @"tasks\MSBuild", ID = "PS.Build.Tasks")]

[assembly: NugetBuild(@"{dir.solution}_Artifacts\{prop.configuration}.{prop.platform}", ID = "PS.Build.Tasks")]
[assembly: NugetDebugSubstitution(ID = "PS.Build.Tasks")]

#endif