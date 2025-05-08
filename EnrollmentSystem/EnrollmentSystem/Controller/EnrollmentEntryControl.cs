using System;
using System.Collections.Generic;
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
    public partial class EnrollmentEntryControl : UserControl
    {
        private EnrollmentEntryData repo;
        public EnrollmentEntryControl()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            repo = new EnrollmentEntryData(connectionString);
        }

        private void IDNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // On Enter, fetch and display student info
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (int.TryParse(IDNumberTextBox.Text.Trim(), out int studentId))
                {
                    var info = repo.GetStudentInfo(studentId);
                    if (info != null)
                    {
                        NameTextBox.Text = $"{info.LastName}, {info.FirstName} {info.MiddleName}";
                        CourseTextBox.Text = info.Course;
                        YearTextBox.Text = info.Year;
                    }
                    else
                    {
                        MessageBox.Show("Student not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        NameTextBox.Clear();
                        CourseTextBox.Clear();
                        YearTextBox.Clear();
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric Student ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void EDPCodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers and letters only
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            // On Enter, fetch subject schedule and units, check pre-reqs, add to grid
            if (e.KeyChar == (char)Keys.Enter)
            {
                string edpCode = EDPCodeTextBox.Text.Trim();
                if (string.IsNullOrEmpty(edpCode)) return;
                if (!int.TryParse(IDNumberTextBox.Text.Trim(), out int studentId))
                {
                    MessageBox.Show("Enter a valid Student ID first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Fetch subject schedule and units by EDP code
                var sched = repo.GetSubjectScheduleInfoByEDP(edpCode);
                if (sched == null)
                {
                    MessageBox.Show("EDP code not found in SubjectSchedFile.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Check pre-reqs using the subject code from the schedule
                if (!repo.CanEnrollInSubject(studentId, sched.SubjectCode, out string preqMsg))
                {
                    MessageBox.Show(preqMsg, "Pre-requisite Not Met", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Check for duplicate in grid
                foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.Rows)
                {
                    if (!row.IsNewRow && row.Cells["EDPCodeColumn"].Value?.ToString() == sched.EDPCode)
                    {
                        MessageBox.Show("Subject already added.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                // Add to grid
                EnrolledSubjectsDataGridView.Rows.Add(
                    sched.EDPCode,
                    sched.SubjectCode,
                    sched.StartTime,
                    sched.EndTime,
                    sched.Days,
                    sched.Section,
                    sched.Units
                );
                UpdateTotalUnits();
                EDPCodeTextBox.Clear();
            }
        }

        private void UpdateTotalUnits()
        {
            double total = 0;
            foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.Rows)
            {
                if (!row.IsNewRow && double.TryParse(row.Cells["UnitsColumn"].Value?.ToString(), out double units))
                {
                    total += units;
                }
            }
            TotalUnitsTextBox.Text = total.ToString();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(IDNumberTextBox.Text.Trim(), out int studentId))
            {
                MessageBox.Show("Enter a valid Student ID.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (EncoderComboBox.SelectedItem == null)
            {
                MessageBox.Show("Select an encoder.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (EnrolledSubjectsDataGridView.Rows.Count == 0 || EnrolledSubjectsDataGridView.Rows.Cast<DataGridViewRow>().All(r => r.IsNewRow))
            {
                MessageBox.Show("No subjects to enroll.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string encoder = EncoderComboBox.Text.Trim();
            DateTime dateEnrolled = DateEnrolledDTP.Value;
            if (SchoolYearComboBox.SelectedItem == null)
            {
                MessageBox.Show("Select a school year.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string schoolYear = SchoolYearComboBox.SelectedItem.ToString();
            if (!int.TryParse(TotalUnitsTextBox.Text, out int totalUnits) || totalUnits <= 0)
            {
                MessageBox.Show("Total units must be greater than 0.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var details = new List<EnrollmentDetailRow>();
            foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                var subjCode = row.Cells["SubjectCodeColumn"].Value?.ToString();
                var edpCode = row.Cells["EDPCodeColumn"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(subjCode) || string.IsNullOrWhiteSpace(edpCode))
                {
                    MessageBox.Show("One or more subject details are missing. Please check the grid.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                details.Add(new EnrollmentDetailRow
                {
                    SubjectCode = subjCode,
                    EDPCode = edpCode
                });
            }
            try
            {
                repo.SaveEnrollment(studentId, dateEnrolled, schoolYear, encoder, totalUnits, details);
                MessageBox.Show("Enrollment saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                EnrolledSubjectsDataGridView.Rows.Clear();
                UpdateTotalUnits();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Time conflict"))
                {
                    MessageBox.Show("Cannot enroll: Time conflict detected with existing enrolled subjects. Please check the schedule.", "Time Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Error saving enrollment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (EnrolledSubjectsDataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.SelectedRows)
                {
                    if (!row.IsNewRow)
                        EnrolledSubjectsDataGridView.Rows.Remove(row);
                }
                UpdateTotalUnits();
            }
            else
            {
                MessageBox.Show("Select a row to remove.", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
