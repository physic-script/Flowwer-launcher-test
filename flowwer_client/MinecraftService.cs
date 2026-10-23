
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using CmlLib.Core.ProcessBuilder;

namespace flowwer_client;

// Everything related to Minecraft: Microsoft account connection and game launch
public class MinecraftService
{
    // ===== Easy settings =====
    public string MinecraftVersion = "1.21.1";
    public const bool UseFabric = true;
    public const int RamMb = 4096;

    // Separate game directory
    private static string? _gameDir;

    public static string GameDir
    {
        get
        {
            if (_gameDir == null)
            {
                string configPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FlowwerClient",
                    "config.json");

                if (File.Exists(configPath))
                {
                    try
                    {
                        string json = File.ReadAllText(configPath);

                        using var doc = JsonDocument.Parse(json);

                        if (doc.RootElement.TryGetProperty("GameDir", out var gd))
                        {
                            _gameDir = gd.GetString();
                        }
                    }
                    catch
                    {
                    }
                }

                if (_gameDir == null)
                {
                    _gameDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ".flowwer");
                }
            }

            return _gameDir;
        }
        set
        {
            _gameDir = value;

            try
            {
                string configDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FlowwerClient");

                Directory.CreateDirectory(configDir);

                string configPath = Path.Combine(configDir, "config.json");

                string json = JsonSerializer.Serialize(new
                {
                    GameDir = value
                });

                File.WriteAllText(configPath, json);
            }
            catch
            {
            }
        }
    }

    // Flowwer API mod
    public const string ApiJarUrl =
        "https://raw.githubusercontent.com/physic-script/Flowwer-Needings/main/Flowwer_API.jar";

    private string? _apiJarPath;

    public string ApiJarPath
    {
        get => _apiJarPath ?? Path.Combine(GameDir, "mods", "Flowwer_API.jar");
        set => _apiJarPath = value;
    }

    // Flowwer texture pack
    public const string TexturePackUrl =
        "https://raw.githubusercontent.com/physic-script/Flowwer-Needings/main/Flowwer_client_texture.zip";

    public string TexturePackPath =>
        Path.Combine(GameDir, "resourcepacks", "Flowwer_client_texture.zip");

    private readonly JELoginHandler loginHandler =
        JELoginHandlerBuilder.BuildDefault();

    private static readonly HttpClient http = new();

    public MSession? Session { get; private set; }

    public bool IsLoggedIn => Session != null;

    // Warning raised when a required file is missing
    public event Action<string>? Warning;

    // Automatically reconnect to a saved Microsoft account
    public async Task<bool> TryAutoLoginAsync()
    {
        try
        {
            Session = await loginHandler.AuthenticateSilently();
            return true;
        }
        catch
        {
            Session = null;
            return false;
        }
    }

    // Open Microsoft login window
    public async Task LoginAsync()
    {
        Session = await loginHandler.AuthenticateInteractively();
    }

    public async Task LogoutAsync()
    {
        try
        {
            await loginHandler.Signout();
        }
        catch
        {
        }

        Session = null;
    }

    // Ensure the Flowwer API jar exists
    public async Task EnsureApiJarAsync()
    {
        if (File.Exists(ApiJarPath))
            return;

        Warning?.Invoke("Flowwer_API.jar is missing. Downloading from GitHub...");

        Directory.CreateDirectory(Path.Combine(GameDir, "mods"));

        using var request = new HttpRequestMessage(HttpMethod.Get, ApiJarUrl);
        request.Headers.UserAgent.ParseAdd("FlowwerClient/1.0");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        byte[] data = await response.Content.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(ApiJarPath, data);
    }

    // Ensure the texture pack exists
    public async Task EnsureTexturePackAsync()
    {
        if (File.Exists(TexturePackPath))
            return;

        Warning?.Invoke("Flowwer_client_texture.zip is missing. Downloading from GitHub...");

        Directory.CreateDirectory(Path.Combine(GameDir, "resourcepacks"));

        using var request = new HttpRequestMessage(HttpMethod.Get, TexturePackUrl);
        request.Headers.UserAgent.ParseAdd("FlowwerClient/1.0");

        using var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        byte[] data = await response.Content.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(TexturePackPath, data);
    }

    // Force the texture pack in options.txt
    public void ForceTexturePack()
    {
        try
        {
            string optionsPath = Path.Combine(GameDir, "options.txt");

            var lines = File.Exists(optionsPath)
                ? File.ReadAllLines(optionsPath).ToList()
                : new System.Collections.Generic.List<string>();

            string forced = $"resourcePacks:[\"{TexturePackPath}\"]";
            bool found = false;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith("resourcePacks:"))
                {
                    lines[i] = forced;
                    found = true;
                    break;
                }
            }

            if (!found)
                lines.Add(forced);

            File.WriteAllLines(optionsPath, lines);
        }
        catch
        {
        }
    }

    // Install the appropriate Fabric API for the selected Minecraft version
    private async Task EnsureFabricApiAsync(IProgress<string> progress)
    {
        try
        {
            string modsDir = Path.Combine(GameDir, "mods");
            Directory.CreateDirectory(modsDir);

            var fabricApiFiles = Directory
                .GetFiles(modsDir, "fabric-api-*.jar")
                .ToList();

            progress.Report("Fabric API...");

            string apiMetaUrl =
                $"https://api.modrinth.com/v2/project/P7dR8mSH/version?game_versions=[\"{MinecraftVersion}\"]&loaders=[\"fabric\"]&featured=true";

            using var request = new HttpRequestMessage(HttpMethod.Get, apiMetaUrl);
            request.Headers.UserAgent.ParseAdd("FlowwerClient/1.0");

            using var response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return;

            string json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.GetArrayLength() == 0)
                return;

            string expectedFileName = "";
            string downloadUrl = "";

            foreach (var version in doc.RootElement.EnumerateArray())
            {
                if (!version.TryGetProperty("files", out var files))
                    continue;

                foreach (var file in files.EnumerateArray())
                {
                    if (file.GetProperty("primary").GetBoolean() ||
                        files.GetArrayLength() == 1)
                    {
                        expectedFileName =
                            file.GetProperty("filename").GetString() ?? "";

                        downloadUrl =
                            file.GetProperty("url").GetString() ?? "";

                        break;
                    }
                }

                if (!string.IsNullOrEmpty(downloadUrl))
                    break;
            }

            if (string.IsNullOrEmpty(downloadUrl) ||
                string.IsNullOrEmpty(expectedFileName))
                return;

            bool hasCorrect = fabricApiFiles.Any(
                file => Path.GetFileName(file) == expectedFileName);

            if (hasCorrect)
                return;

            foreach (var oldFile in fabricApiFiles)
            {
                try
                {
                    File.Delete(oldFile);
                }
                catch
                {
                }
            }

            progress.Report("Installing Fabric API...");

            using var downloadRequest =
                new HttpRequestMessage(HttpMethod.Get, downloadUrl);

            downloadRequest.Headers.UserAgent.ParseAdd("FlowwerClient/1.0");

            using var downloadResponse = await http.SendAsync(downloadRequest);

            downloadResponse.EnsureSuccessStatusCode();

            byte[] data = await downloadResponse.Content.ReadAsByteArrayAsync();

            string targetPath = Path.Combine(modsDir, expectedFileName);

            await File.WriteAllBytesAsync(targetPath, data);
        }
        catch (Exception ex)
        {
            Warning?.Invoke($"Fabric API installation failed: {ex.Message}");
        }
    }

    // Launch Minecraft
    public async Task<Process> LaunchAsync(IProgress<string> progress)
    {
        if (Session == null)
        {
            throw new InvalidOperationException(
                "Please sign in to your Microsoft account before launching Minecraft.");
        }

        Directory.CreateDirectory(GameDir);

        var path = new MinecraftPath(GameDir);

        string versionId = MinecraftVersion;

        if (UseFabric)
        {
            progress.Report("Fabric...");

            versionId = await InstallFabricAsync(path);

            await EnsureFabricApiAsync(progress);
        }

        await EnsureApiJarAsync();
        await EnsureTexturePackAsync();

        ForceTexturePack();

        var launcher = new MinecraftLauncher(path);

        launcher.ByteProgressChanged += (_, e) =>
        {
            if (e.TotalBytes > 0)
            {
                progress.Report($"{(int)(e.ToRatio() * 100)} %");
            }
        };

        progress.Report("Preparing...");

        var process = await launcher.InstallAndBuildProcessAsync(
            versionId,
            new MLaunchOption
            {
                Session = Session,
                MaximumRamMb = RamMb,
                GameLauncherName = "Flowwer Client"
            });

        progress.Report("Launching...");

        process.Start();

        return process;
    }

    // Install Fabric profile for the selected Minecraft version
    private async Task<string> InstallFabricAsync(MinecraftPath path)
    {
        string meta =
            $"https://meta.fabricmc.net/v2/versions/loader/{MinecraftVersion}";

        using var loaders =
            JsonDocument.Parse(await http.GetStringAsync(meta));

        string? loaderVersion = null;

        foreach (var item in loaders.RootElement.EnumerateArray())
        {
            var loader = item.GetProperty("loader");

            if (loader.GetProperty("stable").GetBoolean())
            {
                loaderVersion = loader.GetProperty("version").GetString();
                break;
            }
        }

        if (loaderVersion == null)
        {
            throw new Exception(
                $"Fabric does not exist yet for Minecraft {MinecraftVersion}.");
        }

        string versionId =
            $"fabric-loader-{loaderVersion}-{MinecraftVersion}";

        string jsonPath = path.GetVersionJsonPath(versionId);

        if (!File.Exists(jsonPath))
        {
            string profile = await http.GetStringAsync(
                $"{meta}/{loaderVersion}/profile/json");

            string? directory = Path.GetDirectoryName(jsonPath);

            if (directory != null)
                Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(jsonPath, profile);
        }

        return versionId;
    }
}
