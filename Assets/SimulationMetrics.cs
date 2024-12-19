using System.IO;
using UnityEngine;

public static class SimulationMetrics
{
    public enum Metrics {
        ShareOfFullyCovered,
        RankAverageStandardized,
        MinRankStandardized,
        MaxRankStandardized,
        AverageInventoryLoad,
        MinInventoryLoad,
        MaxInventoryLoad,

        ALL
    }

    public static void WriteToFile(int timeStep, int[] coverageData, uint dimension, string fileName = "simdata_test") {

        string directoryPath = Path.Combine(Application.persistentDataPath, "Recordings");

        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, fileName);
        string lineToWrite = "\n"+timeStep;
        lineToWrite += ","+ComputeMetric(Metrics.ShareOfFullyCovered, coverageData, dimension);
        File.AppendAllText(filePath, lineToWrite);
        Debug.Log("File written at: " + filePath);
    }

    public static float ComputeMetric(Metrics metricToCompute, int[] coverageData, uint dimension) {
        switch (metricToCompute) {
            case Metrics.ShareOfFullyCovered:
                int fullyCoveredPoints = 0;
                foreach (int i in coverageData) {
                    if (i == dimension) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.Length;
        }
        return 0f;
    }

}
