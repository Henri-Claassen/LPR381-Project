namespace LPR381
{
    partial class FormNL
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
            this.btnNLMM = new System.Windows.Forms.Button();
            this.btnNLExit = new System.Windows.Forms.Button();
            this.dgvNLDisplay = new System.Windows.Forms.DataGridView();
            this.btnNLChooseFile = new System.Windows.Forms.Button();
            this.btnNLSolve = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnNLShadowPrice = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNLDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnNLMM
            // 
            this.btnNLMM.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNLMM.Location = new System.Drawing.Point(1136, -1);
            this.btnNLMM.Name = "btnNLMM";
            this.btnNLMM.Size = new System.Drawing.Size(120, 32);
            this.btnNLMM.TabIndex = 0;
            this.btnNLMM.Text = "Main Menu";
            this.btnNLMM.UseVisualStyleBackColor = true;
            this.btnNLMM.Click += new System.EventHandler(this.btnNLMM_Click);
            // 
            // btnNLExit
            // 
            this.btnNLExit.BackColor = System.Drawing.Color.Red;
            this.btnNLExit.FlatAppearance.BorderSize = 0;
            this.btnNLExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNLExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNLExit.ForeColor = System.Drawing.Color.White;
            this.btnNLExit.Location = new System.Drawing.Point(1252, -1);
            this.btnNLExit.Name = "btnNLExit";
            this.btnNLExit.Size = new System.Drawing.Size(60, 32);
            this.btnNLExit.TabIndex = 1;
            this.btnNLExit.Text = "Exit";
            this.btnNLExit.UseVisualStyleBackColor = false;
            this.btnNLExit.Click += new System.EventHandler(this.btnNLExit_Click);
            // 
            // dgvNLDisplay
            // 
            this.dgvNLDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNLDisplay.Location = new System.Drawing.Point(0, 38);
            this.dgvNLDisplay.Name = "dgvNLDisplay";
            this.dgvNLDisplay.RowHeadersWidth = 51;
            this.dgvNLDisplay.RowTemplate.Height = 24;
            this.dgvNLDisplay.Size = new System.Drawing.Size(1312, 611);
            this.dgvNLDisplay.TabIndex = 2;
            // 
            // btnNLChooseFile
            // 
            this.btnNLChooseFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNLChooseFile.Location = new System.Drawing.Point(0, -1);
            this.btnNLChooseFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnNLChooseFile.Name = "btnNLChooseFile";
            this.btnNLChooseFile.Size = new System.Drawing.Size(133, 32);
            this.btnNLChooseFile.TabIndex = 3;
            this.btnNLChooseFile.Text = "Choose File";
            this.btnNLChooseFile.UseVisualStyleBackColor = true;
            this.btnNLChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // btnNLSolve
            // 
            this.btnNLSolve.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNLSolve.Location = new System.Drawing.Point(129, -1);
            this.btnNLSolve.Name = "btnNLSolve";
            this.btnNLSolve.Size = new System.Drawing.Size(152, 32);
            this.btnNLSolve.TabIndex = 4;
            this.btnNLSolve.Text = "Non-Linear Solve";
            this.btnNLSolve.UseVisualStyleBackColor = true;
            this.btnNLSolve.Click += new System.EventHandler(this.btnNLSolve_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnNLShadowPrice
            // 
            this.btnNLShadowPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNLShadowPrice.Location = new System.Drawing.Point(280, -1);
            this.btnNLShadowPrice.Name = "btnNLShadowPrice";
            this.btnNLShadowPrice.Size = new System.Drawing.Size(225, 32);
            this.btnNLShadowPrice.TabIndex = 5;
            this.btnNLShadowPrice.Text = "Display Shadow Price";
            this.btnNLShadowPrice.UseVisualStyleBackColor = true;
            this.btnNLShadowPrice.Click += new System.EventHandler(this.btnNLShadowPrice_Click);
            // 
            // FormNL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::LPR381.Properties.Resources.Form_Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1312, 689);
            this.Controls.Add(this.btnNLShadowPrice);
            this.Controls.Add(this.btnNLSolve);
            this.Controls.Add(this.btnNLChooseFile);
            this.Controls.Add(this.dgvNLDisplay);
            this.Controls.Add(this.btnNLExit);
            this.Controls.Add(this.btnNLMM);
            this.Name = "FormNL";
            this.Text = "FormNL";
            ((System.ComponentModel.ISupportInitialize)(this.dgvNLDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnNLMM;
        private System.Windows.Forms.Button btnNLExit;
        private System.Windows.Forms.DataGridView dgvNLDisplay;
        private System.Windows.Forms.Button btnNLChooseFile;
        private System.Windows.Forms.Button btnNLSolve;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnNLShadowPrice;
    }
}