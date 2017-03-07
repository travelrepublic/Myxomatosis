@echo off
cls

REM .paket\paket.bootstrapper.exe
REM if errorlevel 1 (
REM   exit /b %errorlevel%
REM )

REM .paket\paket.exe restore
REM if errorlevel 1 (
REM   exit /b %errorlevel%
REM )
tools\Fake\tools\FAKE.exe build.fsx %*
