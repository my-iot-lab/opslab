# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$slnPath = Join-Path $packFolder "../OpsLab/"
$srcPath = Join-Path $slnPath "src"

# List of projects
$projects = (
    "Ops.Communication",
	"Ops.Exchange",
	"Ops.Extensions.Zero",
	"Ops.Extensions.Zero.EntityFrameworkCore"
)

# Rebuild solution
Set-Location $slnPath
& dotnet restore

# Copy all nuget packages to the pack folder
foreach($project in $projects) {
    
    $projectFolder = Join-Path $srcPath $project

    # Create nuget pack
    Set-Location $projectFolder
    Get-ChildItem (Join-Path $projectFolder "bin/Release") -ErrorAction SilentlyContinue | Remove-Item -Recurse
    & dotnet msbuild /p:Configuration=Release
    & dotnet msbuild /p:Configuration=Release /t:pack /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg

    # Copy nuget package
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $project + ".*.nupkg")
    Move-Item $projectPackPath $packFolder

	# Copy symbol package
    $projectPackPath = Join-Path $projectFolder ("/bin/Release/" + $project + ".*.snupkg")
    Move-Item $projectPackPath $packFolder
}

# Go back to the pack folder
Set-Location $packFolder
