# GopsDailySheet

A small Windows app that puts a handful of web pages in one borderless, full-screen
window for use on a tablet during gliding operations. It is a WinForms shell
(.NET Framework 4.7.2) hosting [Edge WebView2](https://developer.microsoft.com/microsoft-edge/webview2/)
browsers, one per tab.

## What the app does

- Shows one browser tab per configured page (daily sheet, live trackers, and so on).
- Only one instance runs at a time; a second launch pops up a warning and exits.
- A vertical toolbar on the right edge:
  - **REFRESH** reloads the current tab.
  - **QUIT** asks for confirmation, then exits.
  - **PANIC → Magic Fix?** restarts the app (useful when a page gets wedged).
  - The running version is displayed at the bottom.
- No window chrome, always maximised, so it is hard to disturb by accident on a touch screen.

Source layout:

| Path | Purpose |
| --- | --- |
| `app/Program.cs` | Entry point, single-instance check, restart, error handling |
| `app/Form1.cs` | Builds the tabs from config, toolbar behaviour |
| `app/Config/` | Config classes for the `tabsConfigs` section |
| `app/App.config` | Default tabs and font size (template, see below) |

## Configuration

Everything configurable lives in **`GopsDailySheet.exe.config`**, the file next to
`GopsDailySheet.exe`. Config is read once at startup, so restart the app after editing.

A release ships that file as **`GopsDailySheet.exe.config.example`**, never as
`GopsDailySheet.exe.config`. That is what makes updating safe: copying a new folder over an
existing install has no `GopsDailySheet.exe.config` to overwrite, so the settings on the
tablet are left alone. On its first start the app finds no config file and creates one from
the example next to it, and from then on it only ever reads the file it has.

```xml
<configSections>
  <section name="tabsConfigs" type="GopsDailySheet.Config.TabsConfigSection, GopsDailySheet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
</configSections>
<tabsConfigs>
  <tabs>
    <add name="WGCdailySheet" caption="GOPS Daily Sheet" url="http://gops.wwgc.co.nz/DailySheet?org=1&amp;key=" zoomFactor="1.1" />
    <add name="WGCtracking" caption="GOPS Tracking" url="http://www.glidingops.com/wgc" unloadOnLostFocus="true" />
    <add name="gNetTracking" caption="Gliding.net Tracking" url="https://gliding.net.nz/tracking" unloadOnLostFocus="true" />
  </tabs>
</tabsConfigs>
<appSettings>
  <add key="fontSize" value="20" />
</appSettings>
```

Each `<add>` inside `<tabs>` becomes one tab:

| Attribute | Required | Meaning |
| --- | --- | --- |
| `name` | yes | Unique key for the tab. Used internally only |
| `caption` | yes | Text shown on the tab |
| `url` | yes | Page to load |
| `zoomFactor` | no | Zoom level, defaults to `1` |
| `unloadOnLostFocus` | no | `true` disposes the browser when you switch away and reloads it when you come back. Use it for heavy live-tracking pages to keep memory down |

`appSettings`:

| Key | Meaning |
| --- | --- |
| `fontSize` | Font size in points for the tabs and toolbar. Defaults to `18` if the key is missing |

There is no machine-wide or per-user override: the copy of the config inside the app folder
is the one that is used, which is what makes the folder portable. `app/App.config` in the repo
is the template the build ships as the `.example` file.

## Deployment

The app ships as a plain folder. There is no installer and the app never downloads or
updates itself — an update is just a newer folder.

### Build it

```powershell
.\build-portable.ps1                     # builds artifacts\GopsDailySheet + zip, version from AssemblyInfo.cs
.\build-portable.ps1 -Version 2.0.1.0    # stamps a version for this build only
```

The script finds MSBuild (PATH or via `vswhere`), restores NuGet packages, rebuilds
`Release` into a clean `artifacts\GopsDailySheet\` folder, writes a `README.txt` for the
person installing it, and zips the folder contents to `artifacts\GopsDailySheet-portable.zip`
(the exe sits at the root of the zip). `-Version` is applied only for the duration of the
build, so `AssemblyInfo.cs` is left untouched.

Building needs Visual Studio 2022 or the Build Tools with the .NET desktop workload and the
4.7.2 targeting pack. Running needs Windows 10 or later (which already includes
.NET Framework 4.7.2+) and the WebView2 Evergreen runtime, which is present on any normal
Windows install; if it is missing the app says so on startup with a link to install it.

### Put it on the tablet

Copy `GopsDailySheet-portable.zip` into the shared Google Drive folder. On the tablet, copy
the zip out of Drive, unzip it somewhere permanent such as `C:\Apps\GopsDailySheet`, and
start `GopsDailySheet.exe`. Copying the folder is the whole installation; on that first start
the app writes `GopsDailySheet.exe.config` from the shipped `.example`, which is the file to
edit from then on.

To update: close the app, then copy the files of the newer folder **over** the existing one.
Do not delete the existing folder first — `GopsDailySheet.exe.config` is the tablet's own and
is never replaced, so an overwrite keeps the settings while replacing everything else.

Two consequences of this layout are deliberate:

- **Only the config is written inside the app folder**, once, on the very first start.
  Browsing data (cookies, logins, cache) is kept per Windows user in
  `%LOCALAPPDATA%\GopsDailySheet`, so it is not tied to the folder at all.
- **`WebView2Loader.dll` ships inside `runtimes\`** for x86, x64 and arm64, so the single
  Any CPU build runs on any of them.

Keep `GopsDailySheet.exe` inside the folder — it needs the DLLs next to it, and it looks for
`GopsDailySheet.exe.config` (or the `.example` to create it from) in the same folder.

### Releases and versioning

`release.ps1` cuts a release. It resolves the version, records it in the app, tags that commit
and builds the zip:

```powershell
.\release.ps1                     # next version, bumped from the highest of AssemblyInfo and the latest tag
.\release.ps1 -Tag v2.1.0.0       # a specific version
```

- **Version format** is four numbers, tagged as `v<major>.<minor>.<patch>.<build>`, e.g. `v2.0.0.1`.
  With no `-Tag`, the next version is the higher of the version in `app/Properties/AssemblyInfo.cs`
  and the latest `v*.*.*.*` tag, with the last number incremented.
- **The version reaches the app** because the script writes it to `AssemblyInfo.cs` and commits
  that as its own commit (`bump version to 2.0.0.1`). The tag points at that commit, so the app's
  displayed version always matches the tag it was built from.
- **Tags are never moved.** If the tag already exists on another commit, or its commit carries a
  different version, the script stops. If it already exists at `HEAD` with the same version, it is
  reused — that is the case when CI runs on a tag you pushed yourself.
- Publishing is left to you, so the tag and its commit go out together:

  ```powershell
  git push origin master v2.0.0.1
  ```

`build-portable.ps1` on its own just builds whatever is in `AssemblyInfo.cs` and touches no git
state — useful for a quick local test build.

### GitHub releases

`.github/workflows/release.yml` runs the same script and attaches the zip to the GitHub release
for the tag:

- Push a tag (`git push origin master v2.0.0.1`) to trigger it.
- Or run **Release portable app** manually from the Actions tab; the optional `tag` input selects
  the version, and leaving it empty bumps to the next one. In that case the workflow commits the
  version bump, creates the tag and pushes it.

If a release already exists for the tag, the workflow fails rather than replacing it.
Use the release zip (or one you built yourself) as the file you drop into the Drive folder; the
workflow is a convenience, not a dependency.