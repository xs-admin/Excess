
..\..\.nuget\nuget pack Excess.Compiler.nuspec
..\..\.nuget\nuget push Excess.Compiler.0.47.5-alpha.nupkg

del Excess.Compiler.0.47.5-alpha.nupkg

..\..\.nuget\nuget pack Excess.Runtime.nuspec
..\..\.nuget\nuget push Excess.Runtime.0.47.0-alpha.nupkg

del Excess.Runtime.0.47.0-alpha.nupkg