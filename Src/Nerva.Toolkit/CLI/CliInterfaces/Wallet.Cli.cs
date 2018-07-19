using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AngryWasp.Logger;
using Nerva.Toolkit.Config;
using Nerva.Toolkit.Helpers;
using Newtonsoft.Json;

namespace Nerva.Toolkit.CLI
{
    public partial class WalletInterface : CliInterface
    {
        [JsonObject]
        private class RestoreJson
        {
            [JsonProperty("version")]
            public int Version => 1;

            [JsonProperty("filename")]
            public string FileName { get; set; } = string.Empty;

            [JsonProperty("password")]
            public string Password { get; set; } = string.Empty;

            [JsonProperty("scan_from_height")]
            public int ScanHeight => 0;

            [JsonProperty("create_address_file")]
            public int CreateAddressFile => 1;

            [JsonProperty("seed_language")]
            public string SeedLanguage { get; set; } = "English";
        }

        [JsonObject]
        private class RestoreJsonKeys : RestoreJson
        {
            [JsonProperty("viewkey")]
            public string ViewKey { get; set; } = string.Empty;

            [JsonProperty("spendkey")]
            public string SpendKey { get; set; } = string.Empty;
        }

        [JsonObject]
        private class RestoreJsonSeed : RestoreJson
        {
            [JsonProperty("seed")]
            public string Seed { get; set; } = string.Empty;

            [JsonProperty("seed_passphrase")]
            public string Passphrase { get; set; } = string.Empty;
        }

        public WalletInterface() : base(Configuration.Instance.Wallet.Rpc) { }

        public void RestoreWalletFromKeys(string walletName, string viewKey, string spendKey, string pass, string language)
        {
            string walletPath = Path.Combine(Configuration.Instance.Wallet.WalletDir, walletName);

            CreateJson(walletPath, new RestoreJsonKeys
            {
                FileName = walletPath,
                Password = pass,
                ViewKey = viewKey,
                SpendKey = spendKey,
                SeedLanguage = language
            });

            RestoreWalletFromJson(walletPath);
        }

        public void RestoreWalletFromSeed(string walletName, string seed, string pass, string language)
        {
            string walletPath = Path.Combine(Configuration.Instance.Wallet.WalletDir, walletName);

            CreateJson(walletPath, new RestoreJsonSeed
            {
                FileName = walletPath,
                Password = pass,
                Seed = seed,
                SeedLanguage = language
            });

            RestoreWalletFromJson(walletPath);
        }
        
        private void CreateJson(string walletPath, object data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            var serializer = JsonSerializer.Create(settings);

            using (var sw = new StreamWriter(File.OpenWrite($"{walletPath}.json")))
                using (var jsonTextWriter = new JsonTextWriter(sw))
                    serializer.Serialize(jsonTextWriter, data);
        }

        private void RestoreWalletFromJson(string walletPath)
        {
            string daemon = $"127.0.0.1:{Configuration.Instance.Daemon.Rpc.Port}";
            string exe = FileNames.GetCliExePath(FileNames.CLI_WALLET);
            string args = $"--daemon-address {daemon} --generate-from-json {walletPath}.json";
            if (Configuration.Instance.Testnet)
                args += " --testnet";

            Log.Instance.Write("Restoring wallet with command {0} {1}", exe, args);

            Process proc = Process.Start(new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            });

            proc.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    Log.Instance.Write(e.Data);

                    if (e.Data.Contains("Refresh done"))
                    {
                        Log.Instance.Write("Wallet import complete");
                        proc.StandardInput.WriteLine("exit");
                    }
                }
            };

            proc.BeginOutputReadLine();
            proc.WaitForExit();

            int exitCode = proc.ExitCode;

            Log.Instance.Write("Import wallet exited with code: {0}", exitCode);
        }
    }
}