namespace myfarstAPP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            listBox1 = new ListBox();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            toolStripMenuItem_Show = new ToolStripMenuItem();
            toolStripMenuItem_Exit = new ToolStripMenuItem();
            btnClear = new Button();
            btnSave = new Button(); // 追加
            btnLoad = new Button(); // 追加
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(12, 12);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(698, 364);
            listBox1.TabIndex = 0;
            listBox1.MouseDoubleClick += listBox1_MouseDoubleClick; // ダブルクリックイベントを関連付け
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Text = "ClipBoardHistory";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem_Show, toolStripMenuItem_Exit });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(101, 48);
            // 
            // toolStripMenuItem_Show
            // 
            toolStripMenuItem_Show.Name = "toolStripMenuItem_Show";
            toolStripMenuItem_Show.Size = new Size(100, 22);
            toolStripMenuItem_Show.Text = "表示";
            toolStripMenuItem_Show.Click += toolStripMenuItem_Show_Click;
            // 
            // toolStripMenuItem_Exit
            // 
            toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            toolStripMenuItem_Exit.Size = new Size(100, 22);
            toolStripMenuItem_Exit.Text = "終了";
            toolStripMenuItem_Exit.Click += toolStripMenuItem_Exit_Click;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.Location = new Point(605, 382);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(105, 34);
            btnClear.TabIndex = 3;
            btnClear.Text = "履歴をクリア";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnSave
            // ★履歴を保存ボタン
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSave.Location = new Point(12, 382);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(105, 34);
            btnSave.TabIndex = 1;
            btnSave.Text = "履歴を保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnLoad
            // ★履歴を読み込みボタン
            btnLoad.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnLoad.Location = new Point(123, 382);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(105, 34);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "履歴を読み込み";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += btnLoad_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(722, 428);
            Controls.Add(btnLoad);  // フォームにボタンを追加
            Controls.Add(btnSave);  // フォームにボタンを追加
            Controls.Add(btnClear);
            Controls.Add(listBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "ClipBoardHistory";
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private ListBox listBox1;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem toolStripMenuItem_Show;
        private ToolStripMenuItem toolStripMenuItem_Exit;
        private Button btnClear;
        private Button btnSave; // 追加
        private Button btnLoad; // 追加
    }
}