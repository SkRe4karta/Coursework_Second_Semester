using System.Collections.Generic;
using System.Linq;

namespace SchoolScheduler
{
    public class Schedule
    {
        public List<Lesson> Lessons { get; } = new List<Lesson>();

        public Schedule() { }

        // Получить уроки для конкретного класса
        public List<Lesson> GetLessonsForClass(string className)
        {
            return Lessons.Where(l => l.Class == className).ToList();
        }

        // Проверка, занят ли учитель в заданное время
        public bool IsTeacherBusy(string teacher, int day, int lessonNumber)
        {
            return Lessons.Any(l => l.Teacher == teacher && l.Day == day && l.LessonNumber == lessonNumber);
        }
    }
}
