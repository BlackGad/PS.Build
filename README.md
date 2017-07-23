# PS.Build: Toolchain to control msbuild process via C# code
Build process with [MSBuild](https://msdn.microsoft.com/en-us/library/0k6kkbsd.aspx) engine is powerful and simple. Current toolchain allows developer to extend and adapt his project build process using inline C# code via [Attributes](https://msdn.microsoft.com/en-us/library/aa288454(v=vs.71).aspx).

*Master*

[![NuGet Version](https://img.shields.io/nuget/v/PS.Build.svg?label=master+nuget)](https://www.nuget.org/packages?q=PS.Build)
[![Build status](https://ci.appveyor.com/api/projects/status/ki1xn6w347k0vord?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build)

*CI*

[![MyGet CI](https://img.shields.io/myget/ps-projects/vpre/PS.Build.svg?label=CI+nuget)](https://www.myget.org/gallery/ps-projects)
[![Build status](https://ci.appveyor.com/api/projects/status/ixmnwi3hxi4jot9b?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-xhs18)


## Usage
Build process extending have 2 logical parts 
*	Attribute definition
*	Adaptation applying

Attribute definition looks like generic C# attribute definition with additional ```DesignerAttribute("PS.Build.Adaptation")``` attribute and several implemented methods with specific signature. Available methods:
* ```void PreBuild(IServiceProvider)``` - called before build. Here you can analyze MSBuild environment before build. Also gives the opportunity to add additional generated MSBuild items.
* ```void PostBuild(IServiceProvider)``` – called after build. Here you can setup deploy, clean etc tasks with generated items.
To execute defined earlier instructions you must add reference to PS.Build.Tasks nuget package and apply attributes to relevant elements. NOTE: attribute definition must be in separate assembly. Same assembly attribute definition not implemented yet (Technically it is possible but whole build process time will be dramatically increased). 
