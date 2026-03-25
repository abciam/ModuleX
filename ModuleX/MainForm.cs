using Common.Interfaces;
using Common.Models;
using ModuleX.Core;
using System.Reactive.Linq;

namespace ModuleX;

public partial class MainForm : Form
{
    private readonly Bus _bus;
    private IDisposable? _activeModuleSubscription;

    internal MainForm(Bus bus)
    {
        _bus = bus;
        InitializeComponent();
        BarText.BringToFront(); 
        
        ContextMenuStrip settingsMenu = new();

        var reloadItem = new ToolStripMenuItem("Reload");
        reloadItem.Click += async (s, e) =>
        {
            await Program.Instance!.Reload();
        };
        var unloadItem = new ToolStripMenuItem("Unload");
        unloadItem.Click += async (s, e) =>
        {
            await Program.Instance!.Unload();
        };

        var modulesItem = new ToolStripMenuItem("Open Modules Folder");
        modulesItem.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", Program.ModulesPath);
        var logsItem = new ToolStripMenuItem("Open Logs Folder");
        logsItem.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", Program.LogsPath);

        settingsMenu.Items.Add(unloadItem);
        settingsMenu.Items.Add(reloadItem);
        settingsMenu.Items.Add(new ToolStripSeparator());
        settingsMenu.Items.Add(modulesItem);
        settingsMenu.Items.Add(logsItem);

        SettingsButton.MouseEnter += (s, e) =>
        {
            settingsMenu.Show(SettingsButton, new Point(0, SettingsButton.Height));
        };
    }

    public void RegisterModuleButton(string dllPath, string moduleName, AsyncModule? loadedInstance)
    {
        var existingBtn = ButtonsPanel.Controls.OfType<CustomButton>()
            .FirstOrDefault(b => b.ModulePath == dllPath);

        if (existingBtn != null)
        {
            existingBtn.UpdateStatus(loadedInstance != null, loadedInstance);
            return;
        }

        var customBtn = new CustomButton(dllPath, moduleName, loadedInstance);
        customBtn.Width = ButtonsPanel.Width - 10;


        customBtn.OnSelectRequested += (s, mod) => {
            if (mod != null) SwitchToModule(mod);
        };

        customBtn.OnLoadRequested += async (s, path) => {
            var newMod = await Program.Instance!.ModuleManager.LoadSingleModuleAsync(path);
            if (newMod != null) customBtn.UpdateStatus(true, newMod);
        };

        customBtn.OnUnloadRequested += async (s, mod) => {
            await Program.Instance!.ModuleManager.UnloadSingleModuleAsync(dllPath);
            customBtn.UpdateStatus(false, null);
            ObjectPanel.Controls.Clear();
        };

        ButtonsPanel.Controls.Add(customBtn);
    }

    private void SwitchToModule(AsyncModule module)
    {
        _activeModuleSubscription?.Dispose();

        while (ObjectPanel.Controls.Count > 0)
        {
            var ctrl = ObjectPanel.Controls[0];
            ObjectPanel.Controls.Remove(ctrl);
            ctrl.Dispose();
        }

        _activeModuleSubscription = _bus.GetProgressStream(module.Identity)
            .ObserveOn(SynchronizationContext.Current!)
            .Subscribe(data =>
            {
                ProgressBar.Value = Math.Clamp((int)(data.Value * 2.5), 0, 250);
                BarText.Text = data.Text;
            });

        var builder = new ModuleUiBuilder(ObjectPanel);
        module.Render(builder);
    }
}