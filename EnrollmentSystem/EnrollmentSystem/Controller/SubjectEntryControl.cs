using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using EnrollmentSystem.Repo;

namespace EnrollmentSystem.Controller
{
    public partial class SubjectEntryControl : UserControl
    {
        private SubjectEntryData repo;

        public SubjectEntryControl()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["EnrollmentAccessDB"].ConnectionString;
            repo = new SubjectEntryData(connectionString);

            // SaveButton.Click += SaveButton_Click; // Removed to avoid double event hookup
            CancelButton.Click += CancelButton_Click;
            RequisiteSubjectCodeTextBox.KeyDown += RequisiteSubjectCodeTextBox_KeyDown;
            PrerequisiteRadioButton.CheckedChanged += RequisiteRadioButton_CheckedChanged;
            CorequisiteRadioButton.CheckedChanged += RequisiteRadioButton_CheckedChanged;
            RequisiteDataGridView.SelectionChanged += RequisiteDataGridView_SelectionChanged;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Validate subject info
            if (string.IsNullOrWhiteSpace(SubjectCodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(UnitsTextBox.Text) ||
                OfferingComboBox.SelectedItem == null ||
                CategoryComboBox.SelectedItem == null ||
                CourseCodeComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(CurriculumYearTextBox.Text))
            {
                MessageBox.Show("Please fill in all subject information fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(UnitsTextBox.Text, out double units))
            {
                MessageBox.Show("Units must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(OfferingComboBox.SelectedItem.ToString(), out int offering))
            {
                MessageBox.Show("Offering must be a valid number (e.g., 1 for 1st sem, 2 for 2nd sem).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for duplicate subject code
            if (repo.IsSubjectCodeExists(SubjectCodeTextBox.Text.Trim()))
            {
                MessageBox.Show("Subject code already exists. Please enter a different code.", "Duplicate Subject Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SubjectCodeTextBox.Focus();
                return;
            }

            // Add subject
            bool subjectAdded = repo.AddSubject(
                SubjectCodeTextBox.Text.Trim(),
                DescriptionTextBox.Text.Trim(),
                units,
                offering,
                CategoryComboBox.SelectedItem.ToString(),
                "AC", // Default to active
                CourseCodeComboBox.SelectedItem.ToString(),
                CurriculumYearTextBox.Text.Trim()
            );

            if (!subjectAdded)
            {
                // Error message already shown in repo
                return;
            }

            // Add requisites if any
            bool allRequisitesAdded = true;
            foreach (DataGridViewRow row in RequisiteDataGridView.Rows)
            {
                if (row.IsNewRow) continue;
                string preqCode = row.Cells["SubjectCodeColumn"].Value?.ToString();
                string requisiteType = row.Cells["RequisiteColumn"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(preqCode) || string.IsNullOrWhiteSpace(requisiteType))
                    continue; // skip incomplete

                bool reqAdded = repo.AddRequisite(
                    SubjectCodeTextBox.Text.Trim(),
                    preqCode,
                    requisiteType
                );
                if (!reqAdded) allRequisitesAdded = false;
            }

            if (allRequisitesAdded)
                MessageBox.Show("Subject and requisites saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Subject saved, but some requisites may not have been saved.", "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Optionally clear fields
            ClearAllFields();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            ClearAllFields();
        }

        private void RequisiteSubjectCodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string code = RequisiteSubjectCodeTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(code))
                {
                    MessageBox.Show("Please enter a subject code.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataRow subject = repo.FindSubjectByCode(code);
                if (subject == null)
                {
                    MessageBox.Show("Subject code not found.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check for duplicates in grid
                foreach (DataGridViewRow row in RequisiteDataGridView.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (row.Cells["SubjectCodeColumn"].Value?.ToString() == code)
                    {
                        MessageBox.Show("This subject is already in the requisites list.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Add to grid: code, desc, units, blank requisite type
                RequisiteDataGridView.Rows.Add(
                    subject["SFSUBJCODE"].ToString(),
                    subject["SFSUBJDESC"].ToString(),
                    subject["SFSUBJUNITS"].ToString(),
                    "" // RequisiteColumn
                );
                RequisiteSubjectCodeTextBox.Clear();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void RequisiteRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (RequisiteDataGridView.SelectedRows.Count == 0)
                return;

            var selectedRow = RequisiteDataGridView.SelectedRows[0];
            if (selectedRow.IsNewRow) return;

            if (PrerequisiteRadioButton.Checked)
                selectedRow.Cells["RequisiteColumn"].Value = "PR";
            else if (CorequisiteRadioButton.Checked)
                selectedRow.Cells["RequisiteColumn"].Value = "CR";
        }

        private void RequisiteDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (RequisiteDataGridView.SelectedRows.Count == 0)
                return;

            var selectedRow = RequisiteDataGridView.SelectedRows[0];
            if (selectedRow.IsNewRow) return;

            string type = selectedRow.Cells["RequisiteColumn"].Value?.ToString();
            if (type == "PR")
                PrerequisiteRadioButton.Checked = true;
            else if (type == "CR")
                CorequisiteRadioButton.Checked = true;
            else
            {
                PrerequisiteRadioButton.Checked = false;
                CorequisiteRadioButton.Checked = false;
            }
        }

        private void ClearAllFields()
        {
            SubjectCodeTextBox.Clear();
            DescriptionTextBox.Clear();
            UnitsTextBox.Clear();
            OfferingComboBox.SelectedIndex = -1;
            CategoryComboBox.SelectedIndex = -1;
            CourseCodeComboBox.SelectedIndex = -1;
            CurriculumYearTextBox.Clear();
            RequisiteSubjectCodeTextBox.Clear();
            RequisiteDataGridView.Rows.Clear();
            PrerequisiteRadioButton.Checked = false;
            CorequisiteRadioButton.Checked = false;
        }
    }
}
