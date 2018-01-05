using PS.Build.Essentials.Attributes;

//https://confluence.jetbrains.com/display/TCD10/Configuring+General+Settings#ConfiguringGeneralSettings-ArtifactPaths
//https://github.com/adamhathcock/sharpcompress
//https://github.com/monemihir/LZMA-SDK

[assembly: FilesCopy(@"{dir.target}\archive.zip!hello\**\*.*", @"{dir.target}\archive2.zip!")]