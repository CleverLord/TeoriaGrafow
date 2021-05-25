using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphCreatorCameraController : MonoBehaviour
{
    public bool draging=false;
    public RaycastHit baseHit;
    public GameObject visualNode;
    public Vector3 basePosition;
    [Range(0,3)]
    public float speed=5;
    public bool queueUnset=false;
    public Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam=GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMove();
        ProcessDrag();
        if(draging==false&&queueUnset) {
            visualNode=null;
            queueUnset=false;
        }
    }
    
    public void SetMeAsVirtualNode(GameObject me) {
        queueUnset=false;
        if(draging==false)
            visualNode=me;
    }
    public void UnSetMeAsVirtualNode(GameObject me) {
        queueUnset=true;
    }

    void ProcessDrag() {
        if(!visualNode) 
            return;
        if(Input.GetKeyDown(KeyCode.Mouse0))
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out baseHit)) {
                draging=true;
                basePosition=visualNode.transform.position;
            }
        if(Input.GetKeyUp(KeyCode.Mouse0))
            draging=false;
        if(draging==false) return;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
            Vector3 delta=hit.point-baseHit.point;
            Vector3 targetPosition=basePosition+delta;
            targetPosition.y=0;
            visualNode.transform.position=targetPosition;
        }

    }

    void ProcessMove() {
        Vector3 delta=Vector3.zero;
        delta.x=Input.GetAxisRaw("Horizontal");
        delta.z=Input.GetAxisRaw("Vertical");
        transform.position+=delta*Time.deltaTime*speed*cam.orthographicSize;
        float mouseScrool= Mathf.Clamp(-Input.mouseScrollDelta.y,-1,1);
        cam.orthographicSize+=cam.orthographicSize*mouseScrool*0.1f;
    }
    public void UpdateCameraSpeed(float f) {
        speed=f;
    }
}
