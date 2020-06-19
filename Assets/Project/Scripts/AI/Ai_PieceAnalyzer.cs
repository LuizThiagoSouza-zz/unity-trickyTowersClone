using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_PieceAnalyzer : MonoBehaviour
{
    public TopologyMeter topologyMeter;

    public Piece CurrentPiece { get; private set; }

    private void Update()
    {
        
    }

    public Ai_IdealPlacement Analyze(Piece piece)
    {
        var topology = topologyMeter.CalculateTopology();
        var priorityGuideline = piece.aiPieceGuideline.GetTopologySequencesByPriority();

        foreach (var priority in priorityGuideline)
        {
            foreach (var topologySequence in priority.topologySequences)
            {
                var idealPlacements = AnalyzeSequence(topologySequence.sequence, topology);
                if (idealPlacements.Count > 0)
                {
                    foreach (var topo in topology)
                    {
                        Debug.DrawRay(topo, Vector3.up,
                            idealPlacements.Contains(topo) ?
                            Color.red : Color.white, 0.5f);
                    }

                    return new Ai_IdealPlacement
                    {
                        orientation = priority.orientation,
                        initialPosition = idealPlacements[0]
                    };
                }
            }
        }

        return null;
    }

    private static bool IsEqualTo(float a, float b)
    {
        return Mathf.Abs(a - b) <= 0.1f;
    }

    private List<Vector3> AnalyzeSequence(List<int> sequence, List<Vector3> topology)
    {
        var i = 0;
        var j = 0;
        var last = -1f;
        var candidates = new List<Vector3>();

        while (i < sequence.Count && j < topology.Count)
        {
            if (i == 0)
            {
                last = topology[j].y;
                candidates.Add(topology[j]);
                i++;
                j++;
                if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                    break;
            }
            else
            {
                if (sequence[i] == sequence[i - 1])
                {
                    if (IsEqualTo(topology[j].y, topology[j - 1].y))
                    {
                        last = topology[j].y;
                        candidates.Add(topology[j]);
                        i++;
                        j++;

                        if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                            break;
                    }
                    else
                    {
                        i = 0;
                        candidates.Clear();
                    }
                }
                else if (sequence[i] < sequence[i - 1])
                {
                    if (j - 1 >= 0 && IsEqualTo(topology[j].y, topology[j - 1].y - sequence[i - 1]))
                    {
                        last = topology[j].y;
                        candidates.Add(topology[j]);
                        i++;
                        j++;

                        if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                            break;
                    }
                    else
                    {
                        i = 0;
                        candidates.Clear();
                    }
                }
                else if (sequence[i] > sequence[i - 1])
                {
                    if (j + 1 < topology.Count && IsEqualTo(topology[j].y + sequence[i], topology[j + 1].y))
                    {
                        last = topology[j].y;
                        candidates.Add(topology[j]);
                        i++;
                        j++;

                        if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                            break;
                    }
                    else
                    {
                        i = 0;
                        candidates.Clear();
                    }
                }
                else if (sequence[i] == 0)
                {
                    if (topology[j].y < last)
                    {
                        candidates.Add(topology[j]);
                        i++;
                        j++;
                        if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                            break;
                    }
                    else
                    {
                        i = 0;
                        candidates.Clear();
                    }
                }
                else
                {
                    j++;
                    if (candidates.Count == sequence.Count || i >= sequence.Count || j >= topology.Count)
                        break;
                }
            }
        }

        if (candidates.Count != sequence.Count)
            candidates.Clear();

        return candidates;
    }
}

[System.Serializable]
public class Ai_IdealPlacement
{
    public Orientation orientation;
    public Vector3 initialPosition;
}