using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class DijkstraAssignment {
    public Dictionary<int,int> priceToGetThere=new Dictionary<int, int>();      //tu jest wynik algorytmu
    public List<int> dijkstraTrack = new List<int>();                           //to jest żeby wiedzieć na jakim etapie algorytmu jesteśmy
    public List<string> dijkstraLogs = new List<string>();                      //to jest żeby wiedzieć co się wydarzyło (w sumie to się nie kożysta w praktyce z tego)
    public List<string> dijkstraMiniLogs = new List<string>();                  //to jest żeby wyświetlać na mini ekraniku info w prawym dolnym rogu
    public bool makeNextStep=false;
    public bool fastForward=false;
}

public partial class DijkstraManager{

    //Default graph for algorithm 
    public Graph graph;
    //This function executes when a button on UserInterface is pressed
    public void OnAlgorithmPerformButton() {
        //Coroutines are functions that can be paused for things like rendering
        StartCoroutine(PerformDijkstra());
    }

    //This is a first step of Dijkstra Algorithm
    IEnumerator PerformDijkstra() {
        //this is a sample wait command
        yield return new WaitForEndOfFrame();
        //anything that is between such is percepted by player as immediate or pararelly-done

        Node head=graph.nodes.FirstOrDefault(n=>n.nodeID==graph.head);          //wyciągnięcie zmiennej, żeby była pod ręką
        int totalCost=0;
        dijkstraAssignment = new DijkstraAssignment();
        dijkstraAssignment.priceToGetThere.Add(head.nodeID, totalCost);         //ustalenie że cena dojścia do wierzchołka początkowego to 0
        dijkstraAssignment.dijkstraTrack.Add(head.nodeID);                      //ustalenie że pierwszym wierzchołkiem w danym przejściu jest głowa grafu

        dijkstraAssignment.dijkstraLogs.Add($"Dijkstra algorithm was started, starting point is {head.nodeID}");
        dijkstraAssignment.dijkstraMiniLogs.Add($"Dijkstra algorithm was started, starting point is {head.nodeID}");

        //Czekaj dopóki gracz nie wciśnie przycisku Play, lub tego do przewijania
        yield return new WaitWhile(() => !dijkstraAssignment.makeNextStep && !dijkstraAssignment.fastForward);
        //Odchacz od razu że przejście do następnego kroku zostało wykonane
        dijkstraAssignment.makeNextStep = false;

        foreach(Connection con in head.myOutcommingConnections) {
            //dla każdego połączenia wychodządzego z pierwszego wierzchołka, rekurencyjnie uruchom przeszukiwanie poddrzewa, gdzie głową jest ten nowy wierzchołek
            //Jednocześnie pamiętając skąd poddrzewo pochodzi.
            yield return StartCoroutine(PerformSubDijkstra(dijkstraAssignment, graph.GetNode(con.otherNode(head.nodeID)), totalCost + con.weight));
        }
        //po zakończeniu algorytmu wyświetl że wszystko jest ok
        dijkstraAssignment.dijkstraMiniLogs.Clear();
        dijkstraAssignment.dijkstraMiniLogs.Add("Algorithm has been succesfully finished");
    }
    IEnumerator PerformSubDijkstra(DijkstraAssignment da, Node n, int totalCost) {
        //Dodaj siebie do aktualnej ścieżki np.[0,2,3,tutaj]
        da.dijkstraTrack.Add(n.nodeID);
        //Wyczyść ekranik w prawym dolnym rogu
        da.dijkstraMiniLogs.Clear();
        //Wyświetl tam nową wartość
        //  Spreparuj stringa
        string s=$"Moved to node with ID {n.nodeID}\n\tcurrent cost is {totalCost}\n\tpath is {Graph.NicelyFormatedIntList(da.dijkstraTrack)}";
        //  wrzuć go do logów
        da.dijkstraLogs.Add(s);
        //  wrzuć go na ekran
        da.dijkstraMiniLogs.Add(s);

        //Po wyświetleniu informacji gdzie jesteśmy i jaki jest stan aktualnej sytuacji, poczekaj na play lub fastForward
        yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
        da.makeNextStep = false;

        if(da.priceToGetThere.ContainsKey(n.nodeID)) {                                      //optimize cost eventually
            int currentCost=da.priceToGetThere[n.nodeID];                                   //get current cost to get to current node
            if(totalCost < currentCost) {                                                   //if new cost is better
                da.dijkstraLogs.Add($"Node was already visited, but cost was updated");
                da.dijkstraMiniLogs.Add($"Node was already visited, but cost was updated"); //show this on screen
                da.priceToGetThere[n.nodeID] = totalCost;                                   //and update cost
                yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);      //let player see the update
                da.makeNextStep = false;
                foreach(Connection con in n.myOutcommingConnections) {                      //update subtree
                    yield return StartCoroutine(PerformSubDijkstra(da, graph.GetNode(con.otherNode(n.nodeID)), totalCost + con.weight));
                }
            }
            else {
                da.dijkstraLogs.Add($"Node already have better (or equal) cost");
                da.dijkstraMiniLogs.Add($"Node already have better (or equal) cost");
                yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);
                da.makeNextStep = false;
            }
        }
        else {                                                                              //this node is visited first time
            da.dijkstraLogs.Add($"Node was visited first time - cost is set");
            da.dijkstraMiniLogs.Add($"Node was visited first time - cost is set");
            da.priceToGetThere[n.nodeID] = totalCost;                                       //set cost
            yield return new WaitWhile(() => !da.makeNextStep && !da.fastForward);          //let player see the cost
            da.makeNextStep = false;
            foreach(Connection con in n.myOutcommingConnections) {                          //generate subtree
                yield return StartCoroutine(PerformSubDijkstra(da, graph.GetNode(con.otherNode(n.nodeID)), totalCost + con.weight));
            }
        }
        da.dijkstraTrack.Remove(n.nodeID);                                                  //when finished processing subtree, go back in direction to the root
    }
}
