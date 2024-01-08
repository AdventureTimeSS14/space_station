cd ../../

dotnet restore
dotnet build --configuration DebugOpt --no-restore /p:WarningsAsErrors=nullable /m

mkdir Scripts/logs

rm Scripts/logs/Content.Tests.log
dotnet test --no-build --configuration DebugOpt Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0 > Scripts/logs/Content.Tests.log

rm Scripts/logs/Content.IntegrationTests.log
dotnet test --no-build --configuration DebugOpt Content.IntegrationTests/Content.IntegrationTests.csproj -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed > Scripts/logs/Content.IntegrationTests.log

echo "Tests complete. Press enter to continue."
read
