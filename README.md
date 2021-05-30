# Projekt zaliczeniowy z Teorii Grafów
#### by Krzysztof Łazarz (ISI 2021)
---

##### Części nieprogramistyczne są zawarte w PDF'ie na samym wierzchu repozytorium  
##### Interesująca część kodu znajduje się [tu](../master/Assets/Dijkstra/Scripts/DijkstraCoreAlgorithm.cs)
---
## Uruchomienie:
### Uruchomić projekt można na 2 sposoby:
#### Sposób trudniejszy (0.5~1.5h zakładając dobry internet, komputer):
* pobrać UnityHub
* założyć konto Unity
* przy pomocy hub'a zainstalować Unity w wersji 2019.4.X
* sklonować repo
* uruchomić w hubie wskazać folder jako projekt
* zbuildować projekt i uruchomić
#### Sposób prostrzy (mniej niż minuta):
* pobrać zipa, rozpakować, uruchomić exe'ka
## Testowanie:
* wcisnąć przycisk ze słowem Dijkstra
* kliknąć przycisk Perform, a następnie przycisk "play" wielokrotnie
* można wgrać własnego JSON'a i powtórzyć krok 2
## Wgrywanie własnego JSON'a
W projekcie jest wbudowany graficzny kreator grafów, zaleca się kożystanie z niego.  
Należy przeczytać instrukcje sterowania zawarte z lewej strony ekranu.  
Po każdej zmianie JSON jest generowany, dzięki czemu można go zapisać, ewentualnie zmodyfikować i wgrać ponownie przy pomocy pola tekstowego poniżej
### Format JSON'a
Generowanie JSON'a ręczne wcale nie jest takie przyjemne.
Przy wczytywaniu jest regenerowana klasa Graph. Składa się ona z listy klasy Node oraz listy klasy Connection. Każdy Node posiada identyfikator oraz współrzędne w przestrzeni. Pozycja jest określona w płaszczyźnie XZ, gdzie X jest "w prawo" a Z "w górę". Zmienna ta nie jest wymagana do poprawnego działania algorytmu i nie trzeba jej podawać.
Zmienne w Connection mówią za siebie. Nie należy robić dwóch jednostronnych połączeń między dwoma wierzchołkami. Zamiast tego należy zrobić tylko jedno z nich i ustawić flagę bidirectional na true. Zmienna Head wybiera wierzchołek początkowy z pasującym identyfikatorem.
Przykładowy JSON:
```
{
    "head": 0,
    "nodes": [
        {
            "nodeID": 0,
            "position": {
                "x": 0,
                "y": 0,
                "z": 0
            }
        },
        {
            "nodeID": 1,
            "position": {
                "x": 200,
                "y": 0,
                "z": 200
            }
        },
        {
            "nodeID": 2,
            "position": {
                "x": -100,
                "y": 0,
                "z": 270
            }
        },
        {
            "nodeID": 3,
            "position": {
                "x": 270,
                "y": 0,
                "z": -100
            }
        }
    ],
    "connections": [
        {
            "connectionID": 0,
            "fromNode": 0,
            "toNode": 1,
            "bidirectional": false,
            "weight": 7
        },
        {
            "connectionID": 1,
            "fromNode": 1,
            "toNode": 2,
            "bidirectional": true,
            "weight": 3
        },
        {
            "connectionID": 2,
            "fromNode": 0,
            "toNode": 2,
            "bidirectional": false,
            "weight": 2
        },
        {
            "connectionID": 3,
            "fromNode": 0,
            "toNode": 3,
            "bidirectional": false,
            "weight": 4
        },
        {
            "connectionID": 4,
            "fromNode": 3,
            "toNode": 1,
            "bidirectional": false,
            "weight": 0
        }
    ]
}
```
Takie uproszczone wierzchołki też działają:
```
    "nodes": [
        {
            "nodeID": 0,
        },
        {
            "nodeID": 1,
        },
        {
            "nodeID": 2,
        },
        {
            "nodeID": 3,
        }
    ]
```
