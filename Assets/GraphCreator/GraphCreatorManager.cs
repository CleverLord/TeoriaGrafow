using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileOpener;
using System.IO;
using System.Linq;
using TMPro;
public class GraphCreatorManager : MonoBehaviour
{
    public SimpleGraph simpleGraph;
    public Graph advGraph;

    public int liczbaChromatyczna=-1;
    public List<int> vertexChromaIndexAssignment = new List<int>();
    [Range(0,1000)]
    public int skipper=2;
    public int skp=0;
    public int indexChromatyczny=-1;
    public List<EdgeChromaAssignment> indexChromaIndexAssignment = new List<EdgeChromaAssignment>();
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
    #region LiczbaChromatyczna
    public void OnCheckLiczbaChromatyczna() {
        if(simpleGraph.nodes.Count==0) return;

        //new System.Threading.Thread(simpleGraph.getVertexChromaIndex).Start();
        getLiczbaChromatyczna();
        //return;
        if(liczbaChromatyczna == -1) {
            chromaText.text="Sth Broke";
        }
        else
            chromaText.text=$"Vertex chroma index:{liczbaChromatyczna}\n"+
                Graph.NicelyFormatedIntList(vertexChromaIndexAssignment);
    }
    public void getLiczbaChromatyczna() {
        bool satisfied=false;
        int totalChromaIndexes=1;
        int[] assignment=new int[simpleGraph.nodes.Count+1];

        for(int i = 0; i < assignment.Length; i++)
            assignment[i] = 0;

        while(!satisfied) {

            simpleGraph.p.y++;
            simpleGraph.p.pgr = 1.0 * simpleGraph.p.y / simpleGraph.p.z;
            satisfied = ValidateLiczbaAssignment(assignment);
            if(!satisfied) {//move to next assignment
                assignment[0]++;
                for(int i = 0; i < simpleGraph.nodes.Count; i++) {
                    if(assignment[i] >= totalChromaIndexes) {
                        assignment[i] = 0;
                        assignment[i + 1]++;
                    }
                }
                if(assignment[simpleGraph.nodes.Count] >= 1) {//end of possibilities for given chroma indexes
                    totalChromaIndexes++;
                    assignment[simpleGraph.nodes.Count] = 0;
                    simpleGraph.p.y = 0;
                    simpleGraph.p.z = (int)Mathf.Pow(totalChromaIndexes, simpleGraph.nodes.Count());
                    simpleGraph.p.x = totalChromaIndexes;
                }
            }
            if(totalChromaIndexes > simpleGraph.nodes.Count) {
                Debug.Log("Coś zepsułem");
                break;
            }
        }
        liczbaChromatyczna = totalChromaIndexes;
        vertexChromaIndexAssignment = new List<int>(assignment);
        vertexChromaIndexAssignment.RemoveAt(simpleGraph.nodes.Count);
    }
    bool ValidateLiczbaAssignment(int[] assignment) {
        for(int i = 0; i < simpleGraph.nodes.Count; i++) {
            SimpleNode sm=simpleGraph.nodes[i];
            int selfColor=assignment[i];
            foreach(int c in sm.connections) {
                int otherColor=assignment[c];
                if(selfColor == otherColor)
                    return false;
            }
        }
        return true;
    }
    #endregion
    #region IndexChromatyczny
    public void OnCheckIndeksChromatyczny() {
        liczbaChromatyczna = -1;
        advGraph = simpleGraph.Upgraded();
        RegenNodeConnectionsPairs();
        StartCoroutine(GetIndeksChromatyczyCrt());
    }
    public void RegenNodeConnectionsPairs() {
        foreach(Node n in advGraph.nodes) {
            List<Connection> cs=new List<Connection>();
            foreach(Connection c in advGraph.connections) {
                if(c.relatedWithNode(n.nodeID))
                    cs.Add(c);
            }
            advGraph.nodesRelatedConnections.Add(n.nodeID, cs);
        }
    }
    public IEnumerator GetIndeksChromatyczyCrt() {
        yield return new WaitForEndOfFrame();

        bool satisfied=false;
        int totalChromaIndexes=1;
        foreach(SimpleNode sn in simpleGraph.nodes)
            totalChromaIndexes = Mathf.Max(totalChromaIndexes, sn.connections.Count());

        List<Connection> floatingConnections=advGraph.connections;
        indexChromaIndexAssignment = new List<EdgeChromaAssignment>();

        foreach(SimpleNode sn in simpleGraph.nodes)
            if(sn.connections.Count == totalChromaIndexes) {
                //podstaw pod wierzchołek z największą ilością krawędzi
                int col=0;
                foreach(Connection c in advGraph.nodes[sn.nodeID].myRelatedConnections) {
                    indexChromaIndexAssignment.Add(new EdgeChromaAssignment() { c = c, color = col });
                    col++;
                    floatingConnections.Remove(c);
                }
                break;
            }

        foreach(SimpleNode sn in simpleGraph.nodes) {
            nodesColorslists.Add(new List<int>());
        }

        if(!ValidateIndexPartialAssignment(indexChromaIndexAssignment)) {
            Debug.Log("CosZepsułem");
            yield break;
        }

        while(indexChromatyczny==-1) {
            StartCoroutine(GetIndeksChromatyczySubCrt(indexChromaIndexAssignment, floatingConnections, totalChromaIndexes));
            totalChromaIndexes++;
            yield return new WaitForEndOfFrame();
        }
            //validate
            //if valid and not all floating conenctions not empty
                //make podstawienie
                //validate
                    //if valid and not all floating conenctions not empty
                    // ... wiadomo co dalej
    }
    public IEnumerator GetIndeksChromatyczySubCrt(List<EdgeChromaAssignment> assignment, List<Connection> floatingConnections,int totalChromaIndexes) {
        if(floatingConnections.Count() == 0) { //success - no free connection and assignment is valid
            indexChromatyczny = totalChromaIndexes;
            yield break;
        }
        Connection c=floatingConnections.Last();                                                                //pick a connection
        floatingConnections.Remove(c);                                                                          //remove from floating
        EdgeChromaAssignment ech=new EdgeChromaAssignment(){c=c,color=0};                                       //create new assignment part
        assignment.Add(ech);                                                                                    //add it
        for(int i = 0; i < totalChromaIndexes && indexChromatyczny==-1 ; i++){                                  //move through all colors only if assignment has not been found yet
            ech.color = 0;                                                                                      //
            if(ValidateIndexPartialAssignment(assignment))
                yield return StartCoroutine(GetIndeksChromatyczySubCrt(assignment, floatingConnections, totalChromaIndexes));
        }
        //no valid assignment in this subtree
        floatingConnections.Add(c);
        assignment.Remove(ech);

        if(skp == skipper) {
            yield return new WaitForEndOfFrame();
            skp = 0;
        }
        else
            skp++;
    }
    List<List<int>> nodesColorslists=new List<List<int>>();
    
    bool ValidateIndexPartialAssignment(List<EdgeChromaAssignment> eca) {
        foreach(List<int> lista in nodesColorslists)
            lista.Clear();

        foreach(EdgeChromaAssignment ec in eca) {
            if(nodesColorslists[ec.c.fromNode].Contains(ec.color))
                return false;
            if(nodesColorslists[ec.c.toNode].Contains(ec.color))
                return false;
            nodesColorslists[ec.c.fromNode].Add(ec.color);
            nodesColorslists[ec.c.toNode].Add(ec.color);
        }

        return true;
    }
    
    #endregion
    #endregion
}
[System.Serializable]
public class EdgeChromaAssignment {
    public Connection c;
    public int color=-1;
}

[System.Serializable]
public class Progress{
    public int x;
    public int y;
    public int z;
    [Range(0,1)]
    public double pgr;
}
