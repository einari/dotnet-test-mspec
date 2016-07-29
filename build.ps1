Param(
    [switch] $CI,
    [string] $Version = "0.2.0-alpha3",
    [string] $Configuration = "Release"
)

function Invoke-ExpressionExitCodeCheck([string] $command)
{
    Invoke-Expression $command

    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

[string] $testProjectsDirectory = "test"
[string] $codeProjectsDirectory = "src"
[string] $nugetOutputDirectory = "Build"

# Patch version
if ($CI) {
    Write-Host "Patching versions to ${version}..."

    Get-ChildItem $codeProjectsDirectory -Recurse -File -Filter "project.json" | ForEach {
        Write-Host "Updating version: $($_.FullName)"

        $foundVersion = $false; # replace only the first occurance of versin (assumes package version is on top)

        (Get-Content $_.FullName -ErrorAction Stop) |
            Foreach-Object {
                $versionLine = '"version":\s*?".*?"'
                if (($foundVersion -ne $true) -and ($_ -match $versionLine)) {
                    $foundVersion = $true;
                    return $_ -replace '"version":\s*?".*?"',"""version"": ""$Version"""
                } else {
                    return $_
                }
            } |
            Set-Content $_.FullName -ErrorAction Stop
    }
}

# Restore Packages
Write-Host "Restoring packages..."

Invoke-ExpressionExitCodeCheck "dotnet restore"


# Build
Write-Host "Building in ${Configuration}..."

Invoke-ExpressionExitCodeCheck "dotnet build ${codeProjectsDirectory}\**\project.json -c ${configuration}" -ErrorAction Stop


# Run tests
Write-Host "Running tests..."

[bool] $testsFailed = $false

Get-ChildItem $testProjectsDirectory -Directory | ForEach {
    Invoke-Expression "dotnet test $($_.FullName)"

    if (!$testsFailed) {
        $testsFailed = $LASTEXITCODE -and $LASTEXITCODE -ne 0
    }
}

if ($testsFailed) {
    Write-Host -BackgroundColor Red -ForegroundColor Yellow "Tests failed!"
    exit -1
} else {
    Write-Host -BackgroundColor Green -ForegroundColor White "All good!"
}

# NuGet packaging
if ($CI) {
    Write-Host "Creating a nuget package in ${nugetOutputDirectory}"

    Get-ChildItem $codeProjectsDirectory -Directory | ForEach {
        Invoke-ExpressionExitCodeCheck "dotnet pack $($_.FullName) -c ${configuration} -o ${nugetOutputDirectory} --version-suffix ${version}"
    }
}