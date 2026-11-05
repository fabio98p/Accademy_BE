using AcademyModel.Entities;
using NodaTime;

namespace CodeAcademyWeb.DTOs
{
    /// <summary>Lesson of a course edition.</summary>
    public class LessonDTO
    {
        /// <summary>Lesson id.</summary>
        public long Id { get; set; }
        /// <summary>Lesson subject.</summary>
        public string Subject { get; set; }
        /// <summary>Edition id.</summary>
        public long CourseEditionId { get; set; }
        /// <summary>Classroom id.</summary>
        public long ClassroomId { get; set; }
        //public LocalDateTime Start { get; set; }
        //public LocalDateTime End { get; set; }
    }
}
