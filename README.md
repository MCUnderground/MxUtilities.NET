# MxUtilities.NET

.NET libraries for working with Autodesk 3ds Max data outside of 3ds Max: reading and rewriting `.max` files, managing the external assets they reference, and talking to running 3ds Max instances.

| Package | Contents |
| --- | --- |
| `MxUtilities.FileHandling` | Access to the OLE streams stored inside `.max` files and the render settings they contain. |
| `MxUtilities.AssetManagement` | Reading and rewriting the asset list (textures, xrefs, render outputs, ...) stored in a `.max` file. Depends on `MxUtilities.FileHandling`. |
| `MxUtilities.Scripting` | Discovery of running 3ds Max instances, sending MAXScript to them and monitoring instance start/exit/title changes. |

All packages target `net8.0`. `MxUtilities.Scripting` is Windows-only because it uses Win32 window APIs; the other two are platform independent.

## Installation

```powershell
dotnet add package MxUtilities.FileHandling
dotnet add package MxUtilities.AssetManagement
dotnet add package MxUtilities.Scripting
```

## MxUtilities.FileHandling

### Reading render settings

```csharp
using MxUtilities.FileHandling;
using MxUtilities.FileHandling.Models;

using var maxFile = new MaxFile(@"C:\scenes\scene.max");

RenderData? renderData = maxFile.GetRenderData();
if (renderData is not null)
{
    Console.WriteLine($"{renderData.Width}x{renderData.Height}, aspect {renderData.Aspect}");
    Console.WriteLine($"Renderer: {renderData.Renderer}");
    Console.WriteLine($"Output: {renderData.RenderOutput} (gamma {renderData.RenderOutputGamma})");
    Console.WriteLine($"Cameras: {string.Join(", ", renderData.Cameras)}");
}
```

`GetRenderData()` returns `null` when the file contains no render data.

### Working with raw streams

`.max` files are OLE compound documents. Stream names are listed in `MaxFileStreams` (`Scene`, `Config`, `ClassData`, `FileAssetMetaData3`, ...).

```csharp
using MxUtilities.FileHandling;
using MxUtilities.FileHandling.Constants;
using MxUtilities.FileHandling.Models;

using var maxFile = new MaxFile(@"C:\scenes\scene.max");

if (maxFile.TryGetStream(MaxFileStreams.Config, out MaxStream config))
{
    byte[] data = config.GetBuffer();
    Console.WriteLine($"{config.StreamName}: {data.Length} bytes");
}
```

Opening a file with `MaxFileAccess.Write` allows replacing a stream:

```csharp
using MxUtilities.FileHandling;
using MxUtilities.FileHandling.Constants;
using MxUtilities.FileHandling.Enums;
using MxUtilities.FileHandling.Models;

using var maxFile = new MaxFile(@"C:\scenes\scene.max", MaxFileAccess.Write);

await maxFile.SetStream(
    MaxFileStreams.Config,
    new MaxStream(newConfigBytes, MaxFileStreams.Config));
```

`SetStream` deletes the existing stream and writes the new one. It throws when the `MaxFile` instance was opened for read-only access. Always work on a copy while developing; there is no undo.

## MxUtilities.AssetManagement

`AssetManager` reads the asset metadata stream of a `.max` file, lets you change the stored paths, and writes the result back. This is intended for offline repathing, for example when a project is moved to another drive.

```csharp
using MxUtilities.AssetManagement;
using MxUtilities.AssetManagement.Models;

const string origin = @"D:\Projects\Scene\";
const string target = @"E:\Archive\Scene\";

if (AssetManager.TryReadAssets(@"C:\scenes\scene.max", out var assets))
{
    foreach (Asset asset in assets.Assets)
    {
        if (asset.FileName.StartsWith(origin, StringComparison.OrdinalIgnoreCase))
        {
            asset.FileName = asset.FileName.Replace(origin, target, StringComparison.OrdinalIgnoreCase);
        }

        if (asset.ResolvedFile is not null
            && asset.ResolvedFile.StartsWith(origin, StringComparison.OrdinalIgnoreCase))
        {
            asset.ResolvedFile = asset.ResolvedFile.Replace(origin, target, StringComparison.OrdinalIgnoreCase);
        }
    }

    await AssetManager.WriteAssets(@"C:\scenes\scene.max", assets);
}
```

- `TryReadAssets` returns `false` when the file contains no asset data or cannot be read.
- `ReadAssets` does the same but throws `InvalidOperationException` on errors.
- `Asset.FileName` is the path stored in the scene, `Asset.ResolvedFile` is the path the asset was resolved to when the scene was last saved. `ResolvedFile` is always `null` for the older stream layout.
- `AssetCollection.IsOldStream` indicates that the older `FileAssetMetaData2` layout was used; the collection is written back in the same layout. `ReachedEnd` is `false` when parsing stopped early.

## MxUtilities.Scripting

### Finding running instances

```csharp
using MxUtilities.Scripting;
using MxUtilities.Scripting.Models;

IReadOnlyList<MaxInstance> instances = InstanceManager.GetActiveInstances();

foreach (MaxInstance instance in instances)
{
    Console.WriteLine($"PID {instance.ProcessId}, 3ds Max {instance.Version}: {instance.Title}");
}
```

Only processes named `3dsmax` with a main window title matching `... - Autodesk 3ds Max <year>` are returned.

### Sending MAXScript

```csharp
var manager = new InstanceManager();

if (instances.Count > 0)
{
    bool sent = manager.SendMessage(instances[0], "messageBox \"Hello from .NET\"");
}
```

`SendMessage` types the code into the MAXScript Listener and presses Enter, so the Listener must be open in the target instance. It returns `false` when the Listener window could not be found. The listener window handle is cached per instance after the first successful call.

### Monitoring instances

```csharp
manager.InstanceStarted += instance => Console.WriteLine($"Started: {instance.Title}");
manager.InstanceExited += instance => Console.WriteLine($"Exited: {instance.Title}");
manager.InstanceTitleChanged += (instance, title) => Console.WriteLine($"Title: {title}");

manager.StartMonitoring();

// ...

manager.StopMonitoring();
```

Monitoring polls for `3dsmax` processes once per second while it is running.

`MaxInstance.ChangeTitle` changes the title of the 3ds Max main window.

## Building and packing

```powershell
dotnet build MxUtilities.NET.sln -c Release
dotnet pack MxUtilities.NET.sln -c Release -o artifacts
```

Package metadata (version, authors, license, repository URL) is defined in `Directory.Build.props`. To publish a release:

```powershell
dotnet nuget push artifacts/MxUtilities.FileHandling.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
dotnet nuget push artifacts/MxUtilities.AssetManagement.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
dotnet nuget push artifacts/MxUtilities.Scripting.1.0.0.nupkg --api-key <key> --source https://api.nuget.org/v3/index.json
```

## License

MIT, see [LICENSE](LICENSE).
