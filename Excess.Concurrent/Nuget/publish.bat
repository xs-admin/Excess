
..\..\.nuget\nuget pack Excess.Concurrent.nuspec
..\..\.nuget\nuget push Excess.Concurrent.0.48.10-alpha.nupkg

del Excess.Concurrent.0.48.10-alpha.nupkg

..\..\.nuget\nuget pack Excess.Concurrent.Runtime.nuspec
..\..\.nuget\nuget push Excess.Concurrent.Runtime.0.48.10-alpha.nupkg

del Excess.Concurrent.Runtime.0.48.10-alpha.nupkg