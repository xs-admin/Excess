
..\..\.nuget\nuget pack Excess.NInjector.nuspec
..\..\.nuget\nuget push Excess.NInjector.0.47.0-alpha.nupkg

del Excess.Concurrent.0.46.0-alpha.nupkg

..\..\.nuget\nuget pack Excess.Extensions.nuspec
..\..\.nuget\nuget push Excess.Extensions.0.47.0-alpha.nupkg

del Excess.Extensions.0.47.0-alpha.nupkg