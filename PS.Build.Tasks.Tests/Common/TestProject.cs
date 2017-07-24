using System;

namespace PS.Build.Tasks.Tests.Common
{
    public class TestProject
    {
        #region Constructors

        public TestProject(TestSolution solution)
        {
            if (solution == null) throw new ArgumentNullException(nameof(solution));
            Solution = solution;
        }

        #endregion

        #region Properties

        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public TestSolution Solution { get; }
        public Guid[] Types { get; set; }

        #endregion
    }
}