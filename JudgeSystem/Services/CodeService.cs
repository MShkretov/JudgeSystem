using JudgeSystem.Contracts;
using JudgeSystem.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JudgeSystem.Services
{
    public class CodeService : ICodeService
    {
        private readonly List<TaskModel> _tasks;

        public CodeService()
        {
            _tasks = InitializeTasks();
        }

        public async Task<CodeExecutionResult> RunCodeAndCheckOutputAsync(CodeSubmissionModel submission)
        {
            if (submission.TaskIndex < 0 || submission.TaskIndex >= _tasks.Count)
            {
                return new CodeExecutionResult { Output = "Invalid task index", Points = 0 };
            }

            var task = _tasks[submission.TaskIndex];

            try
            {
                // Compile the user code
                var compilation = CSharpCompilation.Create("UserCode")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(submission.UserCode));

                using var memoryStream = new System.IO.MemoryStream();
                using var outputStream = new System.IO.StreamWriter(memoryStream);

                var result = compilation.Emit(outputStream.BaseStream);

                if (!result.Success)
                {
                    return new CodeExecutionResult { Output = string.Join(Environment.NewLine, result.Diagnostics), Points = 0 };
                }

                string actualOutput = await ExecuteWithInputAsync(submission.UserCode, task.ExampleInputs[0]);
                string expectedOutput = task.ExpectedOutputs[0];

                int points = (actualOutput.Trim() == expectedOutput.Trim()) ? 20 : 0;

                return new CodeExecutionResult { Output = actualOutput, Points = points };
            }
            catch (Exception ex)
            {
                return new CodeExecutionResult { Output = "Exception: " + ex.Message, Points = 0 };
            }
        }

        public List<TaskModel> GetTasks()
        {
            return _tasks;
        }

        private async Task<string> ExecuteWithInputAsync(string code, string input)
        {
            try
            {
                // Compile the user code
                var compilation = CSharpCompilation.Create("UserCode")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(SyntaxFactory.ParseSyntaxTree(code));

                using var memoryStream = new MemoryStream();
                using var outputStream = new StreamWriter(memoryStream);

                var result = compilation.Emit(outputStream.BaseStream);

                if (!result.Success)
                {
                    return string.Join(Environment.NewLine, result.Diagnostics);
                }

                // Create a new process to run the compiled code
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process
                {
                    StartInfo = processStartInfo
                };

                process.Start();

                // Redirect the standard input of the process
                using (var writer = process.StandardInput)
                {
                    if (writer.BaseStream.CanWrite)
                    {
                        await writer.WriteLineAsync(input);
                        writer.Flush(); // Ensure buffered data is written
                    }
                }

                // Read the output and error streams
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    return "Error: " + error;
                }

                return output;
            }
            catch (Exception ex)
            {
                return "Exception: " + ex.Message;
            }
        }


        private List<TaskModel> InitializeTasks()
        {
            return new List<TaskModel>
    {
        new TaskModel
        {
            Description = "Sum of Two Numbers",
            ExampleInputs = new List<string> { "2 3" },
            ExpectedOutputs = new List<string> { "5" }
        },
        new TaskModel
        {
            Description = "Find the Maximum",
            ExampleInputs = new List<string> { "4 7 2" },
            ExpectedOutputs = new List<string> { "7" }
        },
        new TaskModel
        {
            Description = "Calculate Factorial",
            ExampleInputs = new List<string> { "5" },
            ExpectedOutputs = new List<string> { "120" }
        },
        new TaskModel
        {
            Description = "Check Prime Number",
            ExampleInputs = new List<string> { "7" },
            ExpectedOutputs = new List<string> { "Prime" }
        },
        new TaskModel
        {
            Description = "Reverse String",
            ExampleInputs = new List<string> { "hello" },
            ExpectedOutputs = new List<string> { "olleh" }
        }

    };
        }

            List<TaskModel> ICodeService.GetTasks()
        {
            return _tasks;
        }
    }
}