$npmPackages = Get-ChildItem -Path .\artifacts\packages -Filter "*.tgz"

foreach($npmPackage in $npmPackages)
{
    & "npm" publish $npmPackage.FullName --registry https://www.myget.org/F/dserfozo/npm/
}

$nugetPackages = Get-ChildItem -Path .\artifacts\packages -Filter "*.nupkg"

foreach($nugetPackage in $nugetPackages)
{
    & "nuget.exe" push $nugetPackage.FullName -Source https://www.myget.org/F/dserfozo/api/v2/package -ApiKey "$env:apikey"
}