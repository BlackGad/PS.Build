using DefinitionAttributeInitialization;

[assembly: CustomAll(1, b: "hi", D = 1.2, ID = "inh")]
[assembly: CustomPreBuild(1, b: "hi", D = 1.2, ID = "inh")]
[assembly: CustomPostBuild(1, b: "hi", D = 1.2, ID = "inh")]