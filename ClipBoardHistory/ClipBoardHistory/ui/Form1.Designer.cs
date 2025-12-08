using System.Drawing;
using System.Windows.Forms;

namespace ClipBoardHistory.UI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private TextBox txtSearch;          // 検索バー
        private ListView lvHistory;        // 履歴リスト (左側メイン)
        private Panel pnlDetails;          // 詳細プレビューパネル (右側)
        private RichTextBox rtbPreview;    // プレビューテキスト表示用

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
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lvHistory = new System.Windows.Forms.ListView();
            this.pnlDetails = new System.Windows.Forms.Panel();
            this.rtbPreview = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtSearch (検索バー)
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "履歴を検索 (例: 鉄道 or 画像)"; // .NET 9.0で利用可能なPlaceholderTextを使用
            this.txtSearch.Size = new System.Drawing.Size(776, 23);
            this.txtSearch.TabIndex = 0;
            // 
            // lvHistory (履歴リスト - 左側)
            // 
            this.lvHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            new System.Windows.Forms.ColumnHeader() { Text = "日時", Width = 120 },
            new System.Windows.Forms.ColumnHeader() { Text = "種類", Width = 60 },
            new System.Windows.Forms.ColumnHeader() { Text = "内容プレビュー", Width = 200 }});
            this.lvHistory.FullRowSelect = true;
            this.lvHistory.HideSelection = false;
            this.lvHistory.Location = new System.Drawing.Point(12, 41);
            this.lvHistory.Name = "lvHistory";
            this.lvHistory.Size = new System.Drawing.Size(390, 497);
            this.lvHistory.TabIndex = 1;
            this.lvHistory.View = System.Windows.Forms.View.Details;
            this.lvHistory.UseCompatibleStateImageBehavior = false;
            // 
            // pnlDetails (詳細パネル - 右側)
            // 
            this.pnlDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDetails.Controls.Add(this.rtbPreview);
            this.pnlDetails.Location = new System.Drawing.Point(408, 41);
            this.pnlDetails.Name = "pnlDetails";
            this.pnlDetails.Size = new System.Drawing.Size(380, 497);
            this.pnlDetails.TabIndex = 2;
            // 
            // rtbPreview (プレビューRichTextBox)
            // 
            this.rtbPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbPreview.Location = new System.Drawing.Point(0, 0);
            this.rtbPreview.Name = "rtbPreview";
            this.rtbPreview.ReadOnly = true;
            this.rtbPreview.Size = new System.Drawing.Size(378, 495);
            this.rtbPreview.TabIndex = 0;
            this.rtbPreview.Text = "リストからアイテムを選択すると、ここに詳細が表示されます。";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 550);
            this.Controls.Add(this.pnlDetails);
            this.Controls.Add(this.lvHistory);
            this.Controls.Add(this.txtSearch);
            this.MinimumSize = new System.Drawing.Size(600, 400); // 最小サイズを設定
            this.Name = "Form1";
            this.Text = "ClipBoardHistory v2.0";
            this.pnlDetails.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}