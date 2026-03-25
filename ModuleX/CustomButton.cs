using Common.Enums;
using Common.Events;
using Common.Models;

namespace ModuleX;

internal class CustomButton : UserControl
{
    private Panel _statusIndicator;
    private Label _moduleNameLabel;
    private Button _btnLoad;
    private Button _btnUnload;

    public string ModulePath { get; }
    public AsyncModule ModuleInstance { get; private set; }
    public bool IsLoaded => ModuleInstance != null;

    public event EventHandler<string> OnLoadRequested;
    public event EventHandler<AsyncModule> OnUnloadRequested;
    public event EventHandler<AsyncModule> OnSelectRequested;

    public CustomButton(string modulePath, string moduleName, AsyncModule initialInstance = null!)
    {
        ModulePath = modulePath;
        ModuleInstance = initialInstance;

        InitializeUI(moduleName);
        UpdateStatus(initialInstance != null);
    }

    private void InitializeUI(string moduleName)
    {
        this.Size = new Size(400, 60);
        this.BackColor = Color.LightGray;
        this.Margin = new Padding(0);

        _statusIndicator = new Panel
        {
            Size = new Size(4, 60),
            Location = new Point(0, 0),
            BackColor = Color.Gray
        };

        _moduleNameLabel = new Label
        {
            Text = moduleName,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(8, 4),
            AutoSize = true,
            Cursor = Cursors.Hand
        };
        _moduleNameLabel.Click += (s, e) =>
        {
            Program.Instance!._bus.Publish(new LogEvent($"NotLoaded", ELog.Basic));
            if (IsLoaded)
            {
                Program.Instance!._bus.Publish(new LogEvent($"IsLoaded: {IsLoaded}", ELog.Basic));
                OnSelectRequested?.Invoke(this, ModuleInstance);
            }
        };

        _btnLoad = new Button
        {
            Text = "Load",
            Size = new Size(56, 25),
            Location = new Point(65, 32),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.Green
        };
        _btnLoad.Click += (s, e) => OnLoadRequested?.Invoke(this, ModulePath);

        _btnUnload = new Button
        {
            Text = "Unload",
            Size = new Size(56, 25),
            Location = new Point(7, 32),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.DarkRed
        };
        _btnUnload.Click += (s, e) =>
        {
            if (IsLoaded) OnUnloadRequested?.Invoke(this, ModuleInstance);
        };

        this.Controls.Add(_statusIndicator);
        this.Controls.Add(_moduleNameLabel);
        this.Controls.Add(_btnLoad);
        this.Controls.Add(_btnUnload);
    }

    public void UpdateStatus(bool isLoaded, AsyncModule newInstance = null)
    {
        ModuleInstance = newInstance;
        _statusIndicator.BackColor = isLoaded ? Color.LimeGreen : Color.Gray;
        _btnLoad.Enabled = !isLoaded;
        _btnUnload.Enabled = isLoaded;
    }
}