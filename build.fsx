#r "./tools/Fake/tools/FakeLib.dll"

open Fake
let buildVersion = Fake.AppVeyor.AppVeyorEnvironment.BuildVersion

let accessKey = getBuildParamOrDefault "PRIVATEKEY" "NOKEY"
let sourceDir = __SOURCE_DIRECTORY__
let buildDir = sourceDir @@ "bin"

printfn "API KEY is %s" accessKey

Target "Clean" (fun _ -> CleanDir buildDir)

Target "Build" (fun _ -> 
  !! ( "./src/Myxomatosis_*/*.csproj")
  |> Seq.iter(
    fun proj -> 
      let fileI = fileInfo proj
      let fileName (fi : System.IO.FileInfo) = System.IO.Path.GetFileNameWithoutExtension fi.Name
      let name  = (fileName fileI).Replace("Myxomatosis_", "").ToLower()

      build 
        (fun defaults -> 
          { defaults with
              Verbosity = Some Quiet
              Targets = ["Build"]
              Properties =
                  [
                      "Optimize", "True"
                      "DebugSymbols", "True"
                      "Configuration", "Release"
                      "OutputPath", buildDir @@ name
                  ]
          }) proj))


Target "Pack" (fun _ -> 
    Fake.NuGetHelper.NuGetPack (
      fun p -> 
        { p with
            Authors = [ "Travel Republic "]
            Project = "Myxomatosis"
            Description = "Rabbit MQ Client leverating reactive extensions"
            ToolPath = sourceDir @@ ".nuget/nuget.exe"
            WorkingDir = buildDir
            OutputPath = buildDir
            Version = buildVersion
            DependenciesByFramework = 
              [ 
                { FrameworkVersion = "net45"
                  Dependencies = [ "Newtonsoft.Json",       "[7.0.1, )" 
                                   "RabbitMQ.Client",       "[3.2.0, )" 
                                   "System.Reactive.Linq",  "[3.0.0, )"  ]
                }
              ]
            Files = [ "**/Myxomatosis.*", None, None ]
        }
      ) (sourceDir @@ ".nuget/template")
    ())

Target "Publish" (fun _ -> 
  !! (buildDir @@ "**/*.nupkg")
  |> Seq.iter(fun a -> 
      let directory = a |> directory |> directoryInfo
      let workingDir = directory.FullName
      traceImportant (sprintf "Nuget package file: %s" a)
      traceImportant (sprintf "push \"%s\" %s" a accessKey)
      let exitCode = 
        ExecProcess(fun info -> 
          info.FileName <-  sourceDir @@ ".nuget/nuget.exe"
          info.Arguments <- sprintf "push \"%s\" %s -Source https://www.nuget.org/api/v2/package" a accessKey) (System.TimeSpan.FromSeconds 30.0)
      if exitCode = 0 then ()
      else failwith "Error during Nuget push"
    )
  )

"Clean"
  ==> "Build"
  ==> "Pack"
  ==> "Publish"

RunTargetOrDefault "Publish"