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
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.rtb2 = new System.Windows.Forms.RichTextBox();
            this.fps_rtb = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtb
            // 
            this.rtb.BackColor = System.Drawing.SystemColors.InfoText;
            this.rtb.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb.ForeColor = System.Drawing.SystemColors.Info;
            this.rtb.Location = new System.Drawing.Point(0, 33);
            this.rtb.Name = "rtb";
            this.rtb.ReadOnly = true;
            this.rtb.Size = new System.Drawing.Size(309, 164);
            this.rtb.TabIndex = 0;
            this.rtb.TabStop = false;
            this.rtb.Text = "";
            // 
            // rtb2
            // 
            this.rtb2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.rtb2.BackColor = System.Drawing.SystemColors.InfoText;
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
            // DebugBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(312, 199);
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
    }
}