using System.IO;
using System.Linq;
using UnityEngine;

public static class SimulationMetricsIO
{
    public enum Metrics {
        ShareOfFullyCovered,
        ShareOfFullyCoveredMin1Intersect, // TODO
        ShareOfFullyCoveredMin2Intersect, // TODO
        AverageInventoryLoad,
        AverageInventoryLoadStandardized,
        MinInventoryLoad,
        MinInventoryLoadStandardized,
        MaxInventoryLoad,
        MaxInventoryLoadStandardized,
        AverageNonRankIncreasingVectorsPerNode,
    }
    public static int[] coverageData;
    public static int[] nodeInvRanks;
    public static int[] nodeInvLoads;
    public static uint dimension;

    public static string filePath;

    public static void InitFileWrite(string fileName) {

        string directoryPath = Path.Combine(Application.persistentDataPath, "Recordings");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        filePath = Path.Combine(directoryPath, fileName);
        File.AppendAllText(filePath, "TODO: CSV header");
    }

    public static void WriteToFile(int[] coverageData, int[] nodeInvRanks, int[] nodeInvLoads, uint dimension) {

        // Write these to global vars so we don't have to provide them for every call of ComputeMetric
        SimulationMetricsIO.coverageData = coverageData;
        SimulationMetricsIO.nodeInvRanks = nodeInvRanks;
        SimulationMetricsIO.nodeInvLoads = nodeInvLoads;
        SimulationMetricsIO.dimension = dimension;

        string lineToWrite = "\n"
            + (""+ComputeMetric(Metrics.ShareOfFullyCovered)).Replace(',','.')
            + (""+ComputeMetric(Metrics.ShareOfFullyCovered)).Replace(',','.');
        File.AppendAllText(filePath, lineToWrite);
        Debug.Log("File written at: " + filePath);
    }

    public static float ComputeMetric(Metrics metricToCompute) {
        switch (metricToCompute) {
            case Metrics.ShareOfFullyCovered:
                int fullyCoveredPoints = 0;
                foreach (int i in coverageData) {
                    if (i == dimension) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.Length;

            case Metrics.AverageInventoryLoad:
                return (float)nodeInvLoads.Average();

            case Metrics.AverageInventoryLoadStandardized:
                return (float)nodeInvLoads.Average() / dimension;

            case Metrics.MinInventoryLoad:
                return (float)nodeInvLoads.Min();

            case Metrics.MinInventoryLoadStandardized:
                return (float)nodeInvLoads.Min() / dimension;

            case Metrics.MaxInventoryLoad:
                return (float)nodeInvLoads.Max();

            case Metrics.MaxInventoryLoadStandardized:
                return (float)nodeInvLoads.Max() / dimension;
            
            case Metrics.AverageNonRankIncreasingVectorsPerNode:
                return nodeInvLoads.Sum() - nodeInvRanks.Sum();

            default:
                return float.NaN;

        }
    }

}
