using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class DijkstraCameraController : MonoBehaviour {
    public DijkstraManager dm;
    public DijkstraGraphVisualizer dgv;
    public bool draging=false;
    public RaycastHit baseHit;
    public VisualNode visualNode;
    public Vector3 basePosition;
    [Range(0,3)]
    public float speed=5;
    public bool queueUnset=false;
    public Camera cam;
    Connection newCon;
    // Start is called before the first frame update
    private int fingerID = -1;
    private void Awake() {

    }
    void Update() {
        
        if(draging == false && queueUnset) {
            visualNode = null;
            queueUnset = false;
        }

        if(EventSystem.current.IsPointerOverGameObject(fingerID)) return;
        ProcessMove();
        ProcessSpawn();
        ProcessDrag();
        ProcessConnect();
        ProcessUpdateJSON();
    }
    void ProcessUpdateJSON() {
        if(Input.GetKeyUp(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse1))
            dm.UpdateJSON();
    }
    void ProcessMove() {
        if(newCon != null) return;

        Vector3 delta=Vector3.zero;
        delta.x = Input.GetAxisRaw("Horizontal");
        delta.z = Input.GetAxisRaw("Vertical");
        transform.position += delta * Time.deltaTime * speed * cam.orthographicSize;
        float mouseScrool= Mathf.Clamp(-Input.mouseScrollDelta.y,-1,1);
        cam.orthographicSize += cam.orthographicSize * mouseScrool * 0.1f;
        //cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 0.1f, 25);
    }
    public void ProcessSpawn() {
        RaycastHit hit;
        if(visualNode == null) {
            if(Input.GetKeyDown(KeyCode.Mouse0))
                if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                    dm.graph.AddNode(hit.point);
                    dgv.Refresh();
                }
        }
        if(Input.GetKey(KeyCode.Mouse0) && Input.GetKeyDown(KeyCode.Mouse1)) {
            draging = false;
            List<Connection> cs=visualNode.n.myRelatedConnections;
            foreach(Connection c in cs)
                dm.graph.connections.Remove(c);
            dm.graph.nodes.Remove(visualNode.n);
            Destroy(visualNode.go);
            visualNode = null;
            dgv.Refresh();
        }
    }
    public void ProcessConnect() {
        if(Input.GetKey(KeyCode.Mouse0)&&!Input.GetKey(KeyCode.Mouse1)) {
            if(newCon != null) {
                dm.graph.connections.Remove(newCon);
                newCon = null;
                dgv.Refresh();
                return;
            }
        }
        if(Input.GetKeyDown(KeyCode.Mouse1) && visualNode != null) {
            newCon = new Connection(visualNode.n.nodeID, -1, false) { isBeingCreated = true, connectionID = dm.graph.firstFreeConnectionID,weight=-1 };
            dm.graph.connections.Add(newCon);
            dgv.Refresh();
        }
        if(Input.GetKey(KeyCode.Mouse1) && newCon != null) {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                newCon.targetPosition = hit.point;
                if((int)Input.mouseScrollDelta.y != 0)
                    newCon.weight = Mathf.Clamp(newCon.weight + (int)Input.mouseScrollDelta.y, 0, int.MaxValue);
            }
        }
        if(Input.GetKeyUp(KeyCode.Mouse1) && newCon != null) {
            if(visualNode == null || visualNode.n.nodeID == newCon.fromNode) {
                dm.graph.connections.Remove(newCon);
                newCon = null;
                dgv.Refresh();
                return;
            }
            newCon.toNode = visualNode.n.nodeID;
            newCon.isBeingCreated = false;
            ApplyNewConnection();
        }
    }
    void ApplyNewConnection() {
        bool revert=false;
        Connection alreadyExisting=dm.graph.connections.FirstOrDefault(c=>c!=newCon&& c.fromNode==newCon.fromNode&&c.toNode==newCon.toNode);
        if(alreadyExisting == null) {
            alreadyExisting = dm.graph.connections.FirstOrDefault(c => c != newCon && c.fromNode == newCon.toNode && c.toNode == newCon.fromNode);
            revert = true;
        }
        /*if(newCon.weight != -1 && alreadyExisting != null) {
            alreadyExisting.weight = newCon.weight;
            dm.graph.connections.Remove(newCon);
            newCon = null;
            dgv.Refresh();
        }
        else */
        if(alreadyExisting != null && revert) {
            if(newCon.weight != -1)
                alreadyExisting.weight = newCon.weight;
            alreadyExisting.bidirectional = !alreadyExisting.bidirectional;
            dm.graph.connections.Remove(newCon);
            newCon = null;
            dgv.Refresh();
        }
        else if(alreadyExisting != null && !revert && !alreadyExisting.bidirectional && newCon.weight != -1) {
            alreadyExisting.weight = newCon.weight;
            dm.graph.connections.Remove(newCon);
            newCon = null;
            dgv.Refresh();
        }
        else if(alreadyExisting != null && !revert && !alreadyExisting.bidirectional && newCon.weight == -1) {
            dm.graph.connections.Remove(newCon);
            dm.graph.connections.Remove(alreadyExisting);
            newCon = null;
            dgv.Refresh();
        }
        else if(alreadyExisting != null && !revert && alreadyExisting.bidirectional) {//invertConnection
            if(newCon.weight != -1)
                alreadyExisting.weight = newCon.weight;
            int temp=alreadyExisting.fromNode;
            alreadyExisting.fromNode = alreadyExisting.toNode;
            alreadyExisting.toNode = temp;
            alreadyExisting.bidirectional = false;
            dm.graph.connections.Remove(newCon);
            newCon = null;
            dgv.Refresh();
        }
        else {//add this connection
            newCon.weight=Mathf.Clamp(newCon.weight,0,int.MaxValue);
            newCon = null;
            dgv.Refresh();
        }
    }

    void ProcessDrag() {
        if(visualNode == null)
            return;
        if(Input.GetKeyDown(KeyCode.Mouse0))
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out baseHit)) {
                draging = true;
                basePosition = visualNode.go.transform.position;
            }
        if(Input.GetKeyUp(KeyCode.Mouse0)||Input.GetKeyDown(KeyCode.Mouse1))
            draging = false;
        if(draging == false) return;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            Vector3 delta=hit.point-baseHit.point;
            Vector3 targetPosition=basePosition+delta;
            targetPosition.y = 0;
            visualNode.go.transform.position = targetPosition;
            visualNode.n.floatingPosition = targetPosition;
        }

    }


    public void SetMeAsVirtualNode(GameObject me) {
        queueUnset = false;
        int idx=-1;
        int.TryParse(me.name, out idx);
        if(idx < 0) return;
        if(draging == false)
            visualNode = dgv.spawnedNodes.FirstOrDefault(s => s.n.nodeID == idx);
    }
    public void UnSetMeAsVirtualNode(GameObject me) {
        queueUnset = true;
    }
    public void UpdateCameraSpeed(float f) {
        speed = f;
    }
}
