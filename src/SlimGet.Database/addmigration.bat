@echo off

dotnet ef migrations add --startup-project ..\SlimGet -o Migrations "%*"
dotnet ef migrations script --startup-project ..\SlimGet
