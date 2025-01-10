using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SimulationMetricsIO
{
    public enum Metrics {
        ShareOfFullyCovered,
        ShareOfFullyCoveredMin1Intersect,
        ShareOfFullyCoveredMin2Intersect,
        ShareOfFullyCoveredMin3Intersect,
        ShareOfFullyCoveredMin4Intersect,
        AverageInventoryLoad,
        AverageInventoryLoadStandardized,
        MinInventoryLoad,
        MinInventoryLoadStandardized,
        MaxInventoryLoad,
        MaxInventoryLoadStandardized,
        AverageNonRankIncreasingVectorsPerNode,
    }
    public static CoverageCalculator.CoverageData coverageData;
    public static int[] nodeInvRanks;
    public static int[] nodeInvLoads;
    public static uint dimension;

    public static string filePath;

    public static void InitFileWrite(string fileName) {

        string directoryPath = Path.Combine(Application.persistentDataPath, "Recordings");
        if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
        filePath = Path.Combine(directoryPath, fileName);
        string csvHeader = "";
        foreach (string metricName in Enum.GetNames(typeof(Metrics))) {
            csvHeader += ","+metricName;
        }
        csvHeader = csvHeader.Remove(0, 1);
        File.AppendAllText(filePath, csvHeader);
    }

    public static void WriteToFile(CoverageCalculator.CoverageData coverageData, int[] nodeInvRanks, int[] nodeInvLoads, uint dimension) {

        // Write these to global vars so we don't have to provide them for every call of ComputeMetric
        SimulationMetricsIO.coverageData = coverageData;
        SimulationMetricsIO.nodeInvRanks = nodeInvRanks;
        SimulationMetricsIO.nodeInvLoads = nodeInvLoads;
        SimulationMetricsIO.dimension = dimension;

        string lineToWrite = "\n";
        foreach (Metrics metric in Enum.GetValues(typeof(Metrics))) {
            lineToWrite += (""+ComputeMetric(metric)).Replace(",",".")+",";
        }
        File.AppendAllText(filePath, lineToWrite.TrimEnd(','));
        Debug.Log("File written at: " + filePath);
    }

    public static float ComputeMetric(Metrics metricToCompute) {
        switch (metricToCompute) {
            case Metrics.ShareOfFullyCovered:
                int fullyCoveredPoints = 0;
                foreach (int i in coverageData.ranks) {
                    if (i == dimension) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.ranks.Length;

            case Metrics.ShareOfFullyCoveredMin1Intersect:
                fullyCoveredPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.ranks[i] == dimension && coverageData.intersections[i] >= 1) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.ranks.Length;

            case Metrics.ShareOfFullyCoveredMin2Intersect:
                fullyCoveredPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.ranks[i] == dimension && coverageData.intersections[i] >= 2) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.ranks.Length;

            case Metrics.ShareOfFullyCoveredMin3Intersect:
                fullyCoveredPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.ranks[i] == dimension && coverageData.intersections[i] >= 3) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.ranks.Length;

            case Metrics.ShareOfFullyCoveredMin4Intersect:
                fullyCoveredPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.ranks[i] == dimension && coverageData.intersections[i] >= 4) fullyCoveredPoints++;
                }
                return (float)fullyCoveredPoints / coverageData.ranks.Length;

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
                return (nodeInvLoads.Sum() - nodeInvRanks.Sum()) / nodeInvLoads.Length;

            default:
                return float.NaN;

        }
    }

}
