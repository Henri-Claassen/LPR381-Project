namespace LPR381
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
            this.dgwMainDisplay = new System.Windows.Forms.DataGridView();
            this.btnChooseFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnSimplex = new System.Windows.Forms.Button();
            this.btnBranchAndBound = new System.Windows.Forms.Button();
            this.btnKnapsack = new System.Windows.Forms.Button();
            this.btnMainF1 = new System.Windows.Forms.Button();
            this.btnExit1 = new System.Windows.Forms.Button();
            this.btnF1CanonicalForm = new System.Windows.Forms.Button();
            this.btnCuttingPlane = new System.Windows.Forms.Button();
            this.btnNewPivot = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgwMainDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // dgwMainDisplay
            // 
            this.dgwMainDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwMainDisplay.Location = new System.Drawing.Point(0, 48);
            this.dgwMainDisplay.Margin = new System.Windows.Forms.Padding(4);
            this.dgwMainDisplay.Name = "dgwMainDisplay";
            this.dgwMainDisplay.RowHeadersWidth = 51;
            this.dgwMainDisplay.Size = new System.Drawing.Size(1314, 600);
            this.dgwMainDisplay.TabIndex = 0;
            // 
            // btnChooseFile
            // 
            this.btnChooseFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChooseFile.Location = new System.Drawing.Point(0, -1);
            this.btnChooseFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.Size = new System.Drawing.Size(133, 32);
            this.btnChooseFile.TabIndex = 1;
            this.btnChooseFile.Text = "Choose File";
            this.btnChooseFile.UseVisualStyleBackColor = true;
            this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // btnSimplex
            // 
            this.btnSimplex.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSimplex.Location = new System.Drawing.Point(275, -1);
            this.btnSimplex.Margin = new System.Windows.Forms.Padding(4);
            this.btnSimplex.Name = "btnSimplex";
            this.btnSimplex.Size = new System.Drawing.Size(132, 32);
            this.btnSimplex.TabIndex = 2;
            this.btnSimplex.Text = "Simplex Solve";
            this.btnSimplex.UseVisualStyleBackColor = true;
            this.btnSimplex.Click += new System.EventHandler(this.btnSimplex_Click);
            // 
            // btnBranchAndBound
            // 
            this.btnBranchAndBound.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBranchAndBound.Location = new System.Drawing.Point(404, -1);
            this.btnBranchAndBound.Margin = new System.Windows.Forms.Padding(4);
            this.btnBranchAndBound.Name = "btnBranchAndBound";
            this.btnBranchAndBound.Size = new System.Drawing.Size(204, 32);
            this.btnBranchAndBound.TabIndex = 3;
            this.btnBranchAndBound.Text = "Branch And Bound Solve";
            this.btnBranchAndBound.UseVisualStyleBackColor = true;
            this.btnBranchAndBound.Click += new System.EventHandler(this.btnBranchAndBound_Click);
            // 
            // btnKnapsack
            // 
            this.btnKnapsack.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnKnapsack.Location = new System.Drawing.Point(605, -1);
            this.btnKnapsack.Margin = new System.Windows.Forms.Padding(4);
            this.btnKnapsack.Name = "btnKnapsack";
            this.btnKnapsack.Size = new System.Drawing.Size(136, 32);
            this.btnKnapsack.TabIndex = 4;
            this.btnKnapsack.Text = "Knapsack Solve";
            this.btnKnapsack.UseVisualStyleBackColor = true;
            this.btnKnapsack.Click += new System.EventHandler(this.btnKnapsack_Click);
            // 
            // btnMainF1
            // 
            this.btnMainF1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMainF1.Location = new System.Drawing.Point(1138, -1);
            this.btnMainF1.Name = "btnMainF1";
            this.btnMainF1.Size = new System.Drawing.Size(120, 32);
            this.btnMainF1.TabIndex = 5;
            this.btnMainF1.Text = "Main Menu";
            this.btnMainF1.UseVisualStyleBackColor = true;
            this.btnMainF1.Click += new System.EventHandler(this.btnMainF1_Click);
            // 
            // btnExit1
            // 
            this.btnExit1.BackColor = System.Drawing.Color.Red;
            this.btnExit1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnExit1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit1.FlatAppearance.BorderSize = 0;
            this.btnExit1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit1.ForeColor = System.Drawing.Color.White;
            this.btnExit1.Location = new System.Drawing.Point(1254, -1);
            this.btnExit1.Name = "btnExit1";
            this.btnExit1.Size = new System.Drawing.Size(60, 31);
            this.btnExit1.TabIndex = 6;
            this.btnExit1.Text = "Exit";
            this.btnExit1.UseVisualStyleBackColor = false;
            this.btnExit1.Click += new System.EventHandler(this.btnExit1_Click);
            // 
            // btnF1CanonicalForm
            // 
            this.btnF1CanonicalForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnF1CanonicalForm.Location = new System.Drawing.Point(128, -1);
            this.btnF1CanonicalForm.Name = "btnF1CanonicalForm";
            this.btnF1CanonicalForm.Size = new System.Drawing.Size(153, 32);
            this.btnF1CanonicalForm.TabIndex = 8;
            this.btnF1CanonicalForm.Text = "Canonical Form";
            this.btnF1CanonicalForm.UseVisualStyleBackColor = true;
            this.btnF1CanonicalForm.Click += new System.EventHandler(this.btnF1CanonicalForm_Click);
            // 
            // btnCuttingPlane
            // 
            this.btnCuttingPlane.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCuttingPlane.Location = new System.Drawing.Point(739, -1);
            this.btnCuttingPlane.Name = "btnCuttingPlane";
            this.btnCuttingPlane.Size = new System.Drawing.Size(172, 32);
            this.btnCuttingPlane.TabIndex = 9;
            this.btnCuttingPlane.Text = "Cutting Plane Solve";
            this.btnCuttingPlane.UseVisualStyleBackColor = true;
            this.btnCuttingPlane.Click += new System.EventHandler(this.btnCuttingPlane_Click);
            // 
            // btnNewPivot
            // 
            this.btnNewPivot.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewPivot.Location = new System.Drawing.Point(909, -1);
            this.btnNewPivot.Name = "btnNewPivot";
            this.btnNewPivot.Size = new System.Drawing.Size(132, 32);
            this.btnNewPivot.TabIndex = 10;
            this.btnNewPivot.Text = "New Pivot";
            this.btnNewPivot.UseVisualStyleBackColor = true;
            this.btnNewPivot.Click += new System.EventHandler(this.btnNewPivot_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::LPR381.Properties.Resources.Form_Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1312, 689);
            this.Controls.Add(this.btnNewPivot);
            this.Controls.Add(this.btnF1CanonicalForm);
            this.Controls.Add(this.btnExit1);
            this.Controls.Add(this.btnMainF1);
            this.Controls.Add(this.btnCuttingPlane);
            this.Controls.Add(this.btnKnapsack);
            this.Controls.Add(this.btnBranchAndBound);
            this.Controls.Add(this.btnSimplex);
            this.Controls.Add(this.btnChooseFile);
            this.Controls.Add(this.dgwMainDisplay);
            this.Location = new System.Drawing.Point(0, -1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dgwMainDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgwMainDisplay;
        private System.Windows.Forms.Button btnChooseFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnSimplex;
        private System.Windows.Forms.Button btnBranchAndBound;
        private System.Windows.Forms.Button btnKnapsack;
        private System.Windows.Forms.Button btnMainF1;
        private System.Windows.Forms.Button btnExit1;
        private System.Windows.Forms.Button btnF1CanonicalForm;
        private System.Windows.Forms.Button btnCuttingPlane;
        private System.Windows.Forms.Button btnNewPivot;
    }
}

