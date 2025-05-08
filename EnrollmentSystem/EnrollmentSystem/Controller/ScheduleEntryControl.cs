using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnrollmentSystem.Repo;

namespace EnrollmentSystem.Controller
{
    public partial class ScheduleEntryControl : UserControl
    {
        private ScheduleEntryData repo;

        public ScheduleEntryControl()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            repo = new ScheduleEntryData(connectionString);

            // Attach input restriction handlers
            EDPCodeTextBox.KeyPress += EDPCodeTextBox_KeyPress;
            DaysTextBox.KeyPress += DaysTextBox_KeyPress;
            SchoolYearTextBox.KeyPress += SchoolYearTextBox_KeyPress;
        }

        // EDP Code: Only numbers
        private void EDPCodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        // Days: Only letters, comma, and space
        private void DaysTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != ' ')
                e.Handled = true;
        }

        // School Year: Only numbers and dash
        private void SchoolYearTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '-')
                e.Handled = true;
        }

        private void SubjectCodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string subjectCode = SubjectCodeTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(subjectCode))
                {
                    string description = repo.GetSubjectDescription(subjectCode);
                    if (!string.IsNullOrEmpty(description))
                    {
                        DescriptionTextBox.Text = description;
                    }
                    else
                    {
                        MessageBox.Show("Subject code not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DescriptionTextBox.Text = string.Empty;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(EDPCodeTextBox.Text) ||
                string.IsNullOrEmpty(SubjectCodeTextBox.Text) ||
                string.IsNullOrEmpty(DaysTextBox.Text) ||
                string.IsNullOrEmpty(SectionTextBox.Text) ||
                string.IsNullOrEmpty(SchoolYearTextBox.Text) ||
                XMComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Time validation
            if (StartTimeDTP.Value.TimeOfDay == EndTimeDTP.Value.TimeOfDay)
            {
                MessageBox.Show("Start Time and End Time cannot be the same.", "Time Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // EDP Code duplicate check
            if (EDPCodeExists(EDPCodeTextBox.Text.Trim()))
            {
                MessageBox.Show("An entry with this EDP Code already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bool success = repo.InsertScheduleEntry(
                    EDPCodeTextBox.Text.Trim(),
                    SubjectCodeTextBox.Text.Trim(),
                    StartTimeDTP.Value.TimeOfDay,
                    EndTimeDTP.Value.TimeOfDay,
                    DaysTextBox.Text.Trim(),
                    SectionTextBox.Text.Trim(),
                    SchoolYearTextBox.Text.Trim(),
                    XMComboBox.Text.Trim()
                );

                if (success)
                {
                    MessageBox.Show("Schedule entry saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to save schedule entry. There may be a database error.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Duplicate EDP Code check
        private bool EDPCodeExists(string edpCode)
        {
            try
            {
                using (var connection = new System.Data.OleDb.OleDbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM SubjectSchedFile WHERE SSFEDPCODE = ?";
                    using (var command = new System.Data.OleDb.OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", System.Data.OleDb.OleDbType.VarChar).Value = edpCode;
                        int count = 0;
                        var result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out count))
                        {
                            return count > 0;
                        }
                        return false;
                    }
                }
            }
            catch
            {
                // If error, assume not duplicate to allow error to be caught on insert
                return false;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            EDPCodeTextBox.Clear();
            SubjectCodeTextBox.Clear();
            DescriptionTextBox.Clear();
            StartTimeDTP.Value = DateTime.Now;
            EndTimeDTP.Value = DateTime.Now;
            DaysTextBox.Clear();
            SectionTextBox.Clear();
            SchoolYearTextBox.Clear();
            XMComboBox.SelectedIndex = -1;
        }
    }
}
