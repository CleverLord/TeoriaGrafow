using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

public partial class DijkstraManager : MonoBehaviour {

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
            Graph g= JsonUtility.FromJson<Graph>(graphJsonIn.text);
            if(isInputValid(g)) {
                graph = g;
                FindObjectOfType<DijkstraGraphVisualizer>().Refresh();
                errorsTextField.text = "";
                graphErrors.Clear();
                foreach(Node n in g.nodes)
                    n.graphIBelongTo = g;
            }

        }
        catch(Exception e) {
            errorsTextField.text = "Errors have been found:\n  JSON cannot import graph properly";
            ErrorReporter.singleton.SubmitReport(graphJsonIn.text + '\n' + e.Message + '\n' + e.StackTrace);
        }
    }
    void LoadTemplate() {
        graph = new Graph();
        Node n1,n2,n3,n4;
        graph.AddNode(Vector3.zero);
        graph.AddNode(new Vector3(2, 0, 2));
        graph.AddNode(new Vector3(-1, 0, 2.7f));
        graph.AddNode(new Vector3(2.7f, 0, -1));
        n1 = graph.nodes[0];
        n2 = graph.nodes[1];
        n3 = graph.nodes[2];
        n4 = graph.nodes[3];
        Connection c=new Connection(n1.nodeID,n2.nodeID,false){ connectionID=0,weight=7 };
        Connection c2=new Connection(n2.nodeID,n3.nodeID,true){ connectionID=1,weight=3 };
        Connection c3=new Connection(n1.nodeID,n3.nodeID,false){ connectionID=2,weight=2 };
        Connection c4=new Connection(n1.nodeID,n4.nodeID,false){ connectionID=3,weight=4 };
        Connection c5=new Connection(n4.nodeID,n2.nodeID,false){ connectionID=4,weight=0 };
        graph.connections.Add(c);
        graph.connections.Add(c2);
        graph.connections.Add(c3);
        graph.connections.Add(c4);
        graph.connections.Add(c5);
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


    
}
