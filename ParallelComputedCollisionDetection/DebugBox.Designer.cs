namespace ParallelComputedCollisionDetection
{
    partial class DebugBox
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
            this.comp_rtb = new System.Windows.Forms.RichTextBox();
            this.fps_rtb = new System.Windows.Forms.RichTextBox();
            this.rtb2 = new System.Windows.Forms.RichTextBox();
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.rtb_log = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // comp_rtb
            // 
            this.comp_rtb.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.comp_rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.comp_rtb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comp_rtb.ForeColor = System.Drawing.SystemColors.Info;
            this.comp_rtb.Location = new System.Drawing.Point(0, 3);
            this.comp_rtb.Name = "comp_rtb";
            this.comp_rtb.ReadOnly = true;
            this.comp_rtb.Size = new System.Drawing.Size(309, 194);
            this.comp_rtb.TabIndex = 3;
            this.comp_rtb.TabStop = false;
            this.comp_rtb.Text = "";
            // 
            // fps_rtb
            // 
            this.fps_rtb.BackColor = System.Drawing.SystemColors.InfoText;
            this.fps_rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.fps_rtb.ForeColor = System.Drawing.SystemColors.Info;
            this.fps_rtb.Location = new System.Drawing.Point(0, 12);
            this.fps_rtb.Name = "fps_rtb";
            this.fps_rtb.ReadOnly = true;
            this.fps_rtb.Size = new System.Drawing.Size(133, 24);
            this.fps_rtb.TabIndex = 2;
            this.fps_rtb.TabStop = false;
            this.fps_rtb.Text = "";
            // 
            // rtb2
            // 
            this.rtb2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.rtb2.BackColor = System.Drawing.SystemColors.InfoText;
            this.rtb2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb2.ForeColor = System.Drawing.SystemColors.Info;
            this.rtb2.Location = new System.Drawing.Point(209, 101);
            this.rtb2.Name = "rtb2";
            this.rtb2.ReadOnly = true;
            this.rtb2.Size = new System.Drawing.Size(100, 96);
            this.rtb2.TabIndex = 1;
            this.rtb2.TabStop = false;
            this.rtb2.Text = "";
            // 
            // rtb
            // 
            this.rtb.BackColor = System.Drawing.SystemColors.InfoText;
            this.rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb.ForeColor = System.Drawing.SystemColors.Info;
            this.rtb.Location = new System.Drawing.Point(0, 33);
            this.rtb.Name = "rtb";
            this.rtb.ReadOnly = true;
            this.rtb.Size = new System.Drawing.Size(315, 240);
            this.rtb.TabIndex = 0;
            this.rtb.TabStop = false;
            this.rtb.Text = "";
            // 
            // rtb_log
            // 
            this.rtb_log.BackColor = System.Drawing.SystemColors.InfoText;
            this.rtb_log.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb_log.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtb_log.ForeColor = System.Drawing.SystemColors.Info;
            this.rtb_log.Location = new System.Drawing.Point(0, 101);
            this.rtb_log.Name = "rtb_log";
            this.rtb_log.ReadOnly = true;
            this.rtb_log.Size = new System.Drawing.Size(100, 96);
            this.rtb_log.TabIndex = 4;
            this.rtb_log.TabStop = false;
            this.rtb_log.Text = "";
            // 
            // DebugBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(312, 199);
            this.Controls.Add(this.rtb_log);
            this.Controls.Add(this.comp_rtb);
            this.Controls.Add(this.fps_rtb);
            this.Controls.Add(this.rtb2);
            this.Controls.Add(this.rtb);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DebugBox";
            this.Text = "DebugBox";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.DebugBox_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.RichTextBox rtb2;
        private System.Windows.Forms.RichTextBox fps_rtb;
        private System.Windows.Forms.RichTextBox comp_rtb;
        private System.Windows.Forms.RichTextBox rtb_log;
    }
}