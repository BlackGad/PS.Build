# PS.Build: Toolchain to control msbuild process via C# code
[![NuGet Version](https://img.shields.io/nuget/v/PS.Build.svg?label=master+nuget)](https://www.nuget.org/packages?q=PS.Build)
[![Build status](https://ci.appveyor.com/api/projects/status/ki1xn6w347k0vord?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build)
[![MyGet CI](https://img.shields.io/myget/ps-projects/vpre/PS.Build.svg?label=CI+nuget)](https://www.myget.org/gallery/ps-projects)
[![Build status](https://ci.appveyor.com/api/projects/status/ixmnwi3hxi4jot9b?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-xhs18)

Build process with [MSBuild](https://msdn.microsoft.com/en-us/library/0k6kkbsd.aspx) engine is powerful and simple. Current toolchain allows developer to extend and adapt his project build process using inline C# code via [Attributes](https://msdn.microsoft.com/en-us/library/aa288454(v=vs.71).aspx).

## Usage
Build process extending have 2 logical parts 
*	Attribute definition
*	Adaptation applying

[Attribute definition](https://github.com/BlackGad/PS.Build/wiki/Adaptation-attribute) looks like generic C# attribute definition with additional ```DesignerAttribute("PS.Build.Adaptation")``` attribute and several implemented methods with specific signature. Available methods:
* [PreBuild](https://github.com/BlackGad/PS.Build/wiki/PreBuild-method) - called before build. Here you can analyze MSBuild environment before build. Also gives the opportunity to add additional generated MSBuild items.
* [PostBuild](https://github.com/BlackGad/PS.Build/wiki/PostBuild-method) – called after build. Here you can setup deploy, clean etc tasks with generated items.
To execute defined earlier instructions you must add reference to PS.Build.Tasks nuget package and apply attributes to relevant elements. NOTE: attribute definition must be in separate assembly. Same assembly attribute definition not implemented yet (Technically it is possible but whole build process time will be dramatically increased). 

## How it works
[PS.Build.Tasks nuget package](https://www.nuget.org/packages/PS.Build.Tasks/) contains [MSBuild task](https://msdn.microsoft.com/en-us/library/t9883dzc.aspx) which uses [Roslyn](https://github.com/dotnet/roslyn) engine to analyze target assembly source code for adaptation attributes usage prior to compilation and execute their methods with specific signature.

## Attributes isolation
It is crucial to keep compiled output in clean state with minimum amount of dependencies. That's why attributes can be isolated in several ways.
#### Lazy C# isolation
Adaptation attributes do not require any additional types to be defined to. [DesignerAttribute](https://msdn.microsoft.com/en-us/library/system.componentmodel.designerattribute(v=vs.110).aspx) attribute and [IServiceProvider](https://msdn.microsoft.com/en-us/library/system.iserviceprovider(v=vs.110).aspx) interface are public .NET Framework types. So any internal methods content does not require to load additional types which used in adaptation process. Simple attributes scan will not try to resolve this types unless you define them as public contructor parameters or public properties. Also with [GetCustomAttributes](https://msdn.microsoft.com/en-us/library/system.type.getcustomattributes(v=vs.110).aspx) constructor will be called. Thats why try to not use additional types for adaptation in it. And because previous advice makes the use of attributes uncomfortable another isolation way exist.
#### Preprocessor directives isolation
Main adaptation attributes mission - allow you to modify build process. So their existence in source code make sense only till compile finished. There is no reasons to stay in runtime code after build process. That's why syntax trees analyzed with **DEBUG** preprocessor directive. Escape all your attributes with this directive. It is comfortable way to apply them in debug mode with intellisense and to drop them in release build from source code at all.

## Documentation
Additional information could be found at project [wiki page](https://github.com/BlackGad/PS.Build/wiki)

