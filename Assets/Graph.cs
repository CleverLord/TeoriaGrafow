using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Classes for Dijkstra

[System.Serializable]
public class Graph {
    public int head=-1;
    public List<Node> nodes=new List<Node>();
    public List<Connection> connections=new List<Connection>();
    [System.NonSerialized]
    public Node firstNode;
    public Node GetNode(int ID) {
        return nodes.FirstOrDefault(n => n.nodeID == ID);
    }
    public void AddNode(Vector3 position) {
        nodes.Add(new Node(firstFreeNodeID) { graphIBelongTo = this, floatingPosition = position });
    }
    public void DeleteNode(int nodeID) {
        nodes=nodes.Where(n=>n.nodeID!=nodeID).ToList();
        connections = connections.Where(c => c.fromNode != nodeID && c.toNode != nodeID).ToList();
    }
    public int firstFreeNodeID {
        get {
            int freeID=0;
            while(nodes.FirstOrDefault(n => n.nodeID == freeID) != null)
                freeID++;
            return freeID;
            }
    }
    public int firstFreeConnectionID {
        get {
            int freeID=0;
            while(connections.FirstOrDefault(n => n.connectionID == freeID) != null)
                freeID++;
            return freeID;
        }
    }

    public static string NicelyFormatedIntList(List<int> list) {
        string s="[";
        for(int i = 0; i < list.Count; i++)
            s += list[i].ToString() + ", ";
        s = s.Substring(0, s.Length - 2);
        s += "]";
        return s;
    }
}
[System.Serializable]
public class Node {
    [System.NonSerialized]
    public Graph graphIBelongTo;
    public int nodeID=-1;
    public Vector3Int position;
    public Vector3 floatingPosition { 
        get { return ((Vector3)position) / 100f; }
        set { position = fromV3(value * 100); } 
    }
    public static Vector3Int fromV3(Vector3 v3) {
        return new Vector3Int((int)v3.x, (int)v3.y, (int)v3.z);
    }
    public Node(int vertexID) {
        nodeID = vertexID;

    }
    public List<Connection> myOutcommingConnections {
        get {
            return graphIBelongTo.connections.
                Where(con => con.fromNode == nodeID ||
                     ( con.toNode == nodeID && con.bidirectional )).ToList();
        }
    }
    public List<Connection> myRelatedConnections {
        get {
            return graphIBelongTo.connections.
                Where(con => con.fromNode == nodeID ||
                     ( con.toNode == nodeID)).ToList();
        }
    }
}
[System.Serializable]
public class Connection {
    public int connectionID;
    public int fromNode;
    public int toNode;
    public bool bidirectional;
    public int weight=-1;

    [System.NonSerialized]
    public bool isBeingCreated;
    [System.NonSerialized]
    public Vector3 targetPosition;

    public Connection(int fromNode,int toNode,bool bidirectional) {
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.bidirectional = bidirectional;
    }
    public int otherNode(int thisNode) {
        if(fromNode != thisNode && toNode != thisNode) {
            Debug.LogError("Connection is not correlated with given node");
        }
        if(fromNode == thisNode)
            return toNode;
        if(toNode == thisNode && !bidirectional) {
            Debug.LogError("You can't go this way");
        }
        if(toNode == thisNode) {
            return fromNode;
        }
        return -1;

    }
}



//Classes for editor
public enum Hamiltonowskosc { full, partial, none }
[System.Serializable]
public class SimpleGraph {
    public Progress p=new Progress();
    // Start is called before the first frame update
    public List<SimpleNode> nodes=new List<SimpleNode>();
    public Hamiltonowskosc hamiltonowskosc=Hamiltonowskosc.none;
    public List<int> hamiltonowskiPath=new List<int>();
    public int vertexChromaIndex;
    public List<int> vertexChromaIndexAssignment;
    public void CheckHamiltonowskosc() {
        foreach(SimpleNode firstNode in nodes) {
            int[] path=new int[2];
            path[0] = firstNode.nodeID;

            foreach(int connection in firstNode.connections) {
                path[1] = connection;
                CheckHamiltonowskoscRecursive(new List<int>(path), connection);
            }
        }
    }
    void CheckHamiltonowskoscRecursive(List<int> nodesVisited, int nextNodeID) {
        if(hamiltonowskosc == Hamiltonowskosc.full) return;//obciąć pozostałe case'y
        SimpleNode thisNode=nodes[nextNodeID];

        int[] path=new int[nodesVisited.Count+1];
        nodesVisited.CopyTo(path);

        foreach(int connection in thisNode.connections) {
            if(!nodesVisited.Contains(connection)) {//I was not on this wierzchołek yet
                path[path.Length - 1] = connection;
                CheckHamiltonowskoscRecursive(new List<int>(path), connection);
            }
            if(nodesVisited.Count == nodes.Count-1 && hamiltonowskosc == Hamiltonowskosc.none) {
                hamiltonowskosc = Hamiltonowskosc.partial;
                hamiltonowskiPath = nodesVisited;
            }
            if(connection == path[0] && nodesVisited.Count == nodes.Count) {//we're back at start point with all wierzchołki visited
                hamiltonowskosc = Hamiltonowskosc.full;
                nodesVisited.Add(0);
                hamiltonowskiPath = nodesVisited;
            }
        }
    }
    public void getVertexChromaIndex() {
        bool satisfied=false;
        int totalChromaIndexes=1;
        int[] assignment=new int[nodes.Count+1];

        for(int i = 0; i < assignment.Length; i++)
            assignment[i] = 0;

        while(!satisfied) {

            p.y++;
            p.pgr = 1.0 * p.y / p.z;
            satisfied = ValidateAssignment(assignment);
            if(!satisfied) {//move to next assignment
                assignment[0]++;
                for(int i = 0; i < nodes.Count; i++) {
                    if(assignment[i] >= totalChromaIndexes) {
                        assignment[i] = 0;
                        assignment[i + 1]++;
                    }
                }
                if(assignment[nodes.Count] >= 1) {//end of possibilities for given chroma indexes
                    totalChromaIndexes++;
                    assignment[nodes.Count] = 0;
                    p.y = 0;
                    p.z = (int)Mathf.Pow(totalChromaIndexes, nodes.Count());
                    p.x = totalChromaIndexes;
                }
            }
            if(totalChromaIndexes > nodes.Count) {
                Debug.Log("Coś zepsułem");
                break;
            }
        }
        vertexChromaIndex = totalChromaIndexes;
        vertexChromaIndexAssignment = new List<int>(assignment);
        vertexChromaIndexAssignment.RemoveAt(nodes.Count);
    }
    bool ValidateAssignment(int[] assignment) {
        for(int i = 0; i < nodes.Count; i++) {
            SimpleNode sm=nodes[i];
            int selfColor=assignment[i];
            foreach(int c in sm.connections) {
                int otherColor=assignment[c];
                if(selfColor == otherColor)
                    return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public class SimpleNode {
    [System.NonSerialized]
    public int nodeID=-1;
    /// <summary>
    /// połączenia
    /// int - nr innego wierzchołka
    /// </summary>
    public List<int> connections=new List<int>();
    public static SimpleNode fromJSON(string s) {
        SimpleNode r=new SimpleNode();
        string[] values=s.Split(new char[]{','},System.StringSplitOptions.RemoveEmptyEntries);
        foreach(string v in values) {
            int t=-1;
            if(int.TryParse(v, out t))
                r.connections.Add(t);
        }
        return r;
    }

}