using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;

namespace SchoolScheduler
{
    public class AdminForm : Form
    {
        private ComboBox cbClasses;
        private DataGridView dgvSchedule;
        private Button btnCreateBase, btnOpenBase, btnReplaceTeacher, btnExport, btnExit;
        private Button btnEditDb;
        private Schedule currentSchedule = new Schedule();
        private string currentDbPath;
        private Button btnSaveChanges;
        private List<string> classes = new List<string>();

        public AdminForm()
        {
            Text = "Админ — Управление расписанием";
            Width = 1000;
            Height = 670;
            StartPosition = FormStartPosition.CenterScreen;

            // ComboBox для выбора класса
            cbClasses = new ComboBox
            {
                Left = 10,
                Top = 12,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbClasses.SelectedIndexChanged += CbClasses_SelectedIndexChanged;

            // Кнопка "Создать базу"
            btnCreateBase = new Button
            {
                Text = "Создать базу",
                Left = 170,
                Top = 10,
                Width = 120,
                Height = 30
            };
            btnCreateBase.Click += BtnCreateBase_Click;

            // Кнопка "Открыть базу"
            btnOpenBase = new Button
            {
                Text = "Открыть базу",
                Left = 300,
                Top = 10,
                Width = 120,
                Height = 30
            };
            btnOpenBase.Click += BtnOpenBase_Click;

            // Кнопка "Заменить учителя"
            btnReplaceTeacher = new Button
            {
                Text = "Заменить учителя",
                Left = 430,
                Top = 10,
                Width = 120,
                Height = 30
            };
            btnReplaceTeacher.Click += BtnReplaceTeacher_Click;
            
            // Кнопка "Экспорт в CSV"
            btnExport = new Button
            {
                Text = "Экспорт в CSV",
                Left = 560,
                Top = 10,
                Width = 120,
                Height = 30
            };
            btnExport.Click += BtnExport_Click;

            // Кнопка "Сохранить изменения"
            btnSaveChanges = new Button
            {
                Text = "Сохранить изменения",
                Left = 690,
                Top = 10,
                Width = 140,
                Height = 30
            };
            btnSaveChanges.Click += BtnSaveChanges_Click;

            // Кнопка "Редактировать базу"
            btnEditDb = new Button
            {
                Text = "Редактировать базу",
                Left = 840,
                Top = 10,
                Width = 120,
                Height = 30
            };
            btnEditDb.Click += BtnEditDb_Click;

            // Кнопка "Выход"
            btnExit = new Button
            {
                Text = "Выход",
                Left = 10,
                Top = 50,
                Width = 120,
                Height = 30
            };
            btnExit.Click += (s, e) => Close();

            // DataGridView для расписания
            dgvSchedule = new DataGridView
            {
                Left = 15,
                Top = 90,
                Width = 950,
                Height = 520,
                ReadOnly = false,
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

            // Добавляем колонки
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

            Controls.AddRange(new Control[]
            {
        cbClasses, btnCreateBase, btnOpenBase, btnReplaceTeacher,
        btnExport, btnSaveChanges, btnEditDb, btnExit, dgvSchedule
            });
        }

        private void BtnCreateBase_Click(object sender, EventArgs e)
        {
            using (var createForm = new CreateDatabaseForm())
            {
                if (createForm.ShowDialog() == DialogResult.OK)
                {
                    currentDbPath = createForm.CreatedDbPath;
                    LoadScheduleFromDatabase(currentDbPath);
                    LoadClassesFromSchedule();
                    MessageBox.Show("База данных создана и загружена.");
                }
            }
        }

        private void BtnOpenBase_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "SQLite Database (*.sqlite;*.db)|*.sqlite;*.db|All files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentDbPath = ofd.FileName;
                    LoadScheduleFromDatabase(currentDbPath);
                    LoadClassesFromSchedule();
                    MessageBox.Show("База данных загружена.");
                }
            }
        }

        private void LoadScheduleFromDatabase(string path)
        {
            currentSchedule.Lessons.Clear();

            using (var conn = new SQLiteConnection($"Data Source={path};Version=3;"))
            {
                conn.Open();

                string sql = "SELECT Subject, Teacher, Room, Class, LessonsCount FROM LessonsPlan";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subject = reader.GetString(0);
                            string teacher = reader.GetString(1);
                            string room = reader.GetString(2);
                            string className = reader.GetString(3);
                            int lessonsCount = reader.GetInt32(4);

                            DistributeLessons(className, subject, teacher, room, lessonsCount);
                        }
                    }
                }
            }
        }


        // Метод распределения уроков по дням и урокам
        private void DistributeLessons(string className, string subject, string teacher, string room, int lessonsCount)
        {
            int daysCount = 5;
            int lessonsPerDay = 8;
            int maxLessonsPerSubjectPerDay = 2;

            int remaining = lessonsCount;

            // Перебираем уроки (1..8)
            for (int lessonNum = 1; lessonNum <= lessonsPerDay && remaining > 0; lessonNum++)
            {
                // Перебираем дни (Пн-Пт)
                for (int day = 0; day < daysCount && remaining > 0; day++)
                {
                    // Проверяем, сколько уроков этого предмета уже есть в этот день для данного класса
                    int subjectCountToday = currentSchedule.Lessons.Count(l =>
                        l.Class == className && l.Day == day && l.Subject == subject);

                    if (subjectCountToday >= maxLessonsPerSubjectPerDay)
                        continue; // Не больше 2 уроков предмета в день

                    // Проверяем занятость класса, учителя и кабинета
                    bool classBusy = currentSchedule.Lessons.Any(l => l.Class == className && l.Day == day && l.LessonNumber == lessonNum);
                    bool teacherBusy = currentSchedule.Lessons.Any(l => l.Teacher == teacher && l.Day == day && l.LessonNumber == lessonNum);
                    bool roomBusy = currentSchedule.Lessons.Any(l => l.Room == room && l.Day == day && l.LessonNumber == lessonNum);

                    if (!classBusy && !teacherBusy && !roomBusy)
                    {
                        currentSchedule.Lessons.Add(new Lesson
                        {
                            Class = className,
                            Subject = subject,
                            Teacher = teacher,
                            Room = room,
                            Day = day,
                            LessonNumber = lessonNum
                        });
                        remaining--;
                    }
                }
            }
        }

        private void LoadClassesFromSchedule()
        {
            classes = currentSchedule.Lessons
                .Select(l => l.Class)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            cbClasses.Items.Clear();
            cbClasses.Items.AddRange(classes.ToArray());

            if (cbClasses.Items.Count > 0)
                cbClasses.SelectedIndex = 0;
            else
                ClearScheduleGrid();
        }

        private void CbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbClasses.SelectedItem == null)
                return;

            DisplayScheduleForClass(cbClasses.SelectedItem.ToString());
        }

        private void DisplayScheduleForClass(string className)
        {
            ClearScheduleGrid();

            var lessons = currentSchedule.GetLessonsForClass(className);

            foreach (var lesson in lessons)
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

        private void BtnReplaceTeacher_Click(object sender, EventArgs e)
        {
            if (currentSchedule == null)
            {
                MessageBox.Show("Загрузите расписание перед заменой учителя.");
                return;
            }

            if (dgvSchedule.CurrentCell == null)
            {
                MessageBox.Show("Пожалуйста, выберите урок для замены учителя.");
                return;
            }

            int row = dgvSchedule.CurrentCell.RowIndex;
            int col = dgvSchedule.CurrentCell.ColumnIndex;

            if (col == 0)
            {
                MessageBox.Show("Выберите ячейку с уроком, а не номер урока.");
                return;
            }

            string cellText = dgvSchedule.Rows[row].Cells[col].Value as string;

            if (string.IsNullOrWhiteSpace(cellText))
            {
                MessageBox.Show("В выбранной ячейке нет урока.");
                return;
            }

            var parts = cellText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .ToArray();

            if (parts.Length < 3)
            {
                MessageBox.Show("Неверный формат данных в ячейке.");
                return;
            }

            string subject = parts[0];
            string oldTeacher = parts[1];
            string room = parts[2];

            int day = col - 1;
            int lessonNumber = row + 1;

            string selectedClass = cbClasses.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Выберите класс.");
                return;
            }

            List<string> allTeachers = new List<string>();
            using (var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;"))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("SELECT DISTINCT Teacher FROM LessonsPlan", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var teacherName = reader.GetString(0);
                            if (!string.IsNullOrEmpty(teacherName))
                                allTeachers.Add(teacherName);
                        }
                    }
                }
            }

            var busyTeachers = currentSchedule.Lessons
                .Where(l => l.Day == day && l.LessonNumber == lessonNumber)
                .Select(l => l.Teacher)
                .Where(t => !string.IsNullOrEmpty(t) && t != oldTeacher)
                .ToHashSet();

            var freeTeachers = allTeachers
                .Where(t => !busyTeachers.Contains(t) || t == oldTeacher)
                .ToList();

            // Добавляем новые варианты в список для выбора:
            freeTeachers.Add("— Без учителя —");
            freeTeachers.Add("— Пропуск —");

            using (var replaceForm = new TeacherReplaceForm(freeTeachers.ToArray()))
            {
                if (replaceForm.ShowDialog() == DialogResult.OK)
                {
                    string newTeacher = replaceForm.SelectedTeacher;

                    if (string.IsNullOrEmpty(newTeacher))
                    {
                        MessageBox.Show("Учитель не выбран.");
                        return;
                    }

                    if (newTeacher == "— Пропуск —")
                    {
                        // Удаляем урок из расписания
                        int removedCount = currentSchedule.Lessons.RemoveAll(l =>
                            l.Day == day && l.LessonNumber == lessonNumber && l.Teacher == oldTeacher && l.Class == selectedClass);

                        if (removedCount == 0)
                        {
                            MessageBox.Show("Не удалось найти урок для удаления.");
                            return;
                        }

                        dgvSchedule.Rows[row].Cells[col].Value = "";
                    }
                    else if (newTeacher == "— Без учителя —")
                    {
                        // Находим урок и очищаем учителя
                        var lesson = currentSchedule.Lessons.FirstOrDefault(l =>
                            l.Day == day && l.LessonNumber == lessonNumber && l.Teacher == oldTeacher && l.Class == selectedClass);

                        if (lesson == null)
                        {
                            MessageBox.Show("Не удалось найти урок для замены.");
                            return;
                        }

                        lesson.Teacher = ""; // или null — по вашей логике

                        // Отображаем предмет и кабинет без учителя
                        dgvSchedule.Rows[row].Cells[col].Value = $"{lesson.Subject}\n\n{lesson.Room}";
                        dgvSchedule.CurrentCell = dgvSchedule.Rows[row].Cells[col];
                    }
                    else
                    {
                        // Заменяем учителя на выбранного
                        var lesson = currentSchedule.Lessons.FirstOrDefault(l =>
                            l.Day == day && l.LessonNumber == lessonNumber && l.Teacher == oldTeacher && l.Class == selectedClass);

                        if (lesson == null)
                        {
                            MessageBox.Show("Не удалось найти урок для замены.");
                            return;
                        }

                        lesson.Teacher = newTeacher;
                        dgvSchedule.Rows[row].Cells[col].Value = $"{lesson.Subject}\n{lesson.Teacher}\n{lesson.Room}";
                        dgvSchedule.CurrentCell = dgvSchedule.Rows[row].Cells[col];
                    }
                }
            }
        }
        private void BtnSaveChanges_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentDbPath))
            {
                MessageBox.Show("База данных не загружена.");
                return;
            }

            if (currentSchedule == null)
            {
                MessageBox.Show("Расписание не загружено.");
                return;
            }

            string selectedClass = cbClasses.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedClass))
            {
                MessageBox.Show("Выберите класс.");
                return;
            }

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={currentDbPath};Version=3;"))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        // Удаляем все записи для выбранного класса
                        using (var deleteCmd = new SQLiteCommand("DELETE FROM LessonsPlan WHERE Class = @class", conn))
                        {
                            deleteCmd.Parameters.AddWithValue("@class", selectedClass);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Вставляем обновленные уроки из currentSchedule для выбранного класса
                        using (var insertCmd = new SQLiteCommand(
                            "INSERT INTO LessonsPlan (Subject, Teacher, Room, Class, LessonsCount) VALUES (@subject, @teacher, @room, @class, @lessonsCount)",
                            conn))
                        {
                            insertCmd.Parameters.Add(new SQLiteParameter("@subject"));
                            insertCmd.Parameters.Add(new SQLiteParameter("@teacher"));
                            insertCmd.Parameters.Add(new SQLiteParameter("@room"));
                            insertCmd.Parameters.Add(new SQLiteParameter("@class"));
                            insertCmd.Parameters.Add(new SQLiteParameter("@lessonsCount"));

                            var groupedLessons = currentSchedule.Lessons
                                .Where(l => l.Class == selectedClass)
                                .GroupBy(l => new { l.Subject, l.Teacher, l.Room })
                                .Select(g => new
                                {
                                    Subject = g.Key.Subject,
                                    Teacher = g.Key.Teacher,
                                    Room = g.Key.Room,
                                    LessonsCount = g.Count()
                                });

                            foreach (var lessonGroup in groupedLessons)
                            {
                                insertCmd.Parameters["@subject"].Value = lessonGroup.Subject;
                                insertCmd.Parameters["@teacher"].Value = lessonGroup.Teacher;
                                insertCmd.Parameters["@room"].Value = lessonGroup.Room;
                                insertCmd.Parameters["@class"].Value = selectedClass;
                                insertCmd.Parameters["@lessonsCount"].Value = lessonGroup.LessonsCount;

                                insertCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }

                MessageBox.Show("Изменения сохранены в базе данных.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void BtnEditDb_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentDbPath))
            {
                MessageBox.Show("Сначала загрузите или создайте базу данных.");
                return;
            }

            using (var editForm = new EditDatabaseForm(currentDbPath))
            {
                editForm.ShowDialog();

                // После закрытия обновим расписание и классы, т.к. данные могли измениться
                LoadScheduleFromDatabase(currentDbPath);
                LoadClassesFromSchedule();

                // Если выбран класс, обновим отображение
                if (cbClasses.SelectedItem != null)
                    DisplayScheduleForClass(cbClasses.SelectedItem.ToString());
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (currentSchedule == null || currentSchedule.Lessons.Count == 0)
            {
                MessageBox.Show("Создайте или загрузите расписание перед экспортом.");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                string className = cbClasses.SelectedItem?.ToString() ?? "schedule";
                sfd.FileName = $"{className}.csv";
                sfd.Filter = "CSV Files|*.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        CsvExporter.ExportDataGridViewToCsv(dgvSchedule, sfd.FileName);
                        MessageBox.Show($"Расписание экспортировано в файл:\n{sfd.FileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при экспорте: " + ex.Message);
                    }
                }
            }
        }
    }
}
