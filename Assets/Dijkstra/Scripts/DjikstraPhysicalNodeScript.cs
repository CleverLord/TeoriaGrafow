using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DjikstraPhysicalNodeScript : MonoBehaviour {
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    private void OnMouseEnter() {
        FindObjectOfType<DijkstraCameraController>().SetMeAsVirtualNode(this.gameObject);
    }
    private void OnMouseExit() {
        FindObjectOfType<DijkstraCameraController>().UnSetMeAsVirtualNode(this.gameObject);
    }
}
