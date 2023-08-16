using JudgeSystem.Contracts;
using JudgeSystem.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace JudgeSystem.Services
{
    public class CodeService : ICodeService
    {
        private readonly List<TaskModel> _tasks;

        public CodeService()
        {
            // Initialize tasks (you can load this from a data source)
            _tasks = InitializeTasks();
        }

        public async Task<CodeExecutionResult> RunCodeAndCheckOutputAsync(int taskIndex, string userCode, string input)
        {
            if (taskIndex < 0 || taskIndex >= _tasks.Count)
            {
                return new CodeExecutionResult { Output = "Invalid task index", Points = 0 };
            }

            var task = _tasks[taskIndex];
            var expectedOutput = task.ExpectedOutputs.FirstOrDefault();

            // Here you would execute the user's code using a compiler/interpreter
            // For example, if you're dealing with C# code, you can use Roslyn to execute the code
            string actualOutput = await ExecuteUserCodeAsync(userCode, input);

            var points = (actualOutput == expectedOutput) ? 20 : 0;

            return new CodeExecutionResult { Output = actualOutput, Points = points };
        }

        public List<TaskModel> GetTasks()
        {
            return _tasks;
        }

        private async Task<string> ExecuteUserCodeAsync(string code, string input)
        {
            string output = "Error executing code";

            try
            {
                var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
                var compilation = CSharpCompilation.Create("UserCode")
                                                   .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                                                   .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                                                   .AddSyntaxTrees(syntaxTree);

                using var memoryStream = new MemoryStream();
                using var outputStream = new StreamWriter(memoryStream);

                var result = compilation.Emit(outputStream.BaseStream);

                if (result.Success)
                {
                    var assembly = Assembly.Load(memoryStream.GetBuffer());
                    var entryPoint = assembly.EntryPoint;

                    using var inputReader = new StringReader(input);
                    using var consoleOutput = new StringWriter();

                    Console.SetIn(inputReader);
                    Console.SetOut(consoleOutput);

                    entryPoint.Invoke(null, new object[] { new string[] { } });

                    output = consoleOutput.ToString();
                }
                else
                {
                    output = string.Join(Environment.NewLine, result.Diagnostics);
                }
            }
            catch (Exception ex)
            {
                output = "Exception: " + ex.Message;
            }

            return output;
        }

        private List<TaskModel> InitializeTasks()
        {
            // Initialize tasks with descriptions, example inputs, and expected outputs
            // You can load this from a data source or hard-code them here
            // For simplicity, let's hard-code some tasks
            return new List<TaskModel>
            {
                new TaskModel
                {
                    Description = "Sum of Two Numbers",
                    ExampleInputs = new List<string> { "2 3" },
                    ExpectedOutputs = new List<string> { "5" }
                },
                // Add more tasks here...
            };
        }
    }
}