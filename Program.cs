using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace ScriptHookAutoUpdater
{
    internal class Program
    {
        static string currentDirectory;
        static string extractedDirectory => $"{currentDirectory}/Extracted";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("ScriptHookAutoUpdater");
            currentDirectory = Environment.CurrentDirectory;
            FileVersionInfo gtaVersionInfo = FileVersionInfo.GetVersionInfo($"{currentDirectory}/GTA5.exe");
            if (gtaVersionInfo == null)
            {
                Console.WriteLine("Couldn't get GTA5 version. Aborting.");
                return;
            }
            string gtaVersion = gtaVersionInfo.FileVersion;
            Console.WriteLine($"Detected GTA version: {gtaVersion}");
            bool legacyVersion = gtaVersionInfo.FileBuildPart == 3337;
            if (legacyVersion)
            {
                Console.WriteLine("Game version = 3337 detected. Forcing legacy version.");
            }
            bool changeHost = gtaVersionInfo.FileBuildPart >= 3442;
            if (changeHost)
            {
                Console.WriteLine("Game version >= 3442 detected. Changing download domain to ntscorp.ru.");
            }
            Console.WriteLine("Deleting existing ScriptHook download folder");
            DeleteExistingCache();
            Console.WriteLine("Downloading ScriptHook");
            if (await DownloadScriptHook(gtaVersion, legacyVersion, changeHost))
            {
                Console.WriteLine("Unzipping ScriptHook");
                UnzipScriptHook();
                Console.WriteLine("Deleting existing ScriptHook files");
                DeleteExistingScriptHookFiles();
                Console.WriteLine("Moving ScriptHook files to game folder");
                MoveScriptHookToGameFolder();
                Console.WriteLine("Cleaning up ScriptHook download folder");
                DeleteExistingCache();
                Console.WriteLine("Deleting downloaded zip");
                CleanupZip();
                Console.WriteLine("Successfully installed ScriptHookV");
            }
            Console.WriteLine("Press any key to close...");
            Console.ReadLine();
        }

        static async Task<bool> DownloadScriptHook(string gtaVersion, bool legacy, bool changeHost)
        {
            using (WebClient client = new())
            {
                if (!changeHost)
                {
                    client.Headers.Add(HttpRequestHeader.Referer, "http://www.dev-c.com/gtav/scripthookv/");
                }
                try
                {
                    string uri = changeHost ? $"https://ntscorp.ru/dev-c/ScriptHookV_{gtaVersion}" : $"http://www.dev-c.com/files/ScriptHookV_{gtaVersion}";
                    if (legacy) uri += "_legacy";
                    await client.DownloadFileTaskAsync(new Uri(uri + ".zip"), "ScriptHook.zip");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when downloading ScriptHook!");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    File.Delete("ScriptHook.zip");
                    return false;
                }
            }
        }

        static void CleanupZip()
        {
            File.Delete($"{currentDirectory}/ScriptHook.zip");
        }

        static void UnzipScriptHook()
        {
            ZipFile.ExtractToDirectory("ScriptHook.zip", extractedDirectory);
        }

        static void DeleteExistingCache()
        {
            if (Directory.Exists(extractedDirectory))
            {
                Directory.Delete(extractedDirectory, true);
            }
        }

        static void DeleteExistingScriptHookFiles()
        {
            File.Delete($"{currentDirectory}/dinput8.dll");
            File.Delete($"{currentDirectory}/ScriptHookV.dll");
        }

        static void MoveScriptHookFile(string fileName)
        {
            File.Move($"{extractedDirectory}/bin/{fileName}", $"{currentDirectory}/{fileName}");
        }

        static void MoveScriptHookToGameFolder()
        {
            MoveScriptHookFile("dinput8.dll");
            MoveScriptHookFile("ScriptHookV.dll");
        }
    }
}
