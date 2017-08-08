using PS.Build.Essentials.Attributes;

//https://confluence.jetbrains.com/display/TCD10/Configuring+General+Settings#ConfiguringGeneralSettings-ArtifactPaths
//https://github.com/adamhathcock/sharpcompress
//https://github.com/monemihir/LZMA-SDK

[assembly: FgSelect(@"{dir.target}\**\*.*")]
[assembly: FgFilter(@"*.pdb")]
//

[assembly: FgActionCopy(@"{dir.target}\Hello", SelectPattern = @"**\*.*")]
[assembly: FgActionRemove(SelectPattern = @"**\*.*")]
//

[assembly: FgFork(@"*", FileGroups.Default, Group = FileGroups.Alpha)]
//

[assembly: FgActionPack(@"{dir.target}\backup.zip", "hello", Group = FileGroups.Alpha)]
[assembly: FgActionMove(@"{dir.target}\Forked", Group = FileGroups.Alpha)]
//

[assembly: FgSelectFromArchive(@"{dir.target}\archive.zip", @".\**\*.*", Group = FileGroups.Beta)]
[assembly: FgFilter(@"*.pdb", Group = FileGroups.Beta)]
[assembly: FgActionMove(@"{dir.target}\archive.zip", Group = FileGroups.Beta)]
//

[assembly: FsCopy(@"{dir.target}\archive.zip!hello\**\*.*", @"{dir.target}\archive2.zip!")]