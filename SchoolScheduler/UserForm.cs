using System;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class UserForm : Form
    {
        private string ClassName;
        private DataGridView dgvSchedule;
        private Button btnExit;
        private Schedule currentSchedule;
        private string dbPath = null;

        public UserForm(string className)
        {
            ClassName = className;

            Text = $"Расписание для класса {ClassName}";
            Width = 950;
            Height = 650;
            StartPosition = FormStartPosition.CenterScreen;

            dgvSchedule = new DataGridView
            {
                Left = 10,
                Top = 10,
                Width = 910,
                Height = 550,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = true,
                RowHeadersWidth = 80,
                ColumnHeadersHeight = 40,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            };

            var colLesson = new DataGridViewTextBoxColumn
            {
                Name = "Lesson",
                HeaderText = "Урок",
                ReadOnly = true,
                Width = 60
            };
            dgvSchedule.Columns.Add(colLesson);

            string[] days = { "Пн", "Вт", "Ср", "Чт", "Пт" };
            foreach (var day in days)
                dgvSchedule.Columns.Add(day, day);

            for (int i = 1; i <= 8; i++)
                dgvSchedule.Rows.Add(i.ToString());

            btnExit = new Button
            {
                Text = "Выход",
                Left = 10,
                Top = 570,
                Width = 100
            };
            btnExit.Click += (s, e) => Close();

            Controls.Add(dgvSchedule);
            Controls.Add(btnExit);

            LoadDatabasePath();
            LoadSchedule();
        }

        private void LoadDatabasePath()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "SQLite Database (*.sqlite;*.db)|*.sqlite;*.db|All files (*.*)|*.*";
                ofd.Title = "Выберите базу данных с расписанием";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    dbPath = ofd.FileName;
                }
                else
                {
                    MessageBox.Show("База данных не выбрана. Закрываем программу.");
                    Close();
                }
            }
        }

        private void LoadSchedule()
        {
            if (dbPath == null) return;

            currentSchedule = new Schedule();
            currentSchedule.Lessons.Clear();

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT Subject, Teacher, Room, LessonsCount FROM LessonsPlan WHERE Class = @class";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@class", ClassName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subject = reader.GetString(0);
                            string teacher = reader.GetString(1);
                            string room = reader.GetString(2);
                            int lessonsCount = reader.GetInt32(3);

                            DistributeLessons(subject, teacher, room, lessonsCount);
                        }
                    }
                }
            }

            DisplaySchedule();
        }

        // Новый алгоритм распределения уроков: сначала урок 1 на все дни, потом урок 2 и т.д.
        private void DistributeLessons(string subject, string teacher, string room, int lessonsCount)
        {
            int daysCount = 5;
            int lessonsPerDay = 8;
            int maxLessonsPerSubjectPerDay = 2;

            int remaining = lessonsCount;

            // Перебираем уроки по номеру (1..8)
            for (int lessonNum = 1; lessonNum <= lessonsPerDay && remaining > 0; lessonNum++)
            {
                // Перебираем дни (Пн-Пт)
                for (int day = 0; day < daysCount && remaining > 0; day++)
                {
                    // Считаем сколько уроков данного предмета уже в этот день
                    int subjectCountToday = currentSchedule.Lessons.Count(l => l.Day == day && l.Subject == subject);

                    if (subjectCountToday >= maxLessonsPerSubjectPerDay)
                        continue; // Не больше 2 уроков предмета в день

                    // Проверяем занятость учителя и кабинета в это время
                    bool teacherBusy = currentSchedule.Lessons.Any(l => l.Teacher == teacher && l.Day == day && l.LessonNumber == lessonNum);
                    bool roomBusy = currentSchedule.Lessons.Any(l => l.Room == room && l.Day == day && l.LessonNumber == lessonNum);

                    // Проверяем занятость класса в это время
                    bool slotBusy = currentSchedule.Lessons.Any(l => l.Day == day && l.LessonNumber == lessonNum);

                    if (!teacherBusy && !roomBusy && !slotBusy)
                    {
                        currentSchedule.Lessons.Add(new Lesson
                        {
                            Day = day,
                            LessonNumber = lessonNum,
                            Subject = subject,
                            Teacher = teacher,
                            Room = room,
                            Class = ClassName
                        });
                        remaining--;
                    }
                }
            }
        }

        private void DisplaySchedule()
        {
            ClearScheduleGrid();

            if (currentSchedule == null)
                return;

            foreach (var lesson in currentSchedule.Lessons)
            {
                int col = lesson.Day + 1;
                int row = lesson.LessonNumber - 1;

                dgvSchedule.Rows[row].Cells[col].Value = $"{lesson.Subject}\n{lesson.Teacher}\n{lesson.Room}";
            }
        }

        private void ClearScheduleGrid()
        {
            for (int r = 0; r < dgvSchedule.Rows.Count; r++)
                for (int c = 1; c < dgvSchedule.Columns.Count; c++)
                    dgvSchedule.Rows[r].Cells[c].Value = "";
        }
    }
}
