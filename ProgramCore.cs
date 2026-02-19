using Common.Enums;
using Common.Interfaces;
using ModuleX.Contexts;
using ModuleX.Core;
using System.Reflection;
using System.Text.Json;

namespace ModuleX;

internal partial class Program
{
    internal MainForm? _mainForm;
    public static string ModulesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
    public static string LogsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    public static readonly List<string> FoldersNames = ["Libraries", "Modules", "Logs"];

    internal ModuleManager ModuleManager = null!;
    internal IModuleContext moduleContext = null!;
    internal Bus _bus = null!;
    private string _path;

    internal AppConfig GetAppConfig(string path, Assembly assembly)
    {
        try
        {
            using Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream == null) return new AppConfig("DefaultApp", "Default_ID_123");
            return JsonSerializer.Deserialize<AppConfig>(stream) ?? new AppConfig("DefaultApp", "Default_ID_123");
        }
        catch { return new AppConfig("DefaultApp", "Default_ID_123"); }
    }

    private async Task<bool> Initialize()
    {
        if (!CheckFolders()) return false;

        try
        {
            _bus = new Bus();
            moduleContext = new ModuleContext(_bus);

            string filename = $"Log_[{DateTime.Now:HH-mm-ss_dd-MM-yyyy}].txt";
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", filename);
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.Create(_path).Dispose();


            _bus.BufferedLogs.Subscribe(logs => {
                var lines = logs.Select(l => $"[{l.Time:HH:mm:ss}] [{l.Level}] {l.Msg}");
                File.AppendAllLines(_path, lines);
            });

            ModuleManager = new ModuleManager(moduleContext, _bus);
            await ModuleManager.LoadAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    private bool CheckFolders()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var folder in FoldersNames)
            {
                Directory.CreateDirectory(Path.Combine(baseDir, folder));
            }
            return true;
        }
        catch { return false; }
    }

    public void ShowInterface()
    {
        if (ModuleManager == null || _bus == null) return;

        if (_mainForm == null || _mainForm.IsDisposed)
        {
            _mainForm = new ModuleX.MainForm(_bus);
            _mainForm.Text = Settings?.AppName ?? "ModuleX";

            foreach (var module in ModuleManager.GetAll())
            {
                _mainForm.RegisterModule(module);
            }

            _mainForm.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    _mainForm.Hide();
                }
            };
        }
        _mainForm.Show();
        _mainForm.Activate();
    }
}