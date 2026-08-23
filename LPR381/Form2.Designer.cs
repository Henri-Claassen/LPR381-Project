namespace LPR381
{
    partial class FormMain_Menu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain_Menu));
            this.btnForm1 = new System.Windows.Forms.Button();
            this.btnSen = new System.Windows.Forms.Button();
            this.btnNonLin = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnForm1
            // 
            this.btnForm1.Location = new System.Drawing.Point(123, 224);
            this.btnForm1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnForm1.Name = "btnForm1";
            this.btnForm1.Size = new System.Drawing.Size(164, 41);
            this.btnForm1.TabIndex = 7;
            this.btnForm1.Text = "Solver";
            this.btnForm1.UseVisualStyleBackColor = true;
            this.btnForm1.Click += new System.EventHandler(this.btnForm1_Click);
            // 
            // btnSen
            // 
            this.btnSen.Location = new System.Drawing.Point(565, 224);
            this.btnSen.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSen.Name = "btnSen";
            this.btnSen.Size = new System.Drawing.Size(164, 41);
            this.btnSen.TabIndex = 8;
            this.btnSen.Text = "Sensitivity Analysis";
            this.btnSen.UseVisualStyleBackColor = true;
            this.btnSen.Click += new System.EventHandler(this.btnSen_Click);
            // 
            // btnNonLin
            // 
            this.btnNonLin.Location = new System.Drawing.Point(1009, 224);
            this.btnNonLin.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnNonLin.Name = "btnNonLin";
            this.btnNonLin.Size = new System.Drawing.Size(164, 41);
            this.btnNonLin.TabIndex = 9;
            this.btnNonLin.Text = "Non-Linear Analysis";
            this.btnNonLin.UseVisualStyleBackColor = true;
            this.btnNonLin.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.Red;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(1254, -1);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(60, 37);
            this.btnExit.TabIndex = 10;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // FormMain_Menu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1312, 689);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnNonLin);
            this.Controls.Add(this.btnSen);
            this.Controls.Add(this.btnForm1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "FormMain_Menu";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnForm1;
        private System.Windows.Forms.Button btnSen;
        private System.Windows.Forms.Button btnNonLin;
        private System.Windows.Forms.Button btnExit;
    }
}