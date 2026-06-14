using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkAgentDemo.Plugins
{
    // Simulates a task store - in production this would be injected via DI
    public class TaskPlugin
    {
        // In-memory task list standing in for a real database
        private readonly List<(int Id, string Title, bool Done, DateOnly DueDate)> _tasks =
        [
            (1, "Review PR #42",      false, DateOnly.FromDateTime(DateTime.UtcNow)),
        (2, "Write unit tests",   false, DateOnly.FromDateTime(DateTime.UtcNow)),
        (3, "Deploy to staging",  false, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
        (4, "Update changelog",   true,  DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
    ];

        [KernelFunction("get_tasks_due_today")]
        [Description("Returns all tasks due today that are not yet completed.")]
        public string GetTasksDueToday()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var due = _tasks
                .Where(t => t.DueDate == today && !t.Done)
                .Select(t => $"[{t.Id}] {t.Title}");

            return due.Any()
                ? "Tasks due today:\n" + string.Join("\n", due)
                : "No tasks due today.";
        }

        [KernelFunction("get_task_by_id")]
        [Description("Returns the details of a specific task by its numeric ID.")]
        public string GetTaskById(
            [Description("The numeric task ID")] int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == default) return $"Task {id} not found.";
            return $"Task {task.Id}: '{task.Title}' | Done: {task.Done} | Due: {task.DueDate}";
        }

        [KernelFunction("mark_task_done")]
        [Description("Marks a task as completed by its numeric ID. Returns confirmation or an error.")]
        public string MarkTaskDone(
            [Description("The numeric task ID to mark as complete")] int id)
        {
            var idx = _tasks.FindIndex(t => t.Id == id);
            if (idx < 0) return $"Task {id} not found.";
            var t = _tasks[idx];
            if (t.Done) return $"Task {id} was already marked as done.";
            _tasks[idx] = (t.Id, t.Title, true, t.DueDate);
            return $"Task {id} ('{t.Title}') marked as done.";
        }
    }
}
