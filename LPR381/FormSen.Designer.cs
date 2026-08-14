namespace LPR381
{
    partial class FormSen
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
            this.btnSenChooseFile = new System.Windows.Forms.Button();
            this.btnSenSolve = new System.Windows.Forms.Button();
            this.btnSenMM = new System.Windows.Forms.Button();
            this.btnSenExit = new System.Windows.Forms.Button();
            this.dgwSenDisplay = new System.Windows.Forms.DataGridView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dgwSenDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSenChooseFile
            // 
            this.btnSenChooseFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenChooseFile.Location = new System.Drawing.Point(0, -1);
            this.btnSenChooseFile.Name = "btnSenChooseFile";
            this.btnSenChooseFile.Size = new System.Drawing.Size(133, 33);
            this.btnSenChooseFile.TabIndex = 0;
            this.btnSenChooseFile.Text = "Choose File";
            this.btnSenChooseFile.UseVisualStyleBackColor = true;
            this.btnSenChooseFile.Click += new System.EventHandler(this.btnSenChooseFile_Click);
            // 
            // btnSenSolve
            // 
            this.btnSenSolve.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenSolve.Location = new System.Drawing.Point(130, -1);
            this.btnSenSolve.Name = "btnSenSolve";
            this.btnSenSolve.Size = new System.Drawing.Size(267, 33);
            this.btnSenSolve.TabIndex = 1;
            this.btnSenSolve.Text = "Solve using Sensitivity Analysis";
            this.btnSenSolve.UseVisualStyleBackColor = true;
            this.btnSenSolve.Click += new System.EventHandler(this.btnSenSolve_Click);
            // 
            // btnSenMM
            // 
            this.btnSenMM.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenMM.Location = new System.Drawing.Point(1138, -1);
            this.btnSenMM.Name = "btnSenMM";
            this.btnSenMM.Size = new System.Drawing.Size(120, 33);
            this.btnSenMM.TabIndex = 2;
            this.btnSenMM.Text = "Main Menu";
            this.btnSenMM.UseVisualStyleBackColor = true;
            this.btnSenMM.Click += new System.EventHandler(this.btnSenMM_Click);
            // 
            // btnSenExit
            // 
            this.btnSenExit.BackColor = System.Drawing.Color.Red;
            this.btnSenExit.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.btnSenExit.FlatAppearance.BorderSize = 0;
            this.btnSenExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSenExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenExit.ForeColor = System.Drawing.Color.White;
            this.btnSenExit.Location = new System.Drawing.Point(1253, -1);
            this.btnSenExit.Name = "btnSenExit";
            this.btnSenExit.Size = new System.Drawing.Size(60, 33);
            this.btnSenExit.TabIndex = 3;
            this.btnSenExit.Text = "Exit";
            this.btnSenExit.UseVisualStyleBackColor = false;
            this.btnSenExit.Click += new System.EventHandler(this.btnSenExit_Click);
            // 
            // dgwSenDisplay
            // 
            this.dgwSenDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwSenDisplay.Location = new System.Drawing.Point(0, 47);
            this.dgwSenDisplay.Name = "dgwSenDisplay";
            this.dgwSenDisplay.RowHeadersWidth = 51;
            this.dgwSenDisplay.RowTemplate.Height = 24;
            this.dgwSenDisplay.Size = new System.Drawing.Size(1313, 601);
            this.dgwSenDisplay.TabIndex = 4;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // FormSen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::LPR381.Properties.Resources.Form_Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1312, 689);
            this.Controls.Add(this.dgwSenDisplay);
            this.Controls.Add(this.btnSenExit);
            this.Controls.Add(this.btnSenMM);
            this.Controls.Add(this.btnSenSolve);
            this.Controls.Add(this.btnSenChooseFile);
            this.Name = "FormSen";
            this.Text = "FormSen";
            ((System.ComponentModel.ISupportInitialize)(this.dgwSenDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSenChooseFile;
        private System.Windows.Forms.Button btnSenSolve;
        private System.Windows.Forms.Button btnSenMM;
        private System.Windows.Forms.Button btnSenExit;
        private System.Windows.Forms.DataGridView dgwSenDisplay;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}