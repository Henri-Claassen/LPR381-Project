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
            ((System.ComponentModel.ISupportInitialize)(this.dgwMainDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // dgwMainDisplay
            // 
            this.dgwMainDisplay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgwMainDisplay.Location = new System.Drawing.Point(306, 12);
            this.dgwMainDisplay.Name = "dgwMainDisplay";
            this.dgwMainDisplay.Size = new System.Drawing.Size(652, 537);
            this.dgwMainDisplay.TabIndex = 0;
            // 
            // btnChooseFile
            // 
            this.btnChooseFile.Location = new System.Drawing.Point(95, 115);
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.Size = new System.Drawing.Size(75, 23);
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
            this.btnSimplex.Location = new System.Drawing.Point(95, 158);
            this.btnSimplex.Name = "btnSimplex";
            this.btnSimplex.Size = new System.Drawing.Size(119, 23);
            this.btnSimplex.TabIndex = 2;
            this.btnSimplex.Text = "Solve using Simplex";
            this.btnSimplex.UseVisualStyleBackColor = true;
            this.btnSimplex.Click += new System.EventHandler(this.btnSimplex_Click);
            // 
            // btnBranchAndBound
            // 
            this.btnBranchAndBound.Location = new System.Drawing.Point(95, 199);
            this.btnBranchAndBound.Name = "btnBranchAndBound";
            this.btnBranchAndBound.Size = new System.Drawing.Size(181, 23);
            this.btnBranchAndBound.TabIndex = 3;
            this.btnBranchAndBound.Text = "Solve Using Branch And Bound";
            this.btnBranchAndBound.UseVisualStyleBackColor = true;
            // 
            // btnKnapsack
            // 
            this.btnKnapsack.Location = new System.Drawing.Point(95, 246);
            this.btnKnapsack.Name = "btnKnapsack";
            this.btnKnapsack.Size = new System.Drawing.Size(150, 23);
            this.btnKnapsack.TabIndex = 4;
            this.btnKnapsack.Text = "Solve Using Knapsack";
            this.btnKnapsack.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.btnKnapsack);
            this.Controls.Add(this.btnBranchAndBound);
            this.Controls.Add(this.btnSimplex);
            this.Controls.Add(this.btnChooseFile);
            this.Controls.Add(this.dgwMainDisplay);
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
    }
}

