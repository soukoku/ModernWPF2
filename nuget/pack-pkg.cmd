@echo off

@echo compiling solution...
if exist "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" (
	set vcbat="C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat"
)

IF NOT DEFINED vcbat (
	set error="No Compatible Visual Studio (2015) found."
	goto error;
)

call %vcbat% x86
msbuild ..\ModernWpf2.sln /t:Build /p:Configuration=Release /m

@echo packing nuget
nuget pack ..\src\ModernWpf.Core\ModernWpf.Core.csproj -Prop Configuration=Release
nuget pack ..\src\ModernWpf\ModernWpf.csproj -IncludeReferencedProjects -Prop Configuration=Release
goto end;

:error
echo Error: %error%

:end
pause