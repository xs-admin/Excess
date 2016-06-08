
..\..\.nuget\nuget pack Excess.Server.nuspec
..\..\.nuget\nuget push Excess.Server.0.47.0-alpha.nupkg
..\..\.nuget\nuget pack Excess.Server.Runtime.nuspec
..\..\.nuget\nuget push Excess.Server.Runtime.0.47.0-alpha.nupkg

del Excess.Server.0.47.0-alpha.nupkg
del Excess.Server.Runtime.0.47.0-alpha.nupkg