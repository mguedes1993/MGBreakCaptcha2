using System;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;

namespace MGBreakCaptcha2.MLEngine
{
    [Serializable]
    public class SVMLinearMLEngine : IMLEngine
    {
        private MulticlassSupportVectorMachine<Linear> _engine;

        public double Learn(double[][] learnData, int[] learnLabel, double[][] testData, int[] testLabel)
        {
            Log.Write(this.GetType(), "Begin Learning");

            MulticlassSupportVectorLearning<Linear> learning =
                new MulticlassSupportVectorLearning<Linear>
                {
                    Learner = learner => new LinearDualCoordinateDescent<Linear>(),
                    ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }
                };

            _engine = learning.Learn(learnData, learnLabel);

            double scoreResult = Score(testData, testLabel);

            Log.Write(this.GetType(), $"Final Score {scoreResult}");
            Log.Write(this.GetType(), "End Learning");

            return scoreResult;
        }

        public int Predict(double[] input)
        {
            return _engine.Decide(input);
        }

        public int[] Predict(double[][] input)
        {
            int[] results = new int[input.Length];

            Parallel.For(0, input.Length, index =>
            {
                results[index] = Predict(input[index]);
            });

            return results;
        }

        public double Score(double[][] data, int[] label)
        {
            int[] results = Predict(data);
            int nCorrects = results.Where((r, index) => r == label[index]).Count();
            return nCorrects / (double)label.Length;
        }
    }
}