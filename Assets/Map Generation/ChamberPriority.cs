public class ChamberPriority
{
    bool isConnectedToStart;
    int distance;

    public ChamberPriority(bool isConnectedToStart, int distance)
    {
        this.isConnectedToStart = isConnectedToStart;
        this.distance = distance;
    }

    public bool IsBestterChamber(ChamberPriority chamber1)
    {
        //If both are connected to start, go based off of lesser distance
        if (!chamber1.isConnectedToStart && !isConnectedToStart)
        {
            return distance < chamber1.distance ? true : false;
        }

        //Go based off of who is not connected
        if (!chamber1.isConnectedToStart) return false;


        //Chamber 1 is connected, chamber 2 isn't
        return true;
    }

    public bool IsConnectedToStart() => isConnectedToStart;
    public int Distance() => distance;
}
