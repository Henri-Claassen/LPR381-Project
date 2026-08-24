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
            this.btnSenFindRange = new System.Windows.Forms.Button();
            this.btnSenShadowPrice = new System.Windows.Forms.Button();
            this.btnSenApplyVar = new System.Windows.Forms.Button();
            this.btnSenApplyRHS = new System.Windows.Forms.Button();
            this.btnSenApplyCol = new System.Windows.Forms.Button();
            this.btnSenAddActivity = new System.Windows.Forms.Button();
            this.btnSenAddConstraint = new System.Windows.Forms.Button();
            this.btnSenApplyDuality = new System.Windows.Forms.Button();
            this.btnSenSolveDual = new System.Windows.Forms.Button();
            this.btnSenBack = new System.Windows.Forms.Button();
            this.lblSenSelection = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgwSenDisplay)).BeginInit();
            this.SuspendLayout();
            //
            // btnSenChooseFile
            //
            this.btnSenChooseFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenChooseFile.Location = new System.Drawing.Point(-1, -1);
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
            this.btnSenSolve.Location = new System.Drawing.Point(128, -1);
            this.btnSenSolve.Name = "btnSenSolve";
            this.btnSenSolve.Size = new System.Drawing.Size(208, 33);
            this.btnSenSolve.TabIndex = 1;
            this.btnSenSolve.Text = "Sensitivity Analysis Solve";
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
            this.dgwSenDisplay.Location = new System.Drawing.Point(0, 88);
            this.dgwSenDisplay.Name = "dgwSenDisplay";
            this.dgwSenDisplay.RowHeadersWidth = 51;
            this.dgwSenDisplay.RowTemplate.Height = 24;
            this.dgwSenDisplay.Size = new System.Drawing.Size(1313, 560);
            this.dgwSenDisplay.TabIndex = 4;
            //
            // openFileDialog1
            //
            this.openFileDialog1.FileName = "openFileDialog1";
            //
            // btnSenFindRange
            //
            this.btnSenFindRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenFindRange.Location = new System.Drawing.Point(-1, 32);
            this.btnSenFindRange.Name = "btnSenFindRange";
            this.btnSenFindRange.Size = new System.Drawing.Size(140, 33);
            this.btnSenFindRange.TabIndex = 5;
            this.btnSenFindRange.Text = "Find Range";
            this.btnSenFindRange.UseVisualStyleBackColor = true;
            this.btnSenFindRange.Click += new System.EventHandler(this.btnSenFindRange_Click);
            //
            // btnSenShadowPrice
            //
            this.btnSenShadowPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenShadowPrice.Location = new System.Drawing.Point(332, -1);
            this.btnSenShadowPrice.Name = "btnSenShadowPrice";
            this.btnSenShadowPrice.Size = new System.Drawing.Size(185, 33);
            this.btnSenShadowPrice.TabIndex = 6;
            this.btnSenShadowPrice.Text = "Display Shadow Price";
            this.btnSenShadowPrice.UseVisualStyleBackColor = true;
            this.btnSenShadowPrice.Click += new System.EventHandler(this.btnSenShadowPrice_Click);
            //
            // btnSenApplyVar
            //
            this.btnSenApplyVar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenApplyVar.Location = new System.Drawing.Point(135, 32);
            this.btnSenApplyVar.Name = "btnSenApplyVar";
            this.btnSenApplyVar.Size = new System.Drawing.Size(180, 33);
            this.btnSenApplyVar.TabIndex = 7;
            this.btnSenApplyVar.Text = "Apply Variable Change";
            this.btnSenApplyVar.UseVisualStyleBackColor = true;
            this.btnSenApplyVar.Click += new System.EventHandler(this.btnSenApplyVar_Click);
            //
            // btnSenApplyRHS
            //
            this.btnSenApplyRHS.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenApplyRHS.Location = new System.Drawing.Point(311, 32);
            this.btnSenApplyRHS.Name = "btnSenApplyRHS";
            this.btnSenApplyRHS.Size = new System.Drawing.Size(160, 33);
            this.btnSenApplyRHS.TabIndex = 8;
            this.btnSenApplyRHS.Text = "Apply RHS Change";
            this.btnSenApplyRHS.UseVisualStyleBackColor = true;
            this.btnSenApplyRHS.Click += new System.EventHandler(this.btnSenApplyRHS_Click);
            //
            // btnSenApplyCol
            //
            this.btnSenApplyCol.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenApplyCol.Location = new System.Drawing.Point(467, 32);
            this.btnSenApplyCol.Name = "btnSenApplyCol";
            this.btnSenApplyCol.Size = new System.Drawing.Size(180, 33);
            this.btnSenApplyCol.TabIndex = 9;
            this.btnSenApplyCol.Text = "Apply Column Change";
            this.btnSenApplyCol.UseVisualStyleBackColor = true;
            this.btnSenApplyCol.Click += new System.EventHandler(this.btnSenApplyCol_Click);
            //
            // btnSenAddActivity
            //
            this.btnSenAddActivity.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenAddActivity.Location = new System.Drawing.Point(643, 32);
            this.btnSenAddActivity.Name = "btnSenAddActivity";
            this.btnSenAddActivity.Size = new System.Drawing.Size(130, 33);
            this.btnSenAddActivity.TabIndex = 10;
            this.btnSenAddActivity.Text = "Add Activity";
            this.btnSenAddActivity.UseVisualStyleBackColor = true;
            this.btnSenAddActivity.Click += new System.EventHandler(this.btnSenAddActivity_Click);
            //
            // btnSenAddConstraint
            //
            this.btnSenAddConstraint.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenAddConstraint.Location = new System.Drawing.Point(769, 32);
            this.btnSenAddConstraint.Name = "btnSenAddConstraint";
            this.btnSenAddConstraint.Size = new System.Drawing.Size(150, 33);
            this.btnSenAddConstraint.TabIndex = 11;
            this.btnSenAddConstraint.Text = "Add Constraint";
            this.btnSenAddConstraint.UseVisualStyleBackColor = true;
            this.btnSenAddConstraint.Click += new System.EventHandler(this.btnSenAddConstraint_Click);
            //
            // btnSenApplyDuality
            //
            this.btnSenApplyDuality.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenApplyDuality.Location = new System.Drawing.Point(513, -1);
            this.btnSenApplyDuality.Name = "btnSenApplyDuality";
            this.btnSenApplyDuality.Size = new System.Drawing.Size(110, 33);
            this.btnSenApplyDuality.TabIndex = 14;
            this.btnSenApplyDuality.Text = "Duality";
            this.btnSenApplyDuality.UseVisualStyleBackColor = true;
            this.btnSenApplyDuality.Click += new System.EventHandler(this.btnSenApplyDuality_Click);
            //
            // btnSenSolveDual
            //
            this.btnSenSolveDual.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenSolveDual.Location = new System.Drawing.Point(619, -1);
            this.btnSenSolveDual.Name = "btnSenSolveDual";
            this.btnSenSolveDual.Size = new System.Drawing.Size(150, 33);
            this.btnSenSolveDual.TabIndex = 15;
            this.btnSenSolveDual.Text = "Solve Dual Model";
            this.btnSenSolveDual.UseVisualStyleBackColor = true;
            this.btnSenSolveDual.Click += new System.EventHandler(this.btnSenSolveDual_Click);
            //
            // btnSenBack
            //
            this.btnSenBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSenBack.Location = new System.Drawing.Point(765, -1);
            this.btnSenBack.Name = "btnSenBack";
            this.btnSenBack.Size = new System.Drawing.Size(175, 33);
            this.btnSenBack.TabIndex = 18;
            this.btnSenBack.Text = "Back to Model";
            this.btnSenBack.UseVisualStyleBackColor = true;
            this.btnSenBack.Click += new System.EventHandler(this.btnSenBack_Click);
            //
            // lblSenSelection
            //
            this.lblSenSelection.AutoSize = false;
            this.lblSenSelection.BackColor = System.Drawing.Color.White;
            this.lblSenSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSenSelection.Location = new System.Drawing.Point(-1, 65);
            this.lblSenSelection.Name = "lblSenSelection";
            this.lblSenSelection.Size = new System.Drawing.Size(1300, 22);
            this.lblSenSelection.TabIndex = 17;
            this.lblSenSelection.Text = "Selected variable: none    |    Selected constraint: none";
            this.lblSenSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // FormSen
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::LPR381.Properties.Resources.Form_Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1312, 689);
            this.Controls.Add(this.lblSenSelection);
            this.Controls.Add(this.btnSenBack);
            this.Controls.Add(this.btnSenSolveDual);
            this.Controls.Add(this.btnSenApplyDuality);
            this.Controls.Add(this.btnSenAddConstraint);
            this.Controls.Add(this.btnSenAddActivity);
            this.Controls.Add(this.btnSenApplyCol);
            this.Controls.Add(this.btnSenApplyRHS);
            this.Controls.Add(this.btnSenApplyVar);
            this.Controls.Add(this.btnSenShadowPrice);
            this.Controls.Add(this.btnSenFindRange);
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
        private System.Windows.Forms.Button btnSenFindRange;
        private System.Windows.Forms.Button btnSenShadowPrice;
        private System.Windows.Forms.Button btnSenApplyVar;
        private System.Windows.Forms.Button btnSenApplyRHS;
        private System.Windows.Forms.Button btnSenApplyCol;
        private System.Windows.Forms.Button btnSenAddActivity;
        private System.Windows.Forms.Button btnSenAddConstraint;
        private System.Windows.Forms.Button btnSenApplyDuality;
        private System.Windows.Forms.Button btnSenSolveDual;
        private System.Windows.Forms.Button btnSenBack;
        private System.Windows.Forms.Label lblSenSelection;
    }
}
