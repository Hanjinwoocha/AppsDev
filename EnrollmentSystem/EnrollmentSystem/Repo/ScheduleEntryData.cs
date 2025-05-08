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
    }
}
