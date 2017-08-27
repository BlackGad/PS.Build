using PS.Build.Essentials.Attributes;

//https://confluence.jetbrains.com/display/TCD10/Configuring+General+Settings#ConfiguringGeneralSettings-ArtifactPaths
//https://github.com/adamhathcock/sharpcompress
//https://github.com/monemihir/LZMA-SDK

[assembly: GroupImport(@"{dir.target}\**\*.*")]
[assembly: GroupFilter(@"*.pdb")]
//

[assembly: GroupActionCopy(@"{dir.target}\Hello", SelectPattern = @"**\*.*")]
[assembly: GroupActionRemove(SelectPattern = @"**\*.*")]
//

[assembly: GroupFork(@"*", Groups.Default, Group = Groups.Alpha)]
//

[assembly: GroupActionPack(@"{dir.target}\backup.zip", "hello", Group = Groups.Alpha)]
[assembly: GroupActionMove(@"{dir.target}\Forked", Group = Groups.Alpha)]
//

[assembly: GroupImport(@"{dir.target}\archive.zip!**\*.*", Group = Groups.Beta)]
[assembly: GroupFilter(@"*.pdb", Group = Groups.Beta)]
[assembly: GroupActionMove(@"{dir.target}\archive.zip", Group = Groups.Beta)]
//

[assembly: Copy(@"{dir.target}\archive.zip!hello\**\*.*", @"{dir.target}\archive2.zip!")]