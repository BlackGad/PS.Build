# PS.Build: Toolchain to control msbuild process via C# code
[![NuGet Version](https://img.shields.io/nuget/v/PS.Build.svg?label=master+nuget)](https://www.nuget.org/packages?q=PS.Build)
[![Build status](https://ci.appveyor.com/api/projects/status/ki1xn6w347k0vord?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build)
[![MyGet CI](https://img.shields.io/myget/ps-projects/vpre/ps.build.svg?label=CI+nuget)](https://www.myget.org/gallery/ps-projects)
[![Build status](https://ci.appveyor.com/api/projects/status/ixmnwi3hxi4jot9b?svg=true)](https://ci.appveyor.com/project/BlackGad/ps-build-xhs18)

Build process with [MSBuild](https://msdn.microsoft.com/en-us/library/0k6kkbsd.aspx) engine is powerful and simple. Current toolchain allows developer to extend and adapt his project build process using inline C# code via [Attributes](https://msdn.microsoft.com/en-us/library/aa288454(v=vs.71).aspx).

## Future
First implementation showed its best and worst sides. 

### Plusses:
* It really works :)

### Minuses:
* Assembly dependencies isolation battlefield. Cause really big issues in external adaptations.
* Slows build process.

Next step - rewrite assembly dependencies issolation. This will cause some functionality downgrade (source code processing with internal Roslyn components in adaptations, data sharing with vault service.). But it is necessary for future platform evolution.

## Known adaptations
* [NuGet](https://github.com/BlackGad/PS.Build.Nuget) - NuGet stuff via attributes 

## Usage
Build process extending have 2 logical parts 
*	Attribute definition
*	Adaptation applying

[Attribute definition](https://github.com/BlackGad/PS.Build/wiki/Adaptation-attribute) looks like generic C# attribute definition with additional ```DesignerAttribute("PS.Build.Adaptation")``` attribute and several implemented methods with specific signature. Available methods:
* [Setup](https://github.com/BlackGad/PS.Build/wiki/Setup-method) - called before any other method. Here you can setup adaptation specific logic.
* [PreBuild](https://github.com/BlackGad/PS.Build/wiki/PreBuild-method) - called before build. Here you can analyze MSBuild environment before build. Also gives the opportunity to add additional generated MSBuild items.
* [PostBuild](https://github.com/BlackGad/PS.Build/wiki/PostBuild-method) – called after build. Here you can setup deploy, clean etc tasks.

To execute defined earlier instructions you must add reference to [PS.Build.Tasks](https://www.nuget.org/packages/PS.Build.Tasks/) nuget package and apply attributes to relevant elements. **NOTE**: attribute definition must be in separate assembly. Adaptation definition and usage in same assembly not implemented yet (Technically it is possible but whole build process time will be dramatically increased). 

Adaptation definition example:
```csharp
using System;
using System.ComponentModel;
using PS.Build.Extensions;
using PS.Build.Services;

namespace Test
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [Designer("PS.Build.Adaptation")]
    public sealed class AdaptationAttribute : Attribute
    {
        static void Setup(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            logger.Info("Initialization");
        }

        private readonly int _essentialQuestion;

        #region Constructors

        public AdaptationAttribute(int essentialQuestion)
        {
            _essentialQuestion = essentialQuestion;
        }

        #endregion

        #region Members

        void PostBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            logger.Info("Goodbye cruel world");
        }

        void PreBuild(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger>();
            logger.Info("Hello world");
            logger.Info("The answer is: " + _essentialQuestion);
        }

        #endregion
    }
}
```

Adaptation usage example:
```csharp
using Test;

[assembly: Adaptation(42)]
```
## How it works
[PS.Build.Tasks nuget package](https://www.nuget.org/packages/PS.Build.Tasks/) contains [MSBuild task](https://msdn.microsoft.com/en-us/library/t9883dzc.aspx) which uses [Roslyn](https://github.com/dotnet/roslyn) engine to analyze target assembly source code for adaptation attributes usage prior to compilation and execute their methods with specific signature.
## Adaptation methods invocation order
Adaptation methods calls are strongly [ordered](https://github.com/BlackGad/PS.Build/wiki/Method-invocation-order).
<img src="https://rawgit.com/BlackGad/PS.Build/master/.Assets/ExecutionOrder.svg"/>
<!--<img src="https://cdn.rawgit.com/BlackGad/PS.Build/master/.Assets/ExecutionOrder.svg"/>-->
This feature allows you to control cross attribute interaction using [dynamic vault service](https://github.com/BlackGad/PS.Build/wiki/Dynamic-vault-service)

## Attributes isolation
It is crucial to keep compiled output in clean state with minimum amount of dependencies. Main adaptation attributes mission - allow you to modify build process. There is no reason to stay in runtime code after build process. That's why attributes must be isolated with **ADAPTATION** preprocessor directive. Escape all your attributes with this directive. Generic behavior for any file with not isolated adaptations - source code rewriting with cutted definitions to intermediate folder.

## Intellisense issues
When you add additional **COMPILE** items on [PreBuild](https://github.com/BlackGad/PS.Build/wiki/PreBuild-method) instruction with [artifactory service](https://github.com/BlackGad/PS.Build/wiki/Artifactory-service) your intellisense will not resolve them automatically. It happens because intelisense engine parses your compile items independenly from MSBuild. To solve this you have two major options:
* Compile project and reload it with out cleaning. Intellisense updates on project load so it will discover new items and parse them.
* Add to **COMPILE** item where you applying attribute (and all dependant items) metadata `<Generator>MSBuild:Compile</Generator>` it forces intellisense to update on every file change.

## Documentation
Additional information could be found at project [wiki page](https://github.com/BlackGad/PS.Build/wiki)
