using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Configuration;

namespace EnrollmentSystem
{
    public partial class SubjectList : Form
    {
        private readonly string connectionString;

        public SubjectList()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
        }

        private void SubjectList_Load(object sender, EventArgs e)
        {
            DisplaySubjectList();
        }

        private void DisplaySubjectList()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string query = @"SELECT s.SFSUBJCODE, s.SFSUBJDESC, s.SFSUBJUNITS, 
                                           s.SFSUBJREGOFRNG, s.SFSUBJCATEGORY, 
                                           s.SFSUBJCOURSECODE,
                                           sch.SSFSTARTTIME, sch.SSFENDTIME, sch.SSFEDPCODE,
                                           (SELECT TOP 1 SUBJPRECODE 
                                            FROM SubjectPreqFile 
                                            WHERE SUBJCODE = s.SFSUBJCODE 
                                            AND SUBJCATEGORY = 'PR') as PREREQUISITE
                                    FROM SubjectFile s
                                    LEFT JOIN SubjectSchedFile sch ON s.SFSUBJCODE = sch.SSFSUBJCODE
                                    ORDER BY s.SFSUBJCOURSECODE";

                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            SubjectListDataGridView.DataSource = null;
                            SubjectListDataGridView.Rows.Clear();

                            foreach (DataRow row in dt.Rows)
                            {
                                int rowIndex = SubjectListDataGridView.Rows.Add();
                                SubjectListDataGridView.Rows[rowIndex].Cells["SubjectCodeColumn"].Value = row["SFSUBJCODE"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["DescriptionColumn"].Value = row["SFSUBJDESC"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["UnitsColumn"].Value = row["SFSUBJUNITS"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["OfferingColumn"].Value = row["SFSUBJREGOFRNG"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["CategoryColumn"].Value = row["SFSUBJCATEGORY"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["CourseCodeColumn"].Value = row["SFSUBJCOURSECODE"];
                                
                                
                                SubjectListDataGridView.Rows[rowIndex].Cells["StartTimeColumn"].Value = row["SSFSTARTTIME"] != DBNull.Value ? 
                                    ((DateTime)row["SSFSTARTTIME"]).ToString("hh:mm tt") : "";
                                SubjectListDataGridView.Rows[rowIndex].Cells["EndTimeColumn"].Value = row["SSFENDTIME"] != DBNull.Value ? 
                                    ((DateTime)row["SSFENDTIME"]).ToString("hh:mm tt") : "";
                                SubjectListDataGridView.Rows[rowIndex].Cells["EDPCodeColumn"].Value = row["SSFEDPCODE"];
                                SubjectListDataGridView.Rows[rowIndex].Cells["RequisiteColumn"].Value = row["PREREQUISITE"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subject list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
