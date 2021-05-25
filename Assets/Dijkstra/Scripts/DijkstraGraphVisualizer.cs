using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class DijkstraGraphVisualizer : MonoBehaviour {
    public DijkstraManager djm;
    public GameObject nodePrefab;
    public GameObject connectionPrefab;
    public List<VisualNode> spawnedNodes=new List<VisualNode>();
    public List<VisualConnection> spawnedConnections=new List<VisualConnection>();

    [Range(0,1f)]
    public float arrowSize=0.3f;
    [Range(0,40f)]
    public float arrowAngle=30;
    [Range(0,2f)]
    public float arrowOffset=0.1f;

    public void Update() {
        UpdateConnectionsPositions();
    }
    public void Refresh() {
        RespawnNodes();
        RespawnConnections();
    }
    public void RespawnNodes() {
        foreach(VisualNode b in spawnedNodes)
            Destroy(b.go);
        spawnedNodes.Clear();

        foreach(Node n in djm.graph.nodes) {
            GameObject bob= Instantiate(nodePrefab);
            bob.transform.position = n.floatingPosition;
            spawnedNodes.Add(new VisualNode() { n = n, go = bob });
            bob.name = n.nodeID.ToString();
            bob.transform.position = n.floatingPosition;
            bob.GetComponentInChildren<TextMeshPro>().text = n.nodeID.ToString();
        }
    }
    public void RespawnConnections() {
        foreach(VisualConnection b in spawnedConnections)
            Destroy(b.go);
        spawnedConnections.Clear();

        foreach(Connection c in djm.graph.connections) {
            GameObject bob = Instantiate(connectionPrefab);
            LineRenderer lr = bob.GetComponentInChildren<LineRenderer>();
            VisualNode vnFrom=spawnedNodes.FirstOrDefault(x=>x.n.nodeID==c.fromNode);
            VisualNode vnTo=spawnedNodes.FirstOrDefault(x=>x.n.nodeID==c.toNode);
            VisualConnection vc= new VisualConnection() { c = c, go = bob, lr = lr, fromVN = vnFrom, toVN=vnTo };
            spawnedConnections.Add(vc);

            bob.transform.parent = vnFrom.go.transform;
            bob.transform.localPosition = Vector3.zero;
            lr.positionCount = 10;
            lr.SetPositions(new Vector3[10]);
            vc.cube = vc.go.transform.Find("Cube").gameObject;
            vc.tmp = vc.go.GetComponentInChildren<TextMeshPro>();
        }
    }
    public void UpdateConnectionsPositions() {
        foreach(var con in spawnedConnections) {
            Vector3 start=con.fromVN.go.transform.position;
            Vector3 end=Vector3.zero;
            if(con.c.isBeingCreated)
                end = con.c.targetPosition;
            else
                end = con.toVN.go.transform.position;

            Vector3 back=(start-end).normalized;
            Vector3 fwd=(end-start).normalized;

            start += fwd * arrowOffset;
            end += back * arrowOffset;

            Vector3 arrLeft = Quaternion.AngleAxis(-arrowAngle, Vector3.up) * back * arrowSize;
            Vector3 arrRight = Quaternion.AngleAxis(arrowAngle, Vector3.up) * back * arrowSize;
            Vector3 arrLeftBack = Quaternion.AngleAxis(-arrowAngle, Vector3.up) * fwd * arrowSize;
            Vector3 arrRightBack = Quaternion.AngleAxis(arrowAngle, Vector3.up) * fwd * arrowSize;
            LineRenderer lr=con.lr;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.SetPosition(2, end + arrLeft);
            lr.SetPosition(3, end);
            lr.SetPosition(4, end + arrRight);
            lr.SetPosition(5, end);
            if(con.c.bidirectional) {
                lr.SetPosition(6, start);
                lr.SetPosition(7, start + arrLeftBack);
                lr.SetPosition(8, start);
                lr.SetPosition(9, start + arrRightBack);
            }
            else {
                lr.SetPosition(6, end);
                lr.SetPosition(7, end);
                lr.SetPosition(8, end);
                lr.SetPosition(9, end);
            }

            con.cube.transform.position = Vector3.Lerp(start, end, 0.5f);
            if(con.c.weight == -1)
                con.cube.SetActive(false);
            else {
                con.tmp.text = con.c.weight.ToString();
                con.cube.SetActive(true);
            }
        }
    }
    public void UpdateConnectionsPositionsOld() {
        foreach(var con in spawnedConnections) {
            Vector3 dir=Vector3.one;
            Vector3 start=Vector3.one;
            if(con.c.isBeingCreated)
                dir = con.go.transform.InverseTransformPoint(con.c.targetPosition);
            else
                dir = con.toVN.go.transform.position - con.fromVN.go.transform.position;
            start = dir.normalized * arrowOffset;

            Vector3 back=(-1*dir).normalized;
            dir += back * arrowOffset;
            Vector3 arrLeft = Quaternion.AngleAxis(-arrowAngle, Vector3.up) * back * arrowSize;
            Vector3 arrRight = Quaternion.AngleAxis(arrowAngle, Vector3.up) * back * arrowSize;
            Vector3 arrLeftBack = Quaternion.AngleAxis(-arrowAngle, Vector3.up) * dir.normalized * arrowSize;
            Vector3 arrRightBack = Quaternion.AngleAxis(arrowAngle, Vector3.up) * dir.normalized * arrowSize;
            LineRenderer lr=con.lr;

            lr.SetPosition(0, start);
            lr.SetPosition(1, dir);
            lr.SetPosition(2, dir + arrLeft);
            lr.SetPosition(3, dir);
            lr.SetPosition(4, dir + arrRight);
            lr.SetPosition(5, dir);
            if(con.c.bidirectional) {
                lr.SetPosition(6, start);
                lr.SetPosition(7, start + arrLeftBack);
                lr.SetPosition(8, start);
                lr.SetPosition(9, start + arrRightBack);
            }
            else {
                lr.SetPosition(6, dir);
                lr.SetPosition(7, dir);
                lr.SetPosition(8, dir);
                lr.SetPosition(9, dir);
            }
        }
    }
    #region UI Callbacks
    public void UpdateArrowSize(float f) {
        arrowSize = f;
    }
    public void UpdateArrowAngle(float f) {
        arrowAngle = f;
    }
    public void UpdateArrowOffset(float f) {
        arrowOffset = f;
    }

    #endregion

}

public class VisualNode {
    public Node n;
    public GameObject go;
}
public class VisualConnection {
    public Connection c;
    public GameObject cube;
    public TextMeshPro tmp;
    public GameObject go;
    public LineRenderer lr;
    public VisualNode fromVN;
    public VisualNode toVN;
}