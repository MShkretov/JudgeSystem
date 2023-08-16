namespace JudgeSystem.Models
{
    public class TaskModel
    {
        public string Description { get; set; }
        public List<string> ExampleInputs { get; set; }
        public List<string> ExpectedOutputs { get; set; }
    }
}
