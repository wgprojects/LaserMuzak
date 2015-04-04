namespace DirectSoundDemo
{
    partial class NoteViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // NoteViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "NoteViewer";
            this.Size = new System.Drawing.Size(558, 160);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NoteViewer_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.NoteViewer_MouseMove);
            this.Resize += new System.EventHandler(this.NoteViewer_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
