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
    [Header("Indeks chromatyczny")]
    public int indexChromatyczny=-1;
    public List<Connection> floatingConnections;
    public List<EdgeChromaAssignment> indexChromaIndexAssignment = new List<EdgeChromaAssignment>();
    [Header("Planarnosc")]
    public PlanarnoscSolution ps;
    [Header("Inne")]
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
    public TextMeshProUGUI chromaText2;
    string summary="";
    void Start() {
        
    }

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
        List<string> realNodes=nodes.ToList();
        for(int i = 0; i < realNodes.Count(); i++)
            realNodes[i] = realNodes[i].Replace("\r", "").Replace("\n", "");
        //realNodes.ForEach(s=>s.Replace("\r","").Replace("\n",""));
        realNodes=realNodes.Where(n => n.Length > 3).ToList();

        simpleGraph =new SimpleGraph();

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
        float radius=Mathf.Sqrt(graphNodes.Count)*1.3f;
        float angle=360f/graphNodes.Count*Mathf.Deg2Rad;

        for (int i = 0; i<graphNodes.Count;i++) {
            SimpleNode node=graphNodes[i];
            GameObject bob=Instantiate(nodePrefab);
            bob.SetActive(true);
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
        indexChromatyczny = -1;
        advGraph = simpleGraph.Upgraded();
        RegenNodeConnectionsPairs();
        new System.Threading.Thread(GetIndeksChromatyczyCrt).Start();
        StartCoroutine(painter());
    }
    public IEnumerator painter() {
        yield return new WaitWhile(()=>indexChromatyczny == -1);
        chromaText2.text = summary;
    }
    public void RegenNodeConnectionsPairs() {
        foreach(Node n in advGraph.nodes) {
            List<Connection> cs=new List<Connection>();
            foreach(Connection c in advGraph.connections) {
                if(c.isRelated(n.nodeID))
                    cs.Add(c);
            }
            advGraph.nodesRelatedConnections.Add(n.nodeID, cs);
        }
    }
    public void GetIndeksChromatyczyCrt() {

        int totalChromaIndexes=0;
        foreach(SimpleNode sn in simpleGraph.nodes)
            totalChromaIndexes = Mathf.Max(totalChromaIndexes, sn.connections.Count());

        floatingConnections= new List<Connection>( advGraph.connections);
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
            nodesColorslists.Add(new NodesColorLists());
        }

        if(!ValidateIndexPartialAssignment(indexChromaIndexAssignment)) {
            Debug.Log("CosZepsułem");
            return;
        }

        totalChromaIndexes--;
        while(indexChromatyczny==-1) {
            totalChromaIndexes++;
            GetIndeksChromatyczySubCrt(indexChromaIndexAssignment, floatingConnections, totalChromaIndexes);
            if(totalChromaIndexes >= advGraph.connections.Count()) {
                Debug.Log("CosZepsułem");
                return;
            }
        }

        summary= $"Indeks chromatyczny = {totalChromaIndexes}\n";
        summary += "Połączenie \t kolor\n";
        foreach(EdgeChromaAssignment ech in indexChromaIndexAssignment) {
            //summary += $"From node:{ech.c.fromNode} to node: {ech.c.toNode} paint with color {ech.color}\n";
            summary += $"[ {ech.c.fromNode}->{ech.c.toNode} \t{ech.color}]\n";
        }
    }
    public void GetIndeksChromatyczySubCrt(List<EdgeChromaAssignment> assignment, List<Connection> floatingConnections,int totalChromaIndexes) {
        if(floatingConnections.Count() == 0) { //success - no free connection and assignment is valid
            indexChromatyczny = totalChromaIndexes;
            return;
        }
        Connection c=floatingConnections.Last();                                                                //pick a connection
        floatingConnections.Remove(c);                                                                          //remove from floating
        EdgeChromaAssignment ech=new EdgeChromaAssignment(){c=c,color=0};                                       //create new assignment part
        indexChromaIndexAssignment.Add(ech);                                                                    //add it
        for(int i = 0; i < totalChromaIndexes && indexChromatyczny==-1 ; i++){                                  //move through all colors only if assignment has not been found yet
            //Nie działa
            ech.color = i;
            //Działa
            indexChromaIndexAssignment[indexChromaIndexAssignment.Count() - 1].color = i;                                                                                     //
            //Debug.Log($"Looking at connection {c.connectionID} with color {i}");
            if(ValidateIndexPartialAssignment(indexChromaIndexAssignment))
                GetIndeksChromatyczySubCrt(indexChromaIndexAssignment, floatingConnections, totalChromaIndexes);
        }
        if(floatingConnections.Count() == 0) { //success - no free connection and assignment is valid
            indexChromatyczny = totalChromaIndexes;
            return;
        }
        //no valid assignment in this subtree
        floatingConnections.Add(c);
        indexChromaIndexAssignment.RemoveAt(indexChromaIndexAssignment.Count()-1);
    }
    public List<NodesColorLists> nodesColorslists=new List<NodesColorLists>();
    public ulong times=0;
    bool ValidateIndexPartialAssignment(List<EdgeChromaAssignment> eca) {
        times++;
        foreach(NodesColorLists lista in nodesColorslists)
            lista.n.Clear();
        bool f=false;
        foreach(EdgeChromaAssignment ec in eca) {
            if(nodesColorslists[ec.c.fromNode].n.Contains(ec.color))
                return false;
            if(nodesColorslists[ec.c.toNode].n.Contains(ec.color))
                return false;
            nodesColorslists[ec.c.fromNode].n.Add(ec.color);
            nodesColorslists[ec.c.toNode].n.Add(ec.color);
            if(f)
                return false;
        }

        return true;
    }
    #endregion
    #region Planarnosc (porzucone)
    public void OnCheckPlanarnosc() {
        advGraph = simpleGraph.Upgraded();
        PlanarnoscChecker();
    }
    public void PlanarnoscChecker() {
        PlanarnoscK5Checker(advGraph);
    }
    public void PlanarnoscK5Checker(Graph g) {
        int vertsPicked=5;

        List<List<int>>combinations=GenerateCombinations(g,vertsPicked);

        bool foundSolution=false;
        while(!foundSolution) {
            foreach(List<int> combination in combinations) {
                Graph subG=FromIndexes(g,combination);
                List<List<int>> subcombinations=GenerateCombinations(subG,5);
                foreach(List<int> subcombination in subcombinations) {
                    subG = FromIndexes(g, combination);
                    ReduceGraph(subG, subcombination);
                    foundSolution = PlanarnoscK5Validator(subG);
                    if(foundSolution) {
                        ps = new PlanarnoscSolution() { kg = KuratowskiGraph.K5, combination = combination, subcombination = subcombination };
                        goto outOfWhile;
                    }
                }
            }
            vertsPicked++;
            if(vertsPicked > g.nodes.Count) {
                Debug.Log("To nie bedzie K5");
                return;
            }
        }
    outOfWhile:;

    }
    public List<List<int>> GenerateCombinations(Graph g, int howMuchPick) {
        List<List<int>>ret=new List<List<int>>();
        List<int> toPickFrom=new List<int>();
        foreach(Node n in g.nodes)
            toPickFrom.Add(n.nodeID);

        generateSubcombinations(ret, toPickFrom, new List<int>(), howMuchPick);
        return ret;
    }
    public void generateSubcombinations(List<List<int>> master,List<int> toPickFrom, List<int> current, int howMuch) {
        if(current.Count == howMuch) {
            master.Add(current);
            return;
        }
        else if(toPickFrom.Count() == 0)
            return;
        else {
            int vert=toPickFrom.First();
            List<int> newToPickFrom= new List<int>(toPickFrom);
            newToPickFrom.RemoveAt(0);
            //I'm not picking it
            generateSubcombinations(master, newToPickFrom, new List<int>(current), howMuch);
            //I'm picking it
            current.Add(vert);
            generateSubcombinations(master, newToPickFrom, new List<int>(current), howMuch);
        }
    }
    public bool PlanarnoscK5Validator(Graph sub) {
        foreach(Node n in sub.nodes) {
            if(n.myRelatedConnections.Count() != 4)
                return false;
        }
        return true;
    }

    public void PlanarnoscK33Checker() {

    }
    public Graph FromIndexes(Graph oryginal, List<int> preservedNodes) {
        Graph g=new Graph();
        foreach(int x in preservedNodes) {
            Node n=new Node(x,oryginal){};
            g.nodes.Add(n);
        }
        foreach(Connection c in oryginal.connections) {
            if(preservedNodes.Contains(c.fromNode) && preservedNodes.Contains(c.toNode)) {
                Connection c2=new Connection(c.fromNode,c.toNode,false);
                g.connections.Add(c2);
            }
        }
        return g;
    }
    public void ReduceGraph(Graph toBeReduced, List<int> preservedNodes) {
        List<int> verticesToBeDestroyed=new List<int>();
        List<List<int>> verticesToBeConnected=new List<List<int>>();            //każda lista zawiera indesky nodeów do których usunięty node był podłączony

        foreach(Node x in toBeReduced.nodes)
            if(!preservedNodes.Contains(x.nodeID)) {                            //jeżeli node ma zostać zniszczony
                verticesToBeDestroyed.Add(x.nodeID);       
            }

        foreach(int n in verticesToBeDestroyed){                            
            List<int> verticesToBeConnectedByThisNode=new List<int>();          //zacznij zbierać wierzchołki do których ma być podłączony
            foreach(Connection c in toBeReduced.connections) {
                if(c.isRelated(n)) {
                    verticesToBeConnectedByThisNode.Add(c.otherNode(n));        //dodaj tamten drugi wierzchołek
                    toBeReduced.connections.Remove(c);
                }
            }
            verticesToBeConnected.Add(verticesToBeConnectedByThisNode);
        }
        
        foreach(List<int> vs in verticesToBeConnected) {
            for(int i = 0; i < vs.Count; i++) {
                for(int j = i + 1; j < vs.Count; i++) {
                    Connection c =new Connection(vs[i],vs[j],false);
                    Connection c2 =toBeReduced.connections.First(c3=>c3.isSimilar(c));
                    if(c2==null) {
                        toBeReduced.connections.Add(c);
                    }
                }
            }
        }


    }

    #endregion
    #endregion
    public void DebugUpgrade() {
        advGraph = simpleGraph.Upgraded();
        Debug.Log(JsonUtility.ToJson(advGraph));
    }

}
public enum KuratowskiGraph { None,K5,K33};
[System.Serializable]
public class PlanarnoscSolution {
    public KuratowskiGraph kg=KuratowskiGraph.None;
    public List<int> combination;
    public List<int> subcombination;
}

[System.Serializable]
public class NodesColorLists {
    public List<int> n=new List<int>();
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
