using JudgeSystem.Models;

namespace JudgeSystem.Contracts
{
    public interface ICodeService
    {
        List<TaskModel> GetTasks();
        Task<CodeExecutionResult> RunCodeAndCheckOutputAsync(CodeSubmissionModel submission);
    }
}
