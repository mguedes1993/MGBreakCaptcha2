namespace MGBreakCaptcha2.MLEngine
{
    public interface IMLEngine
    {
        double Learn(double[][] learnData, int[] learnLabel, double[][] testData, int[] testLabel);

        int Predict(double[] input);

        int[] Predict(double[][] input);

        double Score(double[][] data, int[] label);
    }
}