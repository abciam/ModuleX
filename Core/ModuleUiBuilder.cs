using Common.Interfaces;

namespace ModuleX.Core;

internal class ModuleUiBuilder(FlowLayoutPanel panel) : IUiBuilder
{
    public void AddAction(string title, Action onClick)
    {
        Button btn = new()
        {
            Text = title,
            Size = new Size(160, 45),
            Margin = new Padding(10),
            FlatStyle = FlatStyle.Standard
        };
        btn.Click += (s, e) => onClick?.Invoke();
        panel.Controls.Add(btn);
    }

    public void AddInfo(string text)
    {
        Label lbl = new()
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(10),
            Font = new Font(Control.DefaultFont.FontFamily, 10, FontStyle.Bold)
        };
        panel.Controls.Add(lbl);
    }
}