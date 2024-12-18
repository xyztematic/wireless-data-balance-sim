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

    public static void WriteToFile(Metrics metricsToRecord, int timeStep, int[] coverageData, string fileName = "simdata_test") {

        string directoryPath = Path.Combine(Application.persistentDataPath, "Recordings");

        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, fileName);

        File.AppendAllText(filePath, "TEST");
        Debug.Log("File written at: " + filePath);
    }
}
