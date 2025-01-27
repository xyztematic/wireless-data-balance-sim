using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SimulationMetricsIO
{
    public enum Metrics {
        MostPrevalentAreaRank,
        ShareOfFullyCoveredMin1Intersect,
        ShareOfFullyCoveredMin2Intersect,
        ShareOfFullyCoveredMin3Intersect,
        ShareOfFullyCoveredMin4Intersect,
        AverageInventoryLoad,
        AverageInventoryLoadNormalized,
        MinInventoryLoad,
        MinInventoryLoadNormalized,
        MaxInventoryLoadExcludingSourceNodes,
        MaxInventoryLoadExcludingSourceNodesNormalized,
        AverageNonRankIncreasingVectorsPerNode,
    }
    public static CoverageCalculator.CoverageData coverageData;
    public static int[] nodeInvRanks;
    public static int[] nodeInvLoads;
    public static bool[] nodeSourceMap;
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

    public static void WriteToFile(CoverageCalculator.CoverageData coverageData, int[] nodeInvRanks, int[] nodeInvLoads, bool[] nodeSourceMap, uint dimension) {

        // Write these to global vars so we don't have to provide them for every call of ComputeMetric
        SimulationMetricsIO.coverageData = coverageData;
        SimulationMetricsIO.nodeInvRanks = nodeInvRanks;
        SimulationMetricsIO.nodeInvLoads = nodeInvLoads;
        SimulationMetricsIO.nodeSourceMap = nodeSourceMap;
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
            case Metrics.MostPrevalentAreaRank:
                int[] rankCounts = new int[dimension + 1];
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.intersections[i] >= 1) {
                        rankCounts[coverageData.ranks[i]]++;
                    }
                }
                return Array.IndexOf(rankCounts, rankCounts.Max());

            case Metrics.ShareOfFullyCoveredMin1Intersect:
                int fullyCoveredPoints = 0;
                int minIntersectPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.intersections[i] >= 1) {
                        minIntersectPoints++;
                        if (coverageData.ranks[i] == dimension) fullyCoveredPoints++;
                    }
                }
                return (float)fullyCoveredPoints / minIntersectPoints;

            case Metrics.ShareOfFullyCoveredMin2Intersect:
                fullyCoveredPoints = 0;
                minIntersectPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.intersections[i] >= 2) {
                        minIntersectPoints++;
                        if (coverageData.ranks[i] == dimension) fullyCoveredPoints++;
                    }
                }
                return (float)fullyCoveredPoints / minIntersectPoints;

            case Metrics.ShareOfFullyCoveredMin3Intersect:
                fullyCoveredPoints = 0;
                minIntersectPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.intersections[i] >= 3) {
                        minIntersectPoints++;
                        if (coverageData.ranks[i] == dimension) fullyCoveredPoints++;
                    }
                }
                return (float)fullyCoveredPoints / minIntersectPoints;

            case Metrics.ShareOfFullyCoveredMin4Intersect:
                fullyCoveredPoints = 0;
                minIntersectPoints = 0;
                for (int i = 0; i < coverageData.ranks.Length; i++) {
                    if (coverageData.intersections[i] >= 4) {
                        minIntersectPoints++;
                        if (coverageData.ranks[i] == dimension) fullyCoveredPoints++;
                    }
                }
                return (float)fullyCoveredPoints / minIntersectPoints;

            case Metrics.AverageInventoryLoad:
                return (float)nodeInvLoads.Average();

            case Metrics.AverageInventoryLoadNormalized:
                return (float)nodeInvLoads.Average() / dimension;

            case Metrics.MinInventoryLoad:
                return (float)nodeInvLoads.Min();

            case Metrics.MinInventoryLoadNormalized:
                return (float)nodeInvLoads.Min() / dimension;

            case Metrics.MaxInventoryLoadExcludingSourceNodes:
                int max = 0;
                for (int i = 0; i < nodeSourceMap.Length; i++) {
                    if (nodeSourceMap[i]) continue;
                    max = (nodeInvLoads[i] > max) ? nodeInvLoads[i] : max;
                }
                return max;

            case Metrics.MaxInventoryLoadExcludingSourceNodesNormalized:
                return ComputeMetric(Metrics.MaxInventoryLoadExcludingSourceNodes) / dimension;
            
            case Metrics.AverageNonRankIncreasingVectorsPerNode:
                return ((float)(nodeInvLoads.Sum() - nodeInvRanks.Sum())) / nodeInvLoads.Length;

            default:
                return float.NaN;

        }
    }

}
