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
        
        ContextMenuStrip settingsMenu = new ContextMenuStrip();

        var reloadItem = new ToolStripMenuItem("Hot Reload");
        reloadItem.Click += async (s, e) =>
        {
            await Program.Instance.HotReload();
        };

        var modulesItem = new ToolStripMenuItem("Open Modules Folder");
        modulesItem.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", Program.ModulesPath);
        var logsItem = new ToolStripMenuItem("Open Logs Folder");
        logsItem.Click += (s, e) => System.Diagnostics.Process.Start("explorer.exe", Program.LogsPath);

        settingsMenu.Items.Add(reloadItem);
        settingsMenu.Items.Add(new ToolStripSeparator());
        settingsMenu.Items.Add(modulesItem);
        settingsMenu.Items.Add(logsItem);

        SettingsButton.MouseEnter += (s, e) =>
        {
            settingsMenu.Show(SettingsButton, new Point(0, SettingsButton.Height));
        };
    }

    public void RegisterModule(AsyncModule module)
    {
        Button navBtn = new()
        {
            Text = module.Name,
            Size = new Size(ButtonsPanel.Width - 10, 45),
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(5),
            Cursor = Cursors.Hand
        };

        navBtn.Click += (s, e) => SwitchToModule(module);
        ButtonsPanel.Controls.Add(navBtn);
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