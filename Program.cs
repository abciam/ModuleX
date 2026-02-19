using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ModuleX;

internal partial class Program
{
    internal static Program? Instance { get; private set; }
    private bool _isInitialized;

    internal record AppConfig(string AppName, string AppId);
    private AppConfig Settings { get; set; } = null!;

    #region WinAPI
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    private const int SW_RESTORE = 9;
    #endregion

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Instance = new Program();
        Instance.Settings = Instance.GetAppConfig("ModuleX.settings.json", Assembly.GetExecutingAssembly());

        using Mutex mutex = new(true, Instance.Settings.AppId, out bool isFirst);
        if (!isFirst)
        {
            IntPtr hWnd = FindWindow(null!, Instance.Settings.AppName);
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
            return;
        }

        Instance.Run(args);
    }

    private SynchronizationContext? _context;

    public void Run(string[] args)
    {
        _context = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();

        Init();

        if (!args.Contains("--background"))
        {
            ShowInterface();
        }

        AddToStartup();
        Application.Run();
    }
    public void AddToStartup()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (key == null) return;

        string appName = "MyApp";
        string appPath = $"\"{Application.ExecutablePath}\" --background";
        if (key.GetValue(appName) == null)
        {
            key.SetValue(appName, appPath);
        }
    }

    public async Task Unload()
    {
        if (ModuleManager == null || _mainForm == null) return;

        await ModuleManager.UnloadAllAsync();

        _context?.Post(nw =>
        {
            _mainForm.ButtonsPanel.Controls.Clear();
            _mainForm.ObjectPanel.Controls.Clear();
            _mainForm.ProgressBar.Value = 0;
            _mainForm.BarText.Text = "System Unloaded";
        }, null);
    }
    public async Task Reload()
    {
        if (ModuleManager == null || _mainForm == null) return;

        await ModuleManager.ReloadAllAsync();

        _context?.Post(nw =>
        {
            _mainForm.ButtonsPanel.Controls.Clear();
            _mainForm.ObjectPanel.Controls.Clear();
            _mainForm.ProgressBar.Value = 0;
            _mainForm.BarText.Text = "System Reloaded";


            foreach (var module in ModuleManager.GetAll())
            {
                _mainForm.RegisterModule(module);
            }
        }, null);
    }
    public void Exit() => Application.Exit();

    public async void Init()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        bool success = await Task.Run(Initialize);

        if (success)
        {
            _context?.Post( nw => 
            {
                if (!Environment.GetCommandLineArgs().Contains("--background"))
                    ShowInterface();
            }, null);
        }
        else Application.Exit();
    }
}