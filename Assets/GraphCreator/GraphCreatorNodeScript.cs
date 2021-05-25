using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphCreatorNodeScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseEnter() {
        FindObjectOfType<GraphCreatorCameraController>().SetMeAsVirtualNode(this.gameObject);
    }
    private void OnMouseExit() {
        FindObjectOfType<GraphCreatorCameraController>().UnSetMeAsVirtualNode(this.gameObject);
    }
}
