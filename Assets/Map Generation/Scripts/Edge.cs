using System.Collections.Generic;

public class Edge
{
    List<GridPosition> path;

    EdgeMonoBehaviour edgeMonobehaviour;
    GridPosition edgeConnectorForChamberA;
    GridPosition edgeConnectorForChamberB;
    Chamber chamberA;
    Chamber chamberB;
    int edgeCost;

    public Edge(
        List<GridPosition> path,
        GridPosition edgeConnectorForChamberA,
        GridPosition edgeConnectorForChamberB,
        Chamber chamberA,
        Chamber chamberB,
        int edgeCost
        )
    {
        this.path = path;
        this.edgeConnectorForChamberA = edgeConnectorForChamberA;
        this.edgeConnectorForChamberB = edgeConnectorForChamberB;
        this.chamberA = chamberA;
        this.chamberB = chamberB;
        this.edgeCost = edgeCost;
    }

    public GridPosition GetEdgeConnectorForChamberA() => edgeConnectorForChamberA;
    public GridPosition GetEdgeConnectorForChamberB() => edgeConnectorForChamberB;
    public Chamber GetChamberA() => chamberA;
    public Chamber GetChamberB() => chamberB;
    public int GetEdgeCost() => edgeCost;
    public void SetEdgeMonobehaviour(EdgeMonoBehaviour newBehaviour) => edgeMonobehaviour = newBehaviour;
    public EdgeMonoBehaviour GetEdgeMonobehaviour() => edgeMonobehaviour;
}