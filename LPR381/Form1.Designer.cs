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
            this.btnCanonicalForm = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
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
            // btnCanonicalForm
            // 
            this.btnCanonicalForm.Location = new System.Drawing.Point(69, 198);
            this.btnCanonicalForm.Name = "btnCanonicalForm";
            this.btnCanonicalForm.Size = new System.Drawing.Size(129, 23);
            this.btnCanonicalForm.TabIndex = 2;
            this.btnCanonicalForm.Text = "Get Canonical  Form";
            this.btnCanonicalForm.UseVisualStyleBackColor = true;
            this.btnCanonicalForm.Click += new System.EventHandler(this.btnCanonicalForm_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.btnCanonicalForm);
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
        private System.Windows.Forms.Button btnCanonicalForm;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

