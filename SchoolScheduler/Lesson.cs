namespace SchoolScheduler
{
    public class Lesson
    {
        public int Day { get; set; }           // 0..4 (Пн..Пт)
        public int LessonNumber { get; set; }  // 1..8
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public string Room { get; set; }
        public string Class { get; set; }      // Класс, к которому относится урок
    }
}
