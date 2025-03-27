using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapLocation
{
    public float x;
    public float y;

    public MapLocation(float _x, float _y)
    {
        x = _x;
        y = _y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public static MapLocation operator + (MapLocation a, MapLocation b) => new MapLocation(a.x + b.x, a.y + b.y);
}

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class Pathfinding : MonoBehaviour
{
    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public List<Vector2> pathFound = new List<Vector2>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;
    bool done = false;
    public bool startSearch = false;

    public List<MapLocation> directions = new List<MapLocation>()
    {
        new MapLocation(1,0),
        new MapLocation(0,1),
        new MapLocation(-1,0),
        new MapLocation(0,-1)
    };

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach(GameObject m in markers)
        {
            Destroy(m);
        }
    }

    public void SetStartAndGoalNodes(Transform startGO, Transform endGO)
    {
        //Set start location to the location of the passed in go (the enemy pathfinding)
        Vector2 startLocation = new Vector2(startGO.position.x, startGO.position.y);
        startNode = new PathMarker(new MapLocation(startLocation.x, startLocation.y), 0, 0, 0, Instantiate(start, startLocation, Quaternion.identity), null);

        //Set end location to the location of the passed in go (Currently the closest player)
        Vector2 goalLocation = new Vector2(endGO.position.x, endGO.position.y);
        goalNode = new PathMarker(new MapLocation(goalLocation.x, goalLocation.y), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);

        //Add start pos to list of open markers, set lastPos = startNode
        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;

        startSearch = true;
        while (startSearch)
        {
            Search(lastPos);
        }
    }

    public void Search(PathMarker thisNode)
    {
        if (thisNode == null) return;
        if ((goalNode.location.x == thisNode.location.x) && (goalNode.location.y == thisNode.location.y))
        {
            done = true;
            startSearch = false;
            Debug.Log("Search done");
            GetPath();
            return;
        }

        foreach(MapLocation dir in directions)
        {
            MapLocation neighbour = dir + thisNode.location;
            //Conditionals to have this loop pass over the neighbour, such as if it's closed or if it's a wall
            if (IsClosed(neighbour)) continue;

            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G; //Distance from starting node to neighbour
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector()); //Distance from neighbour to end node
            float F = G + H;

            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x, neighbour.y, 0), Quaternion.identity);

            if (!UpdateMarker(neighbour, G, H, F, thisNode))
            {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        closed.Add(pm);
        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material.color = Color.gray;

        lastPos = pm;
        //print("Goal node: " + new Vector2(goalNode.location.x, goalNode.location.y));
        //print("ThisNode: " + new Vector2(thisNode.location.x, thisNode.location.y));
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach(PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach(PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;

        while(!startNode.Equals(begin) && begin != null)
        {
            pathFound.Add(new Vector2(begin.location.x, begin.location.y));
            Instantiate(pathP, new Vector3(begin.location.x, begin.location.y, 0), Quaternion.identity);
            begin = begin.parent;
        }

        Instantiate(pathP, new Vector3(startNode.location.x, startNode.location.y, 0), Quaternion.identity);
/*        foreach (Vector2 location in pathFound)
        {
            Debug.Log(location);
        }*/
    }

    private void Update()
    {
        if (!startSearch) return;
        if(startSearch == true)
        {
            Search(lastPos);
        }
    }
}