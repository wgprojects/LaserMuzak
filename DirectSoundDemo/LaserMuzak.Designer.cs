namespace DirectSoundDemo
{
    partial class LaserMuzak
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
            this.btnSaveMuzak = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.nudOffset = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudMicrosecPerPix = new System.Windows.Forms.NumericUpDown();
            this.cbFollow = new System.Windows.Forms.CheckBox();
            this.btnSetEndTime = new System.Windows.Forms.Button();
            this.btnSetStartTime = new System.Windows.Forms.Button();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.cbAxisW = new System.Windows.Forms.CheckBox();
            this.cbAxisY = new System.Windows.Forms.CheckBox();
            this.cbAxisX = new System.Windows.Forms.CheckBox();
            this.noteViewer1 = new DirectSoundDemo.NoteViewer();
            this.btnSetChannelPriorities = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudOffset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMicrosecPerPix)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSaveMuzak
            // 
            this.btnSaveMuzak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveMuzak.Location = new System.Drawing.Point(896, 182);
            this.btnSaveMuzak.Name = "btnSaveMuzak";
            this.btnSaveMuzak.Size = new System.Drawing.Size(149, 47);
            this.btnSaveMuzak.TabIndex = 0;
            this.btnSaveMuzak.Text = "Save Musak";
            this.btnSaveMuzak.UseVisualStyleBackColor = true;
            this.btnSaveMuzak.Click += new System.EventHandler(this.btnSaveMuzak_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox1.Location = new System.Drawing.Point(12, 32);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(209, 197);
            this.textBox1.TabIndex = 1;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(12, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(209, 23);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // nudOffset
            // 
            this.nudOffset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudOffset.Increment = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nudOffset.Location = new System.Drawing.Point(227, 180);
            this.nudOffset.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudOffset.Name = "nudOffset";
            this.nudOffset.Size = new System.Drawing.Size(106, 20);
            this.nudOffset.TabIndex = 4;
            this.nudOffset.ValueChanged += new System.EventHandler(this.nudOffset_ValueChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(339, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "usec";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(339, 208);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "usec pp";
            // 
            // nudMicrosecPerPix
            // 
            this.nudMicrosecPerPix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudMicrosecPerPix.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMicrosecPerPix.Location = new System.Drawing.Point(227, 206);
            this.nudMicrosecPerPix.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.nudMicrosecPerPix.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudMicrosecPerPix.Name = "nudMicrosecPerPix";
            this.nudMicrosecPerPix.Size = new System.Drawing.Size(106, 20);
            this.nudMicrosecPerPix.TabIndex = 6;
            this.nudMicrosecPerPix.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMicrosecPerPix.ValueChanged += new System.EventHandler(this.nudMicrosecPerPix_ValueChanged);
            // 
            // cbFollow
            // 
            this.cbFollow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbFollow.AutoSize = true;
            this.cbFollow.Location = new System.Drawing.Point(375, 181);
            this.cbFollow.Name = "cbFollow";
            this.cbFollow.Size = new System.Drawing.Size(78, 17);
            this.cbFollow.TabIndex = 8;
            this.cbFollow.Text = "Follow time";
            this.cbFollow.UseVisualStyleBackColor = true;
            this.cbFollow.CheckedChanged += new System.EventHandler(this.cbFollow_CheckedChanged);
            // 
            // btnSetEndTime
            // 
            this.btnSetEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetEndTime.Location = new System.Drawing.Point(579, 204);
            this.btnSetEndTime.Name = "btnSetEndTime";
            this.btnSetEndTime.Size = new System.Drawing.Size(94, 23);
            this.btnSetEndTime.TabIndex = 9;
            this.btnSetEndTime.Text = "Set End Time";
            this.btnSetEndTime.UseVisualStyleBackColor = true;
            this.btnSetEndTime.Click += new System.EventHandler(this.btnSetEndTime_Click);
            // 
            // btnSetStartTime
            // 
            this.btnSetStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetStartTime.Location = new System.Drawing.Point(579, 180);
            this.btnSetStartTime.Name = "btnSetStartTime";
            this.btnSetStartTime.Size = new System.Drawing.Size(94, 23);
            this.btnSetStartTime.TabIndex = 10;
            this.btnSetStartTime.Text = "Set Start Time";
            this.btnSetStartTime.UseVisualStyleBackColor = true;
            this.btnSetStartTime.Click += new System.EventHandler(this.btnSetStartTime_Click);
            // 
            // lblStartTime
            // 
            this.lblStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(679, 185);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(62, 13);
            this.lblStartTime.TabIndex = 11;
            this.lblStartTime.Text = "lblStartTime";
            // 
            // lblEndTime
            // 
            this.lblEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(679, 208);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(59, 13);
            this.lblEndTime.TabIndex = 12;
            this.lblEndTime.Text = "lblEndTime";
            // 
            // cbAxisW
            // 
            this.cbAxisW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAxisW.AutoSize = true;
            this.cbAxisW.Checked = true;
            this.cbAxisW.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAxisW.Location = new System.Drawing.Point(823, 183);
            this.cbAxisW.Name = "cbAxisW";
            this.cbAxisW.Size = new System.Drawing.Size(65, 17);
            this.cbAxisW.TabIndex = 13;
            this.cbAxisW.Text = "W Axis?";
            this.cbAxisW.UseVisualStyleBackColor = true;
            // 
            // cbAxisY
            // 
            this.cbAxisY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAxisY.AutoSize = true;
            this.cbAxisY.Checked = true;
            this.cbAxisY.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbAxisY.Location = new System.Drawing.Point(823, 198);
            this.cbAxisY.Name = "cbAxisY";
            this.cbAxisY.Size = new System.Drawing.Size(61, 17);
            this.cbAxisY.TabIndex = 14;
            this.cbAxisY.Text = "Y Axis?";
            this.cbAxisY.UseVisualStyleBackColor = true;
            // 
            // cbAxisX
            // 
            this.cbAxisX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAxisX.AutoSize = true;
            this.cbAxisX.Location = new System.Drawing.Point(823, 213);
            this.cbAxisX.Name = "cbAxisX";
            this.cbAxisX.Size = new System.Drawing.Size(61, 17);
            this.cbAxisX.TabIndex = 15;
            this.cbAxisX.Text = "X Axis?";
            this.cbAxisX.UseVisualStyleBackColor = true;
            // 
            // noteViewer1
            // 
            this.noteViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.noteViewer1.endTime = ((long)(-1));
            this.noteViewer1.Location = new System.Drawing.Point(227, 3);
            this.noteViewer1.MicrosecPerPixel = 1000D;
            this.noteViewer1.MinimumSize = new System.Drawing.Size(160, 100);
            this.noteViewer1.mseq = null;
            this.noteViewer1.Name = "noteViewer1";
            this.noteViewer1.Size = new System.Drawing.Size(818, 171);
            this.noteViewer1.startTime = ((long)(-1));
            this.noteViewer1.TabIndex = 3;
            this.noteViewer1.TimeOffset_us = 0D;
            this.noteViewer1.TimeSeek += new DirectSoundDemo.NoteViewer.TimeSeekHandler(this.noteViewer1_TimeSeek);
            this.noteViewer1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.noteViewer1_MouseClick);
            this.noteViewer1.Resize += new System.EventHandler(this.noteViewer1_Resize);
            // 
            // btnSetChannelPriorities
            // 
            this.btnSetChannelPriorities.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSetChannelPriorities.Location = new System.Drawing.Point(479, 180);
            this.btnSetChannelPriorities.Name = "btnSetChannelPriorities";
            this.btnSetChannelPriorities.Size = new System.Drawing.Size(94, 47);
            this.btnSetChannelPriorities.TabIndex = 16;
            this.btnSetChannelPriorities.Text = "Set Channel # Priorities";
            this.btnSetChannelPriorities.UseVisualStyleBackColor = true;
            this.btnSetChannelPriorities.Click += new System.EventHandler(this.btnSetChannelPriorities_Click);
            // 
            // LaserMuzak
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 232);
            this.Controls.Add(this.btnSetChannelPriorities);
            this.Controls.Add(this.cbAxisX);
            this.Controls.Add(this.cbAxisY);
            this.Controls.Add(this.cbAxisW);
            this.Controls.Add(this.lblEndTime);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.btnSetStartTime);
            this.Controls.Add(this.btnSetEndTime);
            this.Controls.Add(this.cbFollow);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nudMicrosecPerPix);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.nudOffset);
            this.Controls.Add(this.noteViewer1);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnSaveMuzak);
            this.Name = "LaserMuzak";
            this.Text = "LaserMuzak";
            this.Load += new System.EventHandler(this.LaserMuzak_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudOffset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMicrosecPerPix)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSaveMuzak;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnLoad;
        private NoteViewer noteViewer1;
        private System.Windows.Forms.NumericUpDown nudOffset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudMicrosecPerPix;
        private System.Windows.Forms.CheckBox cbFollow;
        private System.Windows.Forms.Button btnSetEndTime;
        private System.Windows.Forms.Button btnSetStartTime;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.CheckBox cbAxisW;
        private System.Windows.Forms.CheckBox cbAxisY;
        private System.Windows.Forms.CheckBox cbAxisX;
        private System.Windows.Forms.Button btnSetChannelPriorities;
    }
}