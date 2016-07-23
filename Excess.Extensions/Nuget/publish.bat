
..\..\.nuget\nuget pack Excess.NInjector.nuspec
..\..\.nuget\nuget push Excess.Extensions.NInjector.0.47.0-alpha.nupkg

del Excess.Extensions.NInjector.0.47.0-alpha.nupkg

..\..\.nuget\nuget pack Excess.Dapper.nuspec
..\..\.nuget\nuget push Excess.Extensions.Dapper.0.48.0-alpha.nupkg

del Excess.Extensions.Dapper.0.48.0-alpha.nupkg

..\..\.nuget\nuget pack Excess.Extensions.nuspec
..\..\.nuget\nuget push Excess.Extensions.0.47.5-alpha.nupkg

del Excess.Extensions.0.47.5-alpha.nupkg