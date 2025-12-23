@echo off
setlocal enabledelayedexpansion

echo Updating all Markdown files to use Waseet.CQRS...

cd /d "c:\Users\hosman-c\source\repos\CQRS"

for %%f in (*.md) do (
    echo Processing: %%f
    powershell -Command "(Get-Content '%%f') -replace 'CQRS\.Mediator', 'Waseet.CQRS' | Set-Content '%%f'"
    powershell -Command "(Get-Content '%%f') -replace 'CQRS Mediator', 'Waseet.CQRS' | Set-Content '%%f'"
)

echo.
echo All markdown files updated!
echo.
echo Next steps:
echo 1. Rename folder: src\CQRS.Mediator to src\Waseet.CQRS
echo 2. Rename folder: tests\CQRS.Mediator.Sample to tests\Waseet.CQRS.Sample
echo 3. Run: dotnet build
pause
