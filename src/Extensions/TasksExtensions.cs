using System.Runtime.CompilerServices;

namespace MessagingDemo.Extensions;

public static class TasksExtensions
{
    public static async IAsyncEnumerable<Task> Stream(this IEnumerable<Task> tasks, [EnumeratorCancellation] CancellationToken ct = default)
    {
        
        var taskList = tasks.ToList();
        while(!ct.IsCancellationRequested && taskList.Count > 0)
        {
            var completedTask = await Task.WhenAny(taskList);
            taskList.Remove(completedTask);
            yield return completedTask;
        }
        
        
    }
    
    public static async IAsyncEnumerable<T> Stream<T>(this IEnumerable<Task<T>> tasks, [EnumeratorCancellation] CancellationToken ct = default)
    {
        
        var taskList = tasks.ToList();
        while(!ct.IsCancellationRequested && taskList.Count > 0)
        {
            var completedTask = await Task.WhenAny(taskList);
            taskList.Remove(completedTask);
            yield return completedTask.Result;
        }
        
        
    }
}