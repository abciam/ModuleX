namespace ModuleX
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ButtonsPanel = new FlowLayoutPanel();
            SettingsButton = new Button();
            ProgressBar = new ProgressBar();
            BarText = new Label();
            ObjectPanel = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // ButtonsPanel
            // 
            ButtonsPanel.BackColor = Color.Gainsboro;
            ButtonsPanel.Location = new Point(4, 31);
            ButtonsPanel.Name = "ButtonsPanel";
            ButtonsPanel.Size = new Size(150, 526);
            ButtonsPanel.TabIndex = 1;
            // 
            // SettingsButton
            // 
            SettingsButton.FlatStyle = FlatStyle.Popup;
            SettingsButton.Location = new Point(4, 4);
            SettingsButton.Name = "SettingsButton";
            SettingsButton.Size = new Size(150, 23);
            SettingsButton.TabIndex = 0;
            SettingsButton.Text = "Настройки";
            SettingsButton.UseVisualStyleBackColor = true;
            // 
            // ProgressBar
            // 
            ProgressBar.Location = new Point(158, 4);
            ProgressBar.Maximum = 250;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(622, 23);
            ProgressBar.Step = 1;
            ProgressBar.Style = ProgressBarStyle.Continuous;
            ProgressBar.TabIndex = 1;
            // 
            // BarText
            // 
            BarText.BackColor = Color.Transparent;
            BarText.Location = new Point(400, 8);
            BarText.Name = "BarText";
            BarText.Size = new Size(100, 15);
            BarText.TabIndex = 2;
            BarText.Text = "";
            BarText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ObjectPanel
            // 
            ObjectPanel.BackColor = SystemColors.Control;
            ObjectPanel.Location = new Point(158, 31);
            ObjectPanel.Name = "ObjectPanel";
            ObjectPanel.Size = new Size(622, 526);
            ObjectPanel.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(ObjectPanel);
            Controls.Add(SettingsButton);
            Controls.Add(ProgressBar);
            Controls.Add(ButtonsPanel);
            Controls.Add(BarText);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            Text = "ModuleX";
            ResumeLayout(false);
        }

        #endregion
        internal FlowLayoutPanel ButtonsPanel;
        internal ProgressBar ProgressBar;
        internal Button SettingsButton;
        internal Label BarText;
        internal FlowLayoutPanel ObjectPanel;
    }
}
