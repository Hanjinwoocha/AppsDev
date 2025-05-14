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
        private readonly EnrollmentEntryData repo;
        private const string EDP_CODE_COLUMN = "EDPCodeColumn";
        private const string SUBJECT_CODE_COLUMN = "SubjectCodeColumn";
        private const string UNITS_COLUMN = "UnitsColumn";

        public EnrollmentEntryControl()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            repo = new EnrollmentEntryData(connectionString);
        }

        /// <summary>
        /// Handles key press events for the ID Number textbox, allowing only numeric input
        /// and fetching student information on Enter key press.
        /// </summary>
        private void IDNumberTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // On Enter, fetch and display student info
            if (e.KeyChar == (char)Keys.Enter)
            {
                ProcessStudentId();
            }
        }

        /// <summary>
        /// Handles key press events for the EDP Code textbox, allowing alphanumeric input
        /// and processing the EDP code on Enter key press.
        /// </summary>
        private void EDPCodeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers and letters only
            if (!char.IsControl(e.KeyChar) && !char.IsLetterOrDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // On Enter, process EDP code
            if (e.KeyChar == (char)Keys.Enter)
            {
                string edpCode = EDPCodeTextBox.Text.Trim();
                if (string.IsNullOrEmpty(edpCode))
                {
                    return;
                }

                if (!ValidateStudentId(IDNumberTextBox.Text, out int studentId))
                {
                    ShowWarning("Enter a valid Student ID first.");
                    return;
                }

                // Check if already enrolled in this subject using repo
                if (repo.IsStudentAlreadyEnrolledInEDP(studentId, edpCode))
                {
                    MessageBox.Show("This student is already enrolled in the subject with this EDP code.", "Duplicate Enrollment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    EDPCodeTextBox.Clear();
                    return;
                }

                ProcessEDPCode();
            }
        }

        /// <summary>
        /// Processes the student ID, fetching and displaying student information if valid.
        /// </summary>
        private void ProcessStudentId()
        {
            if (!ValidateStudentId(IDNumberTextBox.Text, out int studentId))
            {
                return;
            }

            var info = repo.GetStudentInfo(studentId);
            DisplayStudentInfo(info);
        }

        /// <summary>
        /// Processes the EDP code, validating and adding the subject to the grid if valid.
        /// </summary>
        private void ProcessEDPCode()
        {
            string edpCode = EDPCodeTextBox.Text.Trim();
            if (string.IsNullOrEmpty(edpCode))
            {
                return;
            }

            if (!ValidateStudentId(IDNumberTextBox.Text, out int studentId))
            {
                ShowWarning("Enter a valid Student ID first.");
                return;
            }

            var sched = repo.GetSubjectScheduleInfoByEDP(edpCode);
            if (sched == null)
            {
                ShowError("EDP code not found in SubjectSchedFile.");
                return;
            }

            if (!ValidatePrerequisites(studentId, sched.SubjectCode))
            {
                return;
            }

            if (IsSubjectAlreadyAdded(sched.EDPCode))
            {
                ShowWarning("Subject already added.", "Duplicate");
                return;
            }

            AddSubjectToGrid(sched);
            EDPCodeTextBox.Clear();
        }

        /// <summary>
        /// Validates the student ID format and parses it to an integer.
        /// </summary>
        private bool ValidateStudentId(string studentId, out int parsedId)
        {
            if (!int.TryParse(studentId.Trim(), out parsedId))
            {
                ShowWarning("Please enter a valid numeric Student ID.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates if the student meets the prerequisites for a subject.
        /// </summary>
        private bool ValidatePrerequisites(int studentId, string subjectCode)
        {
            if (!repo.CanEnrollInSubject(studentId, subjectCode, out string preqMsg))
            {
                ShowWarning(preqMsg, "Pre-requisite Not Met");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Displays student information in the form controls.
        /// </summary>
        private void DisplayStudentInfo(StudentInfo info)
        {
            if (info != null)
            {
                NameTextBox.Text = $"{info.LastName}, {info.FirstName} {info.MiddleName}";
                CourseTextBox.Text = info.Course;
                YearTextBox.Text = info.Year;
            }
            else
            {
                ShowError("Student not found.");
                ClearStudentInfo();
            }
        }

        /// <summary>
        /// Clears all student information fields.
        /// </summary>
        private void ClearStudentInfo()
        {
            NameTextBox.Clear();
            CourseTextBox.Clear();
            YearTextBox.Clear();
        }

        /// <summary>
        /// Checks if a subject is already added to the grid.
        /// </summary>
        private bool IsSubjectAlreadyAdded(string edpCode)
        {
            return EnrolledSubjectsDataGridView.Rows.Cast<DataGridViewRow>()
                .Any(row => !row.IsNewRow && row.Cells[EDP_CODE_COLUMN].Value?.ToString() == edpCode);
        }

        /// <summary>
        /// Adds a subject to the grid and updates the total units.
        /// </summary>
        private void AddSubjectToGrid(SubjectScheduleInfo sched)
        {
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
        }

        /// <summary>
        /// Updates the total units display based on the subjects in the grid.
        /// </summary>
        private void UpdateTotalUnits()
        {
            double total = EnrolledSubjectsDataGridView.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow)
                .Sum(row => double.TryParse(row.Cells[UNITS_COLUMN].Value?.ToString(), out double units) ? units : 0);
            
            TotalUnitsTextBox.Text = total.ToString();
        }

        /// <summary>
        /// Handles the save button click event, validating and saving the enrollment.
        /// </summary>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateEnrollmentDetails(out int studentId, out string encoder, out string schoolYear, out int totalUnits))
            {
                return;
            }

            var details = CollectEnrollmentDetails();
            if (details == null)
            {
                return;
            }

            try
            {
                SaveEnrollment(studentId, DateEnrolledDTP.Value, schoolYear, encoder, totalUnits, details);
            }
            catch (Exception ex)
            {
                HandleSaveError(ex);
            }
        }

        /// <summary>
        /// Validates all enrollment details before saving.
        /// </summary>
        private bool ValidateEnrollmentDetails(out int studentId, out string encoder, out string schoolYear, out int totalUnits)
        {
            studentId = 0;
            encoder = string.Empty;
            schoolYear = string.Empty;
            totalUnits = 0;

            if (!ValidateStudentId(IDNumberTextBox.Text, out studentId))
            {
                return false;
            }

            if (EncoderComboBox.SelectedItem == null)
            {
                ShowWarning("Select an encoder.");
                return false;
            }
            encoder = EncoderComboBox.Text.Trim();

            if (SchoolYearComboBox.SelectedItem == null)
            {
                ShowWarning("Select a school year.");
                return false;
            }
            schoolYear = SchoolYearComboBox.SelectedItem.ToString();

            if (!int.TryParse(TotalUnitsTextBox.Text, out totalUnits) )
            {
                ShowWarning("Total units must be greater than 0.");
                return false;
            }

            if (EnrolledSubjectsDataGridView.Rows.Count == 0 || 
                EnrolledSubjectsDataGridView.Rows.Cast<DataGridViewRow>().All(r => r.IsNewRow))
            {
                ShowWarning("No subjects to enroll.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Collects enrollment details from the grid.
        /// </summary>
        private List<EnrollmentDetailRow> CollectEnrollmentDetails()
        {
            var details = new List<EnrollmentDetailRow>();
            foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                
                var subjCode = row.Cells[SUBJECT_CODE_COLUMN].Value?.ToString();
                var edpCode = row.Cells[EDP_CODE_COLUMN].Value?.ToString();
                
                if (string.IsNullOrWhiteSpace(subjCode) || string.IsNullOrWhiteSpace(edpCode))
                {
                    ShowWarning("One or more subject details are missing. Please check the grid.");
                    return null;
                }
                
                details.Add(new EnrollmentDetailRow
                {
                    SubjectCode = subjCode,
                    EDPCode = edpCode
                });
            }
            return details;
        }

        /// <summary>
        /// Saves the enrollment and handles the success case.
        /// </summary>
        private void SaveEnrollment(int studentId, DateTime dateEnrolled, string schoolYear, 
            string encoder, int totalUnits, List<EnrollmentDetailRow> details)
        {
            repo.SaveEnrollment(studentId, dateEnrolled, schoolYear, encoder, totalUnits, details);
            ShowSuccess("Enrollment saved successfully!");
            EnrolledSubjectsDataGridView.Rows.Clear();
            UpdateTotalUnits();
        }

        /// <summary>
        /// Handles errors that occur during enrollment saving.
        /// </summary>
        private void HandleSaveError(Exception ex)
        {
            if (ex.Message.Contains("Time conflict"))
            {
                ShowWarning("Cannot enroll: Time conflict detected with existing enrolled subjects. Please check the schedule.", "Time Conflict");
            }
            else
            {
                ShowError($"Error saving enrollment: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the remove button click event, removing selected subjects from the grid.
        /// </summary>
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (EnrolledSubjectsDataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in EnrolledSubjectsDataGridView.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        EnrolledSubjectsDataGridView.Rows.Remove(row);
                    }
                }
                UpdateTotalUnits();
            }
            else
            {
                ShowWarning("Select a row to remove.", "Remove");
            }
        }

        #region Message Display Methods
        private void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void ViewSubjectListButton_Click(object sender, EventArgs e)
        {
            SubjectList subjectList = new SubjectList();
            subjectList.ShowDialog();
        }
    }
}
