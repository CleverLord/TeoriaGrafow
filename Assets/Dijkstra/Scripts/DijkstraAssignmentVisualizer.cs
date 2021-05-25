using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
public class DijkstraAssignmentVisualizer : MonoBehaviour
{
    public DijkstraManager dm;
    public TextMeshProUGUI logs;
    public TextMeshProUGUI miniLogs;
    public bool printLogs;
    public bool printMiniLogs;
    public DijkstraAssignment visualizedAssignment;
    public RectTransform nodeParent;
    public GameObject UiNodePrefab;
    public Dictionary<int,GameObject> spawnedObjects=new Dictionary<int, GameObject>();
    public Dictionary<int,TextMeshProUGUI> nodesValues=new Dictionary<int, TextMeshProUGUI>();
    public void Update() {
        RefreshLogs();
        RefreshMiniLogs();
        RefreshBottomPanel();
    }
    public void RefreshLogs() {
        if(!printLogs) return;
        logs.text = "";
        if(dm.dijkstraAssignment == null) return;
        foreach(string log in dm.dijkstraAssignment.dijkstraLogs)
            logs.text += log+"\n";
    }
    public void RefreshMiniLogs() {
        if(!printMiniLogs) return;
        miniLogs.text = "";
        if(dm.dijkstraAssignment == null) return;
        foreach(string log in dm.dijkstraAssignment.dijkstraMiniLogs)
            miniLogs.text += log + "\n";
    }
    public void RefreshBottomPanel() {
        if(dm.dijkstraAssignment == null) {
            Clear();
            return;
        }
        ManageRespawn();
        PrintCosts();
    }
    public void ManageRespawn() {
        if(visualizedAssignment != dm.dijkstraAssignment) {
            visualizedAssignment = dm.dijkstraAssignment;
            Respawn();
        }
    }
    public void Respawn() {
        Clear();
        nodesValues.Clear();
        foreach(Node n in dm.graph.nodes) {
            GameObject bob=Instantiate(UiNodePrefab,nodeParent);
            spawnedObjects.Add(n.nodeID, bob);
            bob.name = n.nodeID.ToString();
            bob.GetComponentsInChildren<TextMeshProUGUI>().First(t => t.name == "NodeIndexText").text = n.nodeID.ToString();
            nodesValues.Add(n.nodeID, bob.GetComponentsInChildren<TextMeshProUGUI>().First(t => t.name == "NodeValueText"));
        }
    }
    public void PrintCosts() {
        foreach(var x in visualizedAssignment.priceToGetThere) {
            nodesValues[x.Key].text = x.Value.ToString();
        }
    }
    public void Clear() {
        foreach(GameObject g in spawnedObjects.Values)
            Destroy(g);
        spawnedObjects.Clear();
    }
}
