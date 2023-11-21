namespace BellColorTextBox.Demo.Net
{
    partial class WinFormDemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Bell.Languages.Language language2 = new Bell.Languages.Language();
            TextBoxContorl = new Bell.TextBoxControl();
            SuspendLayout();
            // 
            // TextBoxContorl
            // 
            TextBoxContorl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxContorl.AutoIndent = true;
            TextBoxContorl.EolMode = Bell.EolMode.LF;
            TextBoxContorl.IsDebugMode = false;
            TextBoxContorl.Language = language2;
            TextBoxContorl.LeadingHeight = 1.2F;
            TextBoxContorl.Location = new Point(12, 12);
            TextBoxContorl.Name = "TextBoxContorl";
            TextBoxContorl.ReadOnly = false;
            TextBoxContorl.ShowingSpace = true;
            TextBoxContorl.ShowingTab = true;
            TextBoxContorl.Size = new Size(776, 426);
            TextBoxContorl.SyntaxFolding = true;
            TextBoxContorl.SyntaxHighlight = true;
            TextBoxContorl.TabIndex = 0;
            TextBoxContorl.TabMode = Bell.TabMode.Tab;
            TextBoxContorl.TabSize = 4;
            TextBoxContorl.WordWrapIndent = true;
            TextBoxContorl.WrapMode = Bell.WrapMode.None;
            // 
            // WinFormDemoForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TextBoxContorl);
            Name = "WinFormDemoForm";
            Text = "WinFormDemoForm";
            ResumeLayout(false);
        }

        #endregion

        public Bell.TextBoxControl TextBoxContorl;
    }
}