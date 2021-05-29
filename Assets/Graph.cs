using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

//Classes for Dijkstra

[System.Serializable]
public class Graph {
    [System.NonSerialized]
    public Dictionary<int,List<Connection>> nodesRelatedConnections=new Dictionary<int, List<Connection>>();//only for coloring edges

    public int head=-1;
    public List<Node> nodes=new List<Node>();
    public List<Connection> connections=new List<Connection>();
    [System.NonSerialized]
    public Node firstNode;
    public Node GetNode(int ID) {
        return nodes.FirstOrDefault(n => n.nodeID == ID);
    }
    public void AddNode(Vector3 position) {
        nodes.Add(new Node(firstFreeNodeID, this) { floatingPosition = position });
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
    public Node(int vertexID, Graph graphIBelongTo) {
        nodeID = vertexID;
        this.graphIBelongTo = graphIBelongTo;

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
    public bool isSimilar(Connection other) {
        if(other.fromNode == fromNode && other.toNode == toNode)
            return true;
        if(other.fromNode == toNode && other.toNode == fromNode)
            return true;
        return false;
    }
    public bool isRelated(int node) {
        if(fromNode == node || toNode == node)
            return true;
        return false;
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

    public Graph Upgraded() {
        int x=0;
        Graph adv=new Graph();
        foreach(SimpleNode sn in nodes) {
            adv.AddNode(Vector3.right * sn.nodeID);
            sn.connections.Sort();
        }
        foreach(SimpleNode sn in nodes) {
            foreach(int a in sn.connections) {
                Connection c=new Connection(sn.nodeID,a,false){ connectionID=x++};
                Connection c2=adv.connections.FirstOrDefault(cc=>cc.isSimilar(c));
                if(c2 != null) {
                    c2.bidirectional = true;
                    x--;
                }
                else
                    adv.connections.Add(c);
            }
        }
        return adv;
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