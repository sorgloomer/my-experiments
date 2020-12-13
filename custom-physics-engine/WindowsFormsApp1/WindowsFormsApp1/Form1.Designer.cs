namespace WindowsFormsApp1
{
    partial class Form1
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
            this.timer1 = new System.Timers.Timer();
            this.cbEnableRotations = new System.Windows.Forms.CheckBox();
            this.cbDrawAabbTree = new System.Windows.Forms.CheckBox();
            this.cbDrawAabbBoxes = new System.Windows.Forms.CheckBox();
            this.cbDrawContacts = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize) (this.timer1)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 20D;
            this.timer1.SynchronizingObject = this;
            this.timer1.Elapsed += new System.Timers.ElapsedEventHandler(this.timer1_Elapsed);
            // 
            // cbEnableRotations
            // 
            this.cbEnableRotations.BackColor = System.Drawing.Color.Black;
            this.cbEnableRotations.ForeColor = System.Drawing.Color.White;
            this.cbEnableRotations.Location = new System.Drawing.Point(12, 50);
            this.cbEnableRotations.Name = "cbEnableRotations";
            this.cbEnableRotations.Size = new System.Drawing.Size(224, 24);
            this.cbEnableRotations.TabIndex = 0;
            this.cbEnableRotations.Text = "Enable balancing by rotation";
            this.cbEnableRotations.UseVisualStyleBackColor = false;
            this.cbEnableRotations.Checked = true;
            this.cbEnableRotations.CheckedChanged += new System.EventHandler(this.cbEnableRotations_CheckedChanged);
            // 
            // cbDrawAabbTree
            // 
            this.cbDrawAabbTree.BackColor = System.Drawing.Color.Black;
            this.cbDrawAabbTree.ForeColor = System.Drawing.Color.White;
            this.cbDrawAabbTree.Location = new System.Drawing.Point(12, 80);
            this.cbDrawAabbTree.Name = "cbDrawAabbTree";
            this.cbDrawAabbTree.Size = new System.Drawing.Size(224, 24);
            this.cbDrawAabbTree.TabIndex = 1;
            this.cbDrawAabbTree.Text = "Draw AABB tree";
            this.cbDrawAabbTree.UseVisualStyleBackColor = false;
            this.cbDrawAabbTree.Checked = true;
            // 
            // cbDrawAabbBoxes
            // 
            this.cbDrawAabbBoxes.BackColor = System.Drawing.Color.Black;
            this.cbDrawAabbBoxes.ForeColor = System.Drawing.Color.White;
            this.cbDrawAabbBoxes.Location = new System.Drawing.Point(12, 110);
            this.cbDrawAabbBoxes.Name = "cbDrawAabbBoxes";
            this.cbDrawAabbBoxes.Size = new System.Drawing.Size(224, 24);
            this.cbDrawAabbBoxes.TabIndex = 2;
            this.cbDrawAabbBoxes.Text = "Draw AABB boxes";
            this.cbDrawAabbBoxes.UseVisualStyleBackColor = false;
            // 
            // cbDrawContacts
            // 
            this.cbDrawContacts.BackColor = System.Drawing.Color.Black;
            this.cbDrawContacts.ForeColor = System.Drawing.Color.White;
            this.cbDrawContacts.Location = new System.Drawing.Point(12, 140);
            this.cbDrawContacts.Name = "cbDrawContacts";
            this.cbDrawContacts.Size = new System.Drawing.Size(224, 24);
            this.cbDrawContacts.TabIndex = 3;
            this.cbDrawContacts.Text = "Draw contacts";
            this.cbDrawContacts.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 632);
            this.Controls.Add(this.cbDrawContacts);
            this.Controls.Add(this.cbDrawAabbBoxes);
            this.Controls.Add(this.cbDrawAabbTree);
            this.Controls.Add(this.cbEnableRotations);
            this.Name = "Form1";
            this.Text = "Form1";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
            ((System.ComponentModel.ISupportInitialize) (this.timer1)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox cbEnableRotations;
        private System.Windows.Forms.CheckBox cbDrawAabbTree;
        private System.Windows.Forms.CheckBox cbDrawAabbBoxes;
        private System.Windows.Forms.CheckBox cbDrawContacts;

        private System.Timers.Timer timer1;

        #endregion
    }
}