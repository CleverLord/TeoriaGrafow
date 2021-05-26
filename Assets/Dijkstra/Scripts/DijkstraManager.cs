﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public class DijkstraManager : MonoBehaviour {
    //Default graph for algorithm 
    public Graph graph;
    //Data for executing algorithm
    [HideInInspector]
    public DijkstraAssignment dijkstraAssignment;

    //Stuff to import and export
    public TMP_InputField graphJson;
    public TMP_InputField graphJsonIn;
    public TextMeshProUGUI errorsTextField;
    public List<string> graphErrors=new List<string>();


    private void Start() {
        dijkstraAssignment = null;
        LoadTemplate();
        FindObjectOfType<DijkstraGraphVisualizer>().Refresh();
    }
    public void UpdateJSON() {
        graphJson.text = JsonUtility.ToJson(graph, true);
    }
    public void LoadJSON() {
        try {
            Debug.Log(graphJsonIn.text);
            Graph g= JsonUtility.FromJson<Graph>(graphJsonIn.text);
            if(isInputValid(g)) {
                graph = g;
                FindObjectOfType<DijkstraGraphVisualizer>().Refresh();
                errorsTextField.text = "";
                graphErrors.Clear();
            }

        }
        catch(Exception e) {
            errorsTextField.text = "Errors have been found:\n  JSON cannot import graph properly";
            ErrorReporter.singleton.SubmitReport(graphJsonIn.text + '\n' + e.Message + '\n' + e.StackTrace);
        }
    }
    void LoadTemplate() {
        graph = new Graph();
        Node n1,n2,n3;
        graph.AddNode(Vector3.zero);
        graph.AddNode(new Vector3(2, 0, 2));
        graph.AddNode(new Vector3(-1, 0, 2.7f));
        n1 = graph.nodes[0];
        n2 = graph.nodes[1];
        n3 = graph.nodes[2];
        Connection c=new Connection(n1.nodeID,n2.nodeID,false){ connectionID=0,weight=7 };
        Connection c2=new Connection(n2.nodeID,n3.nodeID,true){ connectionID=1,weight=3};
        Connection c3=new Connection(n1.nodeID,n3.nodeID,false){ connectionID=2,weight=2};
        graph.connections.Add(c);
        graph.connections.Add(c2);
        graph.connections.Add(c3);
        graph.head = 0;
        UpdateJSON();
    }
    public bool isInputValid(Graph notYetValidGraph) {
        graphErrors.Clear();
        notYetValidGraph.firstNode = notYetValidGraph.nodes.FirstOrDefault(n => n.nodeID == notYetValidGraph.head);
        if(notYetValidGraph.firstNode == null) {
            graphErrors.Add("There is no node with ID that corresponds to head ID");
        }
        if(notYetValidGraph.nodes.GroupBy(n => n.nodeID).Where(g => g.Count() > 1).Count() > 0) {
            graphErrors.Add("There are nodes with same ID's");
        }
        if(notYetValidGraph.connections.GroupBy(c => c.connectionID).Where(g => g.Count() > 1).Count() > 0) {
            graphErrors.Add("There are connections with same ID's");
        }
        if(notYetValidGraph.connections.Count(c => notYetValidGraph.nodes.FirstOrDefault(n => n.nodeID == c.fromNode) == null) > 0) {
            graphErrors.Add("There are connections that are comming from non-existing node");
        }
        if(notYetValidGraph.connections.Count(c => notYetValidGraph.nodes.FirstOrDefault(n => n.nodeID == c.toNode) == null) > 0) {
            graphErrors.Add("There are connections that are entering non-existing node");
        }
        if(notYetValidGraph.connections.Count(c => notYetValidGraph.nodes.FirstOrDefault(n => n.nodeID == c.toNode) == null) > 0) {
            graphErrors.Add("There are duplicated connections");
        }
        if(notYetValidGraph.connections.Count(c => c.weight<0) > 0) {
            graphErrors.Add("There are negative weights in connections");
        }
        if(graphErrors.Count() > 0) {
            errorsTextField.text = "Errors have been found:";
            foreach(string s in graphErrors)
                errorsTextField.text += "\n" + s;
            return false;
        }
        return true;
    }

    public IEnumerator regenLogs() {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

    }
    public void OnFastForwardButton() {
        if(dijkstraAssignment != null) {
            dijkstraAssignment.fastForward = true;
            dijkstraAssignment.makeNextStep = true;
        }
    }
    public void OnMoveNextButton() {
        if(dijkstraAssignment != null)
            dijkstraAssignment.makeNextStep = true;
    }


    //This function executes when a button on UserInterface is pressed
    public void OnAlgorithmPerformButton() {
        //Coroutines are functions that can be paused for things like rendering
        StartCoroutine(PerformDijkstra());
    }

    //This is a first step of Dijkstra Algorithm
    IEnumerator PerformDijkstra() {
        //this is a sample wait command
        yield return new WaitForEndOfFrame();
        //anything that is between such is percepted by player as immediate or pararelly-done

        Node head=graph.nodes.FirstOrDefault(n=>n.nodeID==graph.head);
        int totalCost=0;
        dijkstraAssignment = new DijkstraAssignment();
        dijkstraAssignment.priceToGetThere.Add(head.nodeID, totalCost);
        dijkstraAssignment.dijkstraTrack.Add(head.nodeID);

        dijkstraAssignment.dijkstraLogs.Add($"Dijkstra algorithm was started, starting point is {head.nodeID}");
        dijkstraAssignment.dijkstraMiniLogs.Add($"Dijkstra algorithm was started, starting point is {head.nodeID}");

        yield return new WaitWhile(() => !dijkstraAssignment.makeNextStep && !dijkstraAssignment.fastForward);
        dijkstraAssignment.makeNextStep = false;
        foreach(Connection con in head.myOutcommingConnections) {
            yield return StartCoroutine(PerformSubDijkstra(dijkstraAssignment, graph.GetNode(con.otherNode(head.nodeID)), totalCost + con.weight));
        }
        dijkstraAssignment.isDone = true;
        dijkstraAssignment.dijkstraMiniLogs.Clear();
        dijkstraAssignment.dijkstraMiniLogs.Add("Algorithm has been succesfully finished");
    }
    IEnumerator PerformSubDijkstra(DijkstraAssignment da, Node n, int totalCost) {
        if(da.isDone)
            yield break;
        da.dijkstraTrack.Add(n.nodeID);
        da.dijkstraMiniLogs.Clear();
        string s=$"Moved to node with ID {n.nodeID}\n\tcurrent cost is {totalCost}\n\tpath is {Graph.NicelyFormatedIntList(da.dijkstraTrack)}";
        da.dijkstraLogs.Add(s);
        da.dijkstraMiniLogs.Add(s);

        yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
        da.makeNextStep = false;

        if(da.priceToGetThere.ContainsKey(n.nodeID)) {                      //optimize cost eventually
            int currentCost=da.priceToGetThere[n.nodeID];                   //get current cost to get to current node
            if(totalCost < currentCost) {                                                   //if new cost is better
                da.dijkstraLogs.Add($"Node was already visited, but cost was updated");
                da.dijkstraMiniLogs.Add($"Node was already visited, but cost was updated");
                da.priceToGetThere[n.nodeID] = totalCost;                   //update cost
                yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
                da.makeNextStep = false;
                foreach(Connection con in n.myOutcommingConnections) {                      //update subtree
                    yield return StartCoroutine(PerformSubDijkstra(da, graph.GetNode(con.otherNode(n.nodeID)), totalCost + con.weight));
                }
            }
            else {
                da.dijkstraLogs.Add($"Node already have better (or equal) cost");
                da.dijkstraMiniLogs.Add($"Node already have better (or equal) cost");
                yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
                da.makeNextStep = false;
            }
        }
        else {                                                                              //this node is visited first time
            da.dijkstraLogs.Add($"Node was visited first time - cost is set");
            da.dijkstraMiniLogs.Add($"Node was visited first time - cost is set");
            da.priceToGetThere[n.nodeID] = totalCost;                       //set cost
            yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
            da.makeNextStep = false;
            foreach(Connection con in n.myOutcommingConnections) {                          //generate subtree
                yield return StartCoroutine(PerformSubDijkstra(da, graph.GetNode(con.otherNode(n.nodeID)), totalCost + con.weight));
            }
        }
        da.dijkstraTrack.Remove(n.nodeID);                                                     //when finished processing subtree, go back
    }
}
[System.Serializable]
public class DijkstraAssignment {
    public Dictionary<int,int> priceToGetThere=new Dictionary<int, int>();
    public List<int> dijkstraTrack = new List<int>();
    public List<string> dijkstraLogs = new List<string>();
    public List<string> dijkstraMiniLogs = new List<string>();
    public bool isDone=false;
    public bool makeNextStep=false;
    public bool fastForward=false;
}