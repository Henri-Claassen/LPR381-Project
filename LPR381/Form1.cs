using LPR381.Input_File_Handler;
using LPR381.Solving;
using LPR381.Stored_Info;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LPR381
{
    public partial class Form1 : Form
    {
        //Variables to use throughout for algorithms etc
        string[] lines;
        LpModel model;
        Solver solver;
        Tableau table;

        public Form1()
        {
            InitializeComponent();
            dgwMainDisplay.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        //Method to show the raw values from the text file
        private void showUserInput(string[] lines)
        {
            string[][] data = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                data[i] = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            dgwMainDisplay.Rows.Clear();
            dgwMainDisplay.Columns.Clear();

            int maxColumns = 0;
            foreach (string[] row in data)
            {
                if (row.Length > maxColumns)
                    maxColumns = row.Length;
            }
            for (int i = 0; i < maxColumns; i++)
            {
                dgwMainDisplay.Columns.Add("col" + i, "col" + i);
            }
            foreach (string[] row in data)
            {
                dgwMainDisplay.Rows.Add(row);
            }
        }

        //Method to show the canonical form to the user
        private void populateMainDisplay(Tableau table)
        {
            dgwMainDisplay.Rows.Clear();
            dgwMainDisplay.Columns.Clear();
            dgwMainDisplay.Columns.Add("TableNumber",table.TableNumber);
            foreach (var col in table.ColumnNames)
            {
                dgwMainDisplay.Columns.Add(col,col);
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                int rowIndex = dgwMainDisplay.Rows.Add();
                dgwMainDisplay.Rows[rowIndex].Cells["TableNumber"].Value = table.RowNames[i];

                for (int j = 0; j < table.Rows[i].Count; j++)
                {
                    dgwMainDisplay.Rows[rowIndex].Cells[j + 1].Value = table.Rows[i][j];   // +1 to skip past the RowLabel column
                }
            }
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;
                lines = HandleInput.ReadModelFile(filePath);
                showUserInput(lines);
            }
        }

        private void btnCanonicalForm_Click(object sender, EventArgs e)
        {
            model = HandleInput.ParseModel(lines);
            solver = new Solver();
            table = solver.BuildCanonicalForm(model);
            populateMainDisplay(table);
        }
    }
}
