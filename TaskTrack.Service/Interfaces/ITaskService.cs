using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTrack.Repo.Models;

namespace TaskTrack.Service.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskTrack.Repo.Models.Task>> GetAllActiveTasksAsync();
        Task<TaskTrack.Repo.Models.Task?> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskTrack.Repo.Models.Task>> GetTasksByProjectAsync(int projectId);
        Task<IEnumerable<TaskTrack.Repo.Models.Task>> SearchTasksAsync(string? title, int? status, int? priority, int? projectId, int? tagId);
        Task<TaskTrack.Repo.Models.Task> CreateTaskAsync(TaskTrack.Repo.Models.Task task, int[]? tagIds);
        Task<TaskTrack.Repo.Models.Task?> UpdateTaskAsync(int id, TaskTrack.Repo.Models.Task task, int[]? tagIds);
        Task<bool> SoftDeleteTaskAsync(int id);
    }
}
