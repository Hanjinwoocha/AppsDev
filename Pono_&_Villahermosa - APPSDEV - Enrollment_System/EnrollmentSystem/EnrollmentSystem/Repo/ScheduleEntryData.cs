using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;

namespace EnrollmentSystem.Repo
{
    internal class ScheduleEntryData
    {
        private readonly string connectionString;
        public ScheduleEntryData(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public string GetSubjectDescription(string subjectCode)
        {
            string description = "";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SFSUBJDESC FROM SubjectFile WHERE SFSUBJCODE = ?";
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.Add("?", OleDbType.VarChar).Value = subjectCode;
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        description = result.ToString();
                    }
                }
            }
            return description;
        }

        public bool InsertScheduleEntry(string edpCode, string subjectCode, TimeSpan startTime, 
            TimeSpan endTime, string days, string section, string schoolYear, string xm)
        {
            try
            {
                // Check for overlapping schedules first
                if (HasOverlappingSchedule(subjectCode, startTime, endTime, days, schoolYear))
                {
                    System.Windows.Forms.MessageBox.Show("This schedule overlaps with an existing schedule for the same subject.", "Schedule Conflict", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO SubjectSchedFile 
                        (SSFEDPCODE, SSFSUBJCODE, SSFSTARTTIME, SSFENDTIME, SSFDAYS, 
                        SSFSECTION, SSFSCHOOLYEAR, SSFXM, SSFMAXSIZE, SSFCLASSSIZE, SSFSTATUS) 
                        VALUES 
                        (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.Add("?", OleDbType.VarChar).Value = edpCode;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = subjectCode;
                        command.Parameters.Add("?", OleDbType.DBTime).Value = startTime;
                        command.Parameters.Add("?", OleDbType.DBTime).Value = endTime;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = days;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = section;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = schoolYear;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = xm;
                        command.Parameters.Add("?", OleDbType.Integer).Value = 50;
                        command.Parameters.Add("?", OleDbType.Integer).Value = 0;
                        command.Parameters.Add("?", OleDbType.VarChar).Value = "AC";

                        command.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        private bool HasOverlappingSchedule(string subjectCode, TimeSpan startTime, TimeSpan endTime, string days, string schoolYear)
        {
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT SSFSTARTTIME, SSFENDTIME, SSFDAYS 
                               FROM SubjectSchedFile 
                               WHERE SSFSUBJCODE = ? AND SSFSCHOOLYEAR = ?";
                
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.Add("?", OleDbType.VarChar).Value = subjectCode;
                    command.Parameters.Add("?", OleDbType.VarChar).Value = schoolYear;
                    
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TimeSpan existingStart = (TimeSpan)reader["SSFSTARTTIME"];
                            TimeSpan existingEnd = (TimeSpan)reader["SSFENDTIME"];
                            string existingDays = reader["SSFDAYS"].ToString();

                            // Check if days overlap
                            if (DoDaysOverlap(days, existingDays))
                            {
                                // Check if times overlap
                                if (DoTimesOverlap(startTime, endTime, existingStart, existingEnd))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool DoDaysOverlap(string days1, string days2)
        {
            var days1Set = new HashSet<char>(days1.ToUpper());
            var days2Set = new HashSet<char>(days2.ToUpper());
            return days1Set.Overlaps(days2Set);
        }

        private bool DoTimesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }
    }
}
