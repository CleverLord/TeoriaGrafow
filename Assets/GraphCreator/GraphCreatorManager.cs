using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileOpener;
using System.IO;
using System.Linq;
using TMPro;
public class GraphCreatorManager : MonoBehaviour
{
    public Progress p;
    public SimpleGraph simpleGraph;
    public GameObject nodePrefab;
    public List<GameObject> visualNodes;
    public string editorFile;
    [Range(0,1f)]
    public float arrowSize=0.3f;
    [Range(0,30f)]
    public float arrowAngle=30;
    [Range(0,2f)]
    public float arrowOffset=0.1f;
    public TextMeshProUGUI hamiltonowskoscText;
    public TextMeshProUGUI chromaText;
    void Start() {
        
    }

    // Update is called once per frame

    // Update is called once per frame
    void Update()
    {
        UpdateLines();
    }

    public void OnLoadFileButton() {
        hamiltonowskoscText.text="Hamiltonowskosc not determined";

#if !UNITY_EDITOR
        List<string> files= FileExplorerApi.Open(windowTitle:"Podaj pliczek do wygenerowania grafu",allowMultiSelect:false);
        if(files.Count==0)
            return;
        string content=File.ReadAllText(files[0]);
#endif
#if UNITY_EDITOR
        string content=File.ReadAllText(editorFile);
#endif
        string[] nodes=content.Split(new char[]{ '[',']'});
        List<string> realNodes=nodes.Where(n=>n.Length>4).ToList();

        simpleGraph=new SimpleGraph();

        for (int i=0;i<realNodes.Count;i++){
            SimpleNode sn=SimpleNode.fromJSON(realNodes[i]);
            sn.nodeID=i;
            simpleGraph.nodes.Add(sn);
        }
        Visualize();
    }

    void Visualize() {
        foreach(GameObject node in visualNodes) {
            if(node)
                Destroy(node);
        }
        visualNodes.Clear();

        List<SimpleNode> graphNodes=simpleGraph.nodes;
        float radius=Mathf.Sqrt(graphNodes.Count);
        float angle=360f/graphNodes.Count*Mathf.Deg2Rad;

        for (int i = 0; i<graphNodes.Count;i++) {
            SimpleNode node=graphNodes[i];
            GameObject bob=Instantiate(nodePrefab);
            bob.transform.position=new Vector3(Mathf.Sin(angle*i), 0, Mathf.Cos(angle*i))*radius;
            LineRenderer lr=bob.GetComponentInChildren<LineRenderer>();
            lr.positionCount=node.connections.Count*6;
            lr.SetPositions(new Vector3[node.connections.Count*6]);
            TextMeshPro tmp=bob.GetComponentInChildren<TextMeshPro>();
            tmp.text=node.nodeID.ToString();
            visualNodes.Add(bob);
        }

    }
    void UpdateLines() {
        for(int i = 0; i<visualNodes.Count; i++) {
            GameObject thisNode=visualNodes[i];
            LineRenderer lr=thisNode.GetComponentInChildren<LineRenderer>();
            SimpleNode sn=simpleGraph.nodes[i];

            for(int j = 0; j<sn.connections.Count; j++) {
                GameObject otherVisualNode=visualNodes[ sn.connections[j] ];
                Vector3 dir=thisNode.transform.InverseTransformPoint(otherVisualNode.transform.position);
                Vector3 back,arrLeft,arrRight;
                back=-1*dir;
                back.Normalize();
                dir+=back*arrowOffset;
                arrLeft=Quaternion.AngleAxis(-arrowAngle, Vector3.up)*back*arrowSize;
                arrRight=Quaternion.AngleAxis(arrowAngle, Vector3.up)*back*arrowSize;

                lr.SetPosition(j*6+1, dir);
                lr.SetPosition(j*6+2, dir+arrLeft);
                lr.SetPosition(j*6+3, dir);
                lr.SetPosition(j*6+4, dir+arrRight);
                lr.SetPosition(j*6+5, dir);
            }
        }

    }

    #region UI Callbacks
    public void UpdateArrowSize(float f) {
        arrowSize=f;
    }
    public void UpdateArrowAngle(float f) {
        arrowAngle=f;
    }
    public void UpdateArrowOffset(float f) {
        arrowOffset=f;
    }
    public void OnCheckHamiltonowskoscButton() {
        if(simpleGraph.nodes.Count==0) return;

        simpleGraph.CheckHamiltonowskosc();
        if(simpleGraph.hamiltonowskosc==Hamiltonowskosc.none) {
            hamiltonowskoscText.text="Graph is not Hamiltonowski";
        }
        if(simpleGraph.hamiltonowskosc==Hamiltonowskosc.partial) {
            hamiltonowskoscText.text="Graph is half-Hamiltonowski with path:\n"+Graph.NicelyFormatedIntList(simpleGraph.hamiltonowskiPath);
        }
        if(simpleGraph.hamiltonowskosc==Hamiltonowskosc.full) {
            hamiltonowskoscText.text="Graph is full-Hamiltonowski with path:\n"+Graph.NicelyFormatedIntList(simpleGraph.hamiltonowskiPath);
        }
    }
    public void OnCheckChromaValues() {
        if(simpleGraph.nodes.Count==0) return;

        //new System.Threading.Thread(simpleGraph.getVertexChromaIndex).Start();
        simpleGraph.getVertexChromaIndex();
        //return;
        if(simpleGraph.vertexChromaIndex==-1) {
            chromaText.text="Sth Broke";
        }
        else
            chromaText.text=$"Vertex chroma index:{simpleGraph.vertexChromaIndex}\n"+
                Graph.NicelyFormatedIntList(simpleGraph.vertexChromaIndexAssignment);
    }
    #endregion
}
[System.Serializable]
public class Progress{
    public int x;
    public int y;
    public int z;
    [Range(0,1)]
    public double pgr;
}
