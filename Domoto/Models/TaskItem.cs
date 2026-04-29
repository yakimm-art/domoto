using System;

namespace Domoto.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }  // Low, Medium, High
        public string Category { get; set; }  // Work, Personal, Other
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }

        public TaskItem()
        {
            DueDate = DateTime.Now.AddDays(1);
            Priority = "Medium";
            Category = "Work";
            IsCompleted = false;
            CreatedDate = DateTime.Now;
        }

        public bool IsOverdue
        {
            get { return !IsCompleted && DueDate < DateTime.Now; }
        }

        public bool IsDueSoon
        {
            get { return !IsCompleted && !IsOverdue && (DueDate - DateTime.Now).TotalHours <= 24; }
        }
    }
}
