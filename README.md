# cie-nis-dotnetcore-sdk
SDK for reading and validate the NIS code from an Italian Electronic Identity Card (CIE) using .NET Core

## Introduction

This library enables developers an easy way to validate and acquire NIS number for multipurpose use. It's written in .net core 2.2 and can be compiled for Windows, Linux and Mac.

## Test Example

Test project is the best way to check library usage and consist in few steps:
* Istantiate smartcard processor
* Map processor events (Read, Error)
* Start processor

```
static void Main(string[] args)
{
    Console.WriteLine("Example for reading and validate the NIS code from an Italian Electronic Identity Card (CIE)\n");            
    using (var pr = new Processor())
    {
        pr.OnException += OnException;
        pr.OnReadComplete += OnReadComplete;
        pr.Start();
        Console.WriteLine("Put your CIE on smartcard reader to start.");
        Console.WriteLine("Press any key to end program");
        Console.ReadLine();
        pr.Stop();
    }               
}
public static void OnException(object sender, Exception e)
{
    Console.WriteLine(e.Message);
}
public static void OnReadComplete(object sender, string e)
{
    if (e != null)
        Console.WriteLine($"NIS {e} verified");
    else
        Console.WriteLine("NIS is not valid");            
}
```
### Install SDK

The software development kit (SDK) includes everything you need to build and run .NET Core applications, using command line tools and any editor (like Visual Studio).
You can download .NET Core 2.2 from [.Net Core Project](https://dotnet.microsoft.com/download) for your operating system.

To build Test project you must open system console, navigate to test project folder and use dotnet build command
```
dotnet build [<PROJECT>|<SOLUTION>] [-c|--configuration] [-f|--framework] [--force] [--interactive] [--no-dependencies]
    [--no-incremental] [--nologo] [--no-restore] [-o|--output] [-r|--runtime] [-v|--verbosity] [--version-suffix]

dotnet build [-h|--help]
```

To publish application you must open system console, navigate to test project folder and use 
dotnet publish command
```
dotnet publish [<PROJECT>] [-c|--configuration] [-f|--framework] [--force] [--manifest] [--no-build] [--no-dependencies]
    [--no-restore] [-o|--output] [-r|--runtime] [--self-contained] [-v|--verbosity] [--version-suffix]
dotnet publish [-h|--help]
```

### Runtime

The runtime includes everything you need to run .NET Core applications. The runtime is also included in the SDK. You can download .NET Core 2.2 from [.Net Core Project](https://dotnet.microsoft.com/download) for your operating system.

Tu run test application you must open system console, navigate to published project folder and use dotnet run command
```
dotnet run [-c|--configuration] [-f|--framework] [--force] [--launch-profile] [--no-build] [--no-dependencies]
    [--no-launch-profile] [--no-restore] [-p|--project] [--runtime] [-v|--verbosity] [[--] [application arguments]]
dotnet run [-h|--help]
```

A list of CLI command available for dotnet core are available [here](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)

#### Windows example

Copy solution in a folder (ex c:\Program Files\dotnet\projects\cie-nis-dotnetcore-master).
Ecexute dotnet commands for build, publish and run application

```
C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master>dotnet add CIE.NIS.SDK package PCSC
  Writing C:\Users\lucag\AppData\Local\Temp\tmpA082.tmp
info : Aggiunta dell'oggetto PackageReference per il pacchetto 'PCSC' nel progetto 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : Ripristino dei pacchetti per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj in corso...
info :   CACHE https://api.nuget.org/v3-flatcontainer/pcsc/index.json
info : Il pacchetto 'PCSC' è compatibile con tutti i framework specificati nel progetto 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : L'oggetto PackageReference per il pacchetto 'PCSC' versione '4.2.0' è stato aggiornato nel file 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : Esecuzione del commit del ripristino...
info : Generazione del file MSBuild C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\obj\CIE.NIS.SDK.csproj.nuget.g.props.
info : Generazione del file MSBuild C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\obj\CIE.NIS.SDK.csproj.nuget.g.targets.
info : Scrittura del file di asset sul disco. Percorso: C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\obj\project.assets.json
log  : Il ripristino in 228,22 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj.

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master>dotnet add CIE.NIS.SDK package PCSC.Iso7816
  Writing C:\Users\lucag\AppData\Local\Temp\tmpBD03.tmp
info : Aggiunta dell'oggetto PackageReference per il pacchetto 'PCSC.Iso7816' nel progetto 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : Ripristino dei pacchetti per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj in corso...
info :   CACHE https://api.nuget.org/v3-flatcontainer/pcsc.iso7816/index.json
info : Il pacchetto 'PCSC.Iso7816' è compatibile con tutti i framework specificati nel progetto 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : L'oggetto PackageReference per il pacchetto 'PCSC.Iso7816' versione '4.2.0' è stato aggiornato nel file 'C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj'.
info : Esecuzione del commit del ripristino...
info : Scrittura del file di asset sul disco. Percorso: C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\obj\project.assets.json
log  : Il ripristino in 216,61 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj.

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master>dotnet build --configuration Release
Microsoft (R) Build Engine versione 16.2.32702+c4012a063 per .NET Core
Copyright (C) Microsoft Corporation. Tutti i diritti sono riservati.

  Il ripristino in 140,78 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj.
  Il ripristino in 178,81 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\Test.csproj.
  CIE.NIS.SDK -> C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\bin\Release\netcoreapp2.2\CIE.NIS.SDK.dll
  Test -> C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release\netcoreapp2.2\Test.dll

Compilazione completata.
    Avvisi: 0
    Errori: 0

Tempo trascorso 00:00:01.44

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master>dotnet publish Test -c Release
Microsoft (R) Build Engine versione 16.2.32702+c4012a063 per .NET Core
Copyright (C) Microsoft Corporation. Tutti i diritti sono riservati.

  Il ripristino in 38,21 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\Test.csproj.
  Il ripristino in 38,21 ms è stato completato per C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\CIE.NIS.SDK.csproj.
  CIE.NIS.SDK -> C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\CIE.NIS.SDK\bin\Release\netcoreapp2.2\CIE.NIS.SDK.dll
  Test -> C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release\netcoreapp2.2\Test.dll
  Test -> C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release\netcoreapp2.2\publish\

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master>cd test

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test>cd bin

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin>cd Release

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release>cd netcoreapp2.2

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release\netcoreapp2.2>cd publish

C:\Program Files\dotnet\projects\cie-nis-dotnetcore-sdk-master\Test\bin\Release\netcoreapp2.2\publish>dotnet Test.dll
Identificazione del documento tramite il Numero Identificativo per i Servizi

Lettore ACS ACR122U PICC Interface 0 in ascolto
Appoggia la cie sul lettore NFC per iniziare il controllo del nis.
Premi un tasto per terminare l'applicazione
```
