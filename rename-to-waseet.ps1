# PowerShell script to rename CQRS.Mediator to Waseet.CQRS

Write-Host "Renaming CQRS.Mediator to Waseet.CQRS..." -ForegroundColor Green

# Update all .cs files - replace namespace declarations
Get-ChildItem -Path . -Recurse -Include *.cs | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace 'namespace CQRS\.Mediator', 'namespace Waseet.CQRS'
    $content = $content -replace 'using CQRS\.Mediator', 'using Waseet.CQRS'
    Set-Content -Path $_.FullName -Value $content -NoNewline
    Write-Host "Updated: $($_.Name)" -ForegroundColor Yellow
}

# Update all .csproj files
Get-ChildItem -Path . -Recurse -Include *.csproj | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace 'CQRS\.Mediator', 'Waseet.CQRS'
    Set-Content -Path $_.FullName -Value $content -NoNewline
    Write-Host "Updated: $($_.Name)" -ForegroundColor Yellow
}

# Update all .md files
Get-ChildItem -Path . -Include *.md | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace 'CQRS\.Mediator', 'Waseet.CQRS'
    $content = $content -replace 'CQRS Mediator', 'Waseet.CQRS'
    Set-Content -Path $_.FullName -Value $content -NoNewline
    Write-Host "Updated: $($_.Name)" -ForegroundColor Yellow
}

# Update .sln file
Get-ChildItem -Path . -Include *.sln | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace 'CQRS\.Mediator', 'Waseet.CQRS'
    Set-Content -Path $_.FullName -Value $content -NoNewline
    Write-Host "Updated: $($_.Name)" -ForegroundColor Yellow
}

Write-Host "`nAll files updated!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Close Visual Studio if open" -ForegroundColor White
Write-Host "2. Rename folder: src\CQRS.Mediator -> src\Waseet.CQRS" -ForegroundColor White
Write-Host "3. Rename folder: tests\CQRS.Mediator.Sample -> tests\Waseet.CQRS.Sample" -ForegroundColor White
Write-Host "4. Rename files:" -ForegroundColor White
Write-Host "   - src\Waseet.CQRS\CQRS.Mediator.csproj -> Waseet.CQRS.csproj" -ForegroundColor White
Write-Host "   - tests\Waseet.CQRS.Sample\CQRS.Mediator.Sample.csproj -> Waseet.CQRS.Sample.csproj" -ForegroundColor White
Write-Host "5. Run: dotnet build" -ForegroundColor White
