using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class SceneFlipper : MonoBehaviour
{
    public void GoMainMenu() {
        Scene s=SceneManager.GetActiveScene();
        SceneManager.LoadScene("MainMenu");
    }
    public void GoPlayWithGraph() {
        Scene s=SceneManager.GetActiveScene();
        SceneManager.LoadScene("GraphCreatorScene");
    }
    public void GoPerfomDijkstra() {
        Scene s=SceneManager.GetActiveScene();
        SceneManager.LoadScene("DijkstraScene");
    }
    public void GoFullScreen() {
        List<Resolution> r=Screen.resolutions.ToList();
        Resolution rb=r.OrderBy(x => -x.width).ThenBy(x => -x.height).ToList()[0];
        Screen.SetResolution(rb.width, rb.height, true);
    }
    public void GoWindowed() {
        Screen.SetResolution(1280, 720, false);
    }
}
