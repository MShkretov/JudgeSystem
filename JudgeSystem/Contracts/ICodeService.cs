using JudgeSystem.Models;

namespace JudgeSystem.Contracts
{
    public interface ICodeService
    {
        List<TaskModel> GetTasks();
        Task<CodeExecutionResult> RunCodeAndCheckOutputAsync(int taskIndex, string userCode, string input);
    }
}
