using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace EnrollmentSystem.Repo
{
    internal class EnrollmentEntryData
    {
        private readonly string connectionString;
        public EnrollmentEntryData(string connectionString)
        {
            this.connectionString = connectionString;
        }
        
        // 1. Fetch student info by ID
        public StudentInfo GetStudentInfo(int studentId)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT STFSTUDFNAME, STFSTUDMNAME, STFSTUDLNAME, STFSTUDCOURSE, STFSTUDYEAR FROM StudentFile WHERE STFSTUDID = ?";
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@STFSTUDID", studentId);
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StudentInfo
                            {
                                FirstName = reader["STFSTUDFNAME"].ToString(),
                                MiddleName = reader["STFSTUDMNAME"].ToString(),
                                LastName = reader["STFSTUDLNAME"].ToString(),
                                Course = reader["STFSTUDCOURSE"].ToString(),
                                Year = reader["STFSTUDYEAR"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        // 2. Fetch subject schedule and units by EDP code
        public SubjectScheduleInfo GetSubjectScheduleInfoByEDP(string edpCode)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                // Get schedule info from SubjectSchedFile by EDP code
                string schedQuery = "SELECT * FROM SubjectSchedFile WHERE SSFEDPCODE = ?";
                using (OleDbCommand schedCmd = new OleDbCommand(schedQuery, conn))
                {
                    schedCmd.Parameters.AddWithValue("@SSFEDPCODE", edpCode);
                    using (OleDbDataReader schedReader = schedCmd.ExecuteReader())
                    {
                        if (schedReader.Read())
                        {
                            string subjCode = schedReader["SSFSUBJCODE"].ToString();
                            // Get units from SubjectFile
                            string unitsQuery = "SELECT SFSUBJUNITS FROM SubjectFile WHERE SFSUBJCODE = ?";
                            using (OleDbCommand unitsCmd = new OleDbCommand(unitsQuery, conn))
                            {
                                unitsCmd.Parameters.AddWithValue("@SFSUBJCODE", subjCode);
                                var unitsResult = unitsCmd.ExecuteScalar();
                                double units = 0;
                                if (unitsResult != null) double.TryParse(unitsResult.ToString(), out units);
                                return new SubjectScheduleInfo
                                {
                                    EDPCode = schedReader["SSFEDPCODE"].ToString(),
                                    SubjectCode = subjCode,
                                    StartTime = schedReader["SSFSTARTTIME"].ToString(),
                                    EndTime = schedReader["SSFENDTIME"].ToString(),
                                    Days = schedReader["SSFDAYS"].ToString(),
                                    Section = schedReader["SSFSECTION"].ToString(),
                                    SchoolYear = schedReader["SSFSCHOOLYEAR"].ToString(),
                                    XM = schedReader["SSFXM"].ToString(),
                                    Units = units
                                };
                            }
                        }
                    }
                }
            }
            return null;
        }

        // 3. Check pre-requisites and grade
        public bool CanEnrollInSubject(int studentId, string subjCode, out string message)
        {
            message = "";
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                // Check if subject has a pre-req
                string preqQuery = "SELECT SUBJPRECODE FROM SubjectPreqFile WHERE SUBJCODE = ? AND SUBJCATEGORY = 'PR'";
                using (OleDbCommand preqCmd = new OleDbCommand(preqQuery, conn))
                {
                    preqCmd.Parameters.AddWithValue("@SUBJCODE", subjCode);
                    var preqResult = preqCmd.ExecuteScalar();
                    if (preqResult != null && preqResult != DBNull.Value)
                    {
                        string preqCode = preqResult.ToString();
                        // Check grade for pre-req
                        string gradeQuery = "SELECT SGFSTUDREMARKS FROM StudentGradeFile WHERE SGFSTUDID = ? AND SGFSTUDSUBJCODE = ?";
                        using (OleDbCommand gradeCmd = new OleDbCommand(gradeQuery, conn))
                        {
                            gradeCmd.Parameters.AddWithValue("@SGFSTUDID", studentId);
                            gradeCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", preqCode);
                            var gradeResult = gradeCmd.ExecuteScalar();
                            if (gradeResult != null && gradeResult.ToString().ToUpper() == "FAILED")
                            {
                                message = $"Cannot enroll: prerequisite subject ({preqCode}) was FAILED.";
                                return false;
                            }
                        }
                    }
                }
                // If no pre-req or pre-req passed, allow
                return true;
            }
        }

        // 4. Save enrollment header and details
        public bool SaveEnrollment(int studentId, DateTime dateEnrolled, string schoolYear, string encoder, int totalUnits, List<EnrollmentDetailRow> details)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                conn.Open();
                using (OleDbTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Check for time conflicts first
                        foreach (var detail in details)
                        {
                            if (HasTimeConflict(studentId, detail.EDPCode, conn, transaction))
                            {
                                throw new Exception("Time conflict detected with existing enrolled subjects.");
                            }
                        }

                        // Check if enrollment exists for this student and school year
                        string checkQuery = "SELECT COUNT(*) FROM EnrollmentHeaderFile WHERE ENRHFSTUDID = ? AND ENRHFSTUDSCHLYR = ?";
                        using (OleDbCommand checkCmd = new OleDbCommand(checkQuery, conn, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@ENRHFSTUDID", studentId);
                            checkCmd.Parameters.AddWithValue("@ENRHFSTUDSCHLYR", schoolYear);
                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (count > 0)
                            {
                                // Update existing enrollment
                                string updateQuery = "UPDATE EnrollmentHeaderFile SET ENRHFSTUDDATEENROLL = ?, ENRHFSTUDENCODER = ?, ENRHFSTUDTOTALUNITS = ? WHERE ENRHFSTUDID = ? AND ENRHFSTUDSCHLYR = ?";
                                using (OleDbCommand updateCmd = new OleDbCommand(updateQuery, conn, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@ENRHFSTUDDATEENROLL", dateEnrolled);
                                    updateCmd.Parameters.AddWithValue("@ENRHFSTUDENCODER", encoder);
                                    updateCmd.Parameters.AddWithValue("@ENRHFSTUDTOTALUNITS", totalUnits);
                                    updateCmd.Parameters.AddWithValue("@ENRHFSTUDID", studentId);
                                    updateCmd.Parameters.AddWithValue("@ENRHFSTUDSCHLYR", schoolYear);
                                    updateCmd.ExecuteNonQuery();
                                }

                                // Delete existing details
                                string deleteQuery = "DELETE FROM EnrollmentDetailFile WHERE ENRDFSTUDID = ? AND ENRDFSTUDSUBJCODE IN (SELECT ENRDFSTUDSUBJCODE FROM EnrollmentDetailFile WHERE ENRDFSTUDID = ?)";
                                using (OleDbCommand deleteCmd = new OleDbCommand(deleteQuery, conn, transaction))
                                {
                                    deleteCmd.Parameters.AddWithValue("@ENRDFSTUDID", studentId);
                                    deleteCmd.Parameters.AddWithValue("@ENRDFSTUDID2", studentId);
                                    deleteCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Insert new enrollment
                                string insertQuery = "INSERT INTO EnrollmentHeaderFile (ENRHFSTUDID, ENRHFSTUDDATEENROLL, ENRHFSTUDSCHLYR, ENRHFSTUDENCODER, ENRHFSTUDTOTALUNITS, ENRHFSTUDSTATUS) VALUES (?, ?, ?, ?, ?, ?)";
                                using (OleDbCommand insertCmd = new OleDbCommand(insertQuery, conn, transaction))
                                {
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDID", studentId);
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDDATEENROLL", dateEnrolled);
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDSCHLYR", schoolYear);
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDENCODER", encoder);
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDTOTALUNITS", totalUnits);
                                    insertCmd.Parameters.AddWithValue("@ENRHFSTUDSTATUS", "EN");
                                    insertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Save details and update class size
                        foreach (var row in details)
                        {
                            string detailQuery = "INSERT INTO EnrollmentDetailFile (ENRDFSTUDID, ENRDFSTUDSUBJCODE, ENRDFSTUDEDPCODE, ENRDFSTUDSTATUS) VALUES (?, ?, ?, ?)";
                            using (OleDbCommand detailCmd = new OleDbCommand(detailQuery, conn, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@ENRDFSTUDID", studentId);
                                detailCmd.Parameters.AddWithValue("@ENRDFSTUDSUBJCODE", row.SubjectCode);
                                detailCmd.Parameters.AddWithValue("@ENRDFSTUDEPCODE", row.EDPCode);
                                detailCmd.Parameters.AddWithValue("@ENRDFSTUDSTATUS", "EN");
                                detailCmd.ExecuteNonQuery();
                            }

                            // Update class size and check if max size reached
                            string updateClassSizeQuery = @"
                                UPDATE SubjectSchedFile 
                                SET SSFCLASSSIZE = SSFCLASSSIZE + 1,
                                    SSFSTATUS = IIF((SSFCLASSSIZE + 1) >= 50 OR (SSFCLASSSIZE + 1) >= SSFMAXSIZE, 'CLO', SSFSTATUS)
                                WHERE SSFEDPCODE = ?";
                            using (OleDbCommand updateClassSizeCmd = new OleDbCommand(updateClassSizeQuery, conn, transaction))
                            {
                                updateClassSizeCmd.Parameters.AddWithValue("@SSFEDPCODE", row.EDPCode);
                                updateClassSizeCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private TimeSpan ParseTime(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                throw new FormatException("Time value is null or empty.");
            string str = dbValue.ToString().Trim();
            if (string.IsNullOrEmpty(str))
                throw new FormatException("Time value is empty.");

            // Try parsing as TimeSpan first
            if (TimeSpan.TryParse(str, out TimeSpan ts))
                return ts;
            // Try parsing as DateTime and get TimeOfDay
            if (DateTime.TryParse(str, out DateTime dt))
                return dt.TimeOfDay;
            // Try parsing as HHmm (e.g., 800, 1330)
            if (int.TryParse(str, out int intTime))
            {
                int hour = intTime / 100;
                int min = intTime % 100;
                return new TimeSpan(hour, min, 0);
            }
            throw new FormatException($"Unrecognized time format: '{str}'");
        }

        private bool HasTimeConflict(int studentId, string newEDPCode, OleDbConnection conn, OleDbTransaction transaction)
        {
            // Get the schedule of the new subject
            string newScheduleQuery = "SELECT SSFSTARTTIME, SSFENDTIME, SSFDAYS FROM SubjectSchedFile WHERE SSFEDPCODE = ?";
            TimeSpan newStartTime, newEndTime;
            string newDays;
            
            using (OleDbCommand cmd = new OleDbCommand(newScheduleQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@SSFEDPCODE", newEDPCode);
                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    newStartTime = ParseTime(reader["SSFSTARTTIME"]);
                    newEndTime = ParseTime(reader["SSFENDTIME"]);
                    newDays = reader["SSFDAYS"].ToString();
                }
            }

            // Get all enrolled subjects for the student
            string enrolledQuery = @"
                SELECT DISTINCT s.SSFSTARTTIME, s.SSFENDTIME, s.SSFDAYS, s.SSFSUBJCODE 
                FROM SubjectSchedFile s
                INNER JOIN EnrollmentDetailFile e ON s.SSFEDPCODE = e.ENRDFSTUDEDPCODE
                WHERE e.ENRDFSTUDID = ? AND e.ENRDFSTUDSTATUS = 'EN'";

            using (OleDbCommand cmd = new OleDbCommand(enrolledQuery, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ENRDFSTUDID", studentId);
                using (OleDbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string subjCode = reader["SSFSUBJCODE"].ToString();
                        // Check if this subject is already graded
                        string gradeQuery = "SELECT SGFSTUDREMARKS FROM StudentGradeFile WHERE SGFSTUDID = ? AND SGFSTUDSUBJCODE = ?";
                        using (OleDbCommand gradeCmd = new OleDbCommand(gradeQuery, conn, transaction))
                        {
                            gradeCmd.Parameters.AddWithValue("@SGFSTUDID", studentId);
                            gradeCmd.Parameters.AddWithValue("@SGFSTUDSUBJCODE", subjCode);
                            var gradeResult = gradeCmd.ExecuteScalar();
                            if (gradeResult != null && gradeResult.ToString().ToUpper() != "NOT GRADED")
                            {
                                // Already graded, skip this subject for conflict
                                continue;
                            }
                        }

                        TimeSpan existingStartTime = ParseTime(reader["SSFSTARTTIME"]);
                        TimeSpan existingEndTime = ParseTime(reader["SSFENDTIME"]);
                        string existingDays = reader["SSFDAYS"].ToString();

                        // Check if days overlap
                        if (DoDaysOverlap(newDays, existingDays))
                        {
                            // Check if time overlaps
                            if (DoTimesOverlap(newStartTime, newEndTime, existingStartTime, existingEndTime))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private bool DoDaysOverlap(string days1, string days2)
        {
            // Split days into individual characters
            var days1Set = new HashSet<char>(days1.ToUpper());
            var days2Set = new HashSet<char>(days2.ToUpper());
            
            // Check if there's any common day
            return days1Set.Overlaps(days2Set);
        }

        private bool DoTimesOverlap(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
        {
            return start1 < end2 && start2 < end1;
        }
    }

    // Helper data classes
    public class StudentInfo
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Course { get; set; }
        public string Year { get; set; }
    }
    public class SubjectScheduleInfo
    {
        public string EDPCode { get; set; }
        public string SubjectCode { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Days { get; set; }
        public string Section { get; set; }
        public string SchoolYear { get; set; }
        public string XM { get; set; }
        public double Units { get; set; }
    }
    public class EnrollmentDetailRow
    {
        public string SubjectCode { get; set; }
        public string EDPCode { get; set; }
    }
}
