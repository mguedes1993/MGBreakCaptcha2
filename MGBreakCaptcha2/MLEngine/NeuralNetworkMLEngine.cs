using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;

namespace MGBreakCaptcha2.MLEngine
{
    [Serializable]
    public class NeuralNetworkMLEngine : IMLEngine
    {
        private DeepBeliefNetwork _neuralNetwork;

        public int[] HiddenLayers { get; set; }

        public double Learn(double[][] learnData, int[] learnLabel, double[][] testData, int[] testLabel)
        {
            Log.Write(this.GetType(), "Begin Learning");

            int nInputs = learnData[0].Length;
            int nOutputs = learnLabel.Distinct().Count();
            double[][] labelDoubles = DictionaryTools.IntToDoubles(nOutputs, learnLabel);

            int[] layers =
            {
                nOutputs * 2, nOutputs
            };

            _neuralNetwork = new DeepBeliefNetwork(nInputs, layers);
            new GaussianWeights(_neuralNetwork).Randomize();
            _neuralNetwork.UpdateVisibleWeights();

            BackPropagationLearning learning = new BackPropagationLearning(_neuralNetwork);

            List<double> errorList = new List<double>();
            int counter = 1;
            while(true)
            {
                double error = learning.RunEpoch(learnData, labelDoubles);
                double tmpError = 0;
                if (errorList.Count > 0)
                {
                    tmpError = errorList.Last();
                }
                errorList.Add(error);

                if (counter % 10 == 0)
                {
                    Log.Write(this.GetType(), $"Iteration {counter} | Score {Score(testData, testLabel)} | Error {error}");
                }

                if (Math.Abs(errorList.Last() - tmpError) < 0.01)
                {
                    break;
                }

                counter++;
            }

            double scoreResult = Score(testData, testLabel);

            Log.Write(this.GetType(), $"Final Score {scoreResult}");
            Log.Write(this.GetType(), "End Learning");

            return scoreResult;
        }

        private void ReinforcementLearn(BackPropagationLearning learning, double[][] data, int[] label, double[][] labelDoubles)
        {
            (double[][] errorData, double[][] errorLabel) = ErrorData(data, label, labelDoubles);

            learning.RunEpoch(errorData, errorLabel);
        }

        private (double[][], double[][]) ErrorData(double[][] data, int[] label, double[][] labelDoubles)
        {
            int[] results = Predict(data);
            int[] indexError = results.Where((r, index) => r != label[index]).ToArray();

            double[][] errorData = indexError.Select(index => data[index]).ToArray();
            double[][] errorLabelDoubles = indexError.Select(index => labelDoubles[index]).ToArray();

            return (errorData, errorLabelDoubles);
        }

        public int Predict(double[] input)
        {
            double[] result = _neuralNetwork.Compute(input);
            return DictionaryTools.DoublesToInt(result);
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