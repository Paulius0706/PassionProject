using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LandMobMovement : MonoBehaviour
{

    public Transform target;
    private Vector3Int lastTargetLocation;

    public Vector3 groundPositionOffset;
    private Vector3 groundPosition;
    
    public float maxSpeed;
    public float acceleration;
    public float jumpPower;
    public ChunkGenerator chunkGenerator;

    // not impemented but it will work with path.last()
    public int size;
    
    private List<Vector3Int> path;
    private Rigidbody rb;
    private bool jump;
    // Start is called before the first frame update
    void Start()
    {
        groundPosition = transform.position + groundPositionOffset;
        rb = GetComponent<Rigidbody>();
        path = new List<Vector3Int>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        groundPosition = transform.position + groundPositionOffset;
        Movement();
        Jumping();
    }
    private void Movement()
    {
        // flaoting (needs remeve y movement)
        if ((target.position - lastTargetLocation).magnitude > 2)
        {
            AStar();
            lastTargetLocation = Vector3Int.RoundToInt(target.position);
        }
        if (path.Count > 0 && Vector3Int.RoundToInt(groundPosition) == path.Last())
        {
            path.RemoveAt(path.Count - 1);
            if (path.Count == 0) AStar();
        }
        Vector3 target1;
        if(path.Count > 0)
        {
            target1 = path.Last();
            jump = target1.y - Vector3Int.RoundToInt(groundPosition).y == 1;
            target1.y = groundPosition.y;
        }
        else
        {
            target1 = target.position - Vector3.up * target.position.y + Vector3.up * groundPosition.y;
        }
        rb.AddForce(
            Vector3.MoveTowards(
                rb.velocity - rb.velocity.y *Vector3.up, 
                (target1 - groundPosition).normalized * maxSpeed, 
                acceleration*Time.deltaTime) - (rb.velocity - rb.velocity.y * Vector3.up), 
            ForceMode.VelocityChange);
    }
    private void Jumping()
    {
        if (jump)
        {
            jump = false;
            //rb.MovePosition(rb.position + Vector3.up);
            rb.AddForce((jumpPower - rb.velocity.y) * Vector3.up, ForceMode.VelocityChange);
        }
    }

    private void AStar()
    {
        Dictionary<Vector3Int,Node> openNodes = new Dictionary<Vector3Int, Node>();
        Dictionary<Vector3Int, Node> closedNodes = new Dictionary<Vector3Int, Node>();
        openNodes.Add(Vector3Int.RoundToInt(groundPosition), 
            new Node(Vector3Int.RoundToInt(groundPosition),
                Vector3Int.RoundToInt(groundPosition), 
                (Vector3Int.RoundToInt(target.position) - Vector3Int.RoundToInt(groundPosition)).sqrMagnitude, 
                0f));
        Vector3Int blockPos;
        Vector3Int AStarTarget = Vector3Int.RoundToInt(transform.position);
        int nodeCounter = 0;
        while (nodeCounter < 100)
        {
            nodeCounter++;
            if (openNodes.Count == 0 || nodeCounter == 100)
            {
                //Debug.Log("Path is not found");
                float minimumHCost = closedNodes.Values.Select(node => node.hCost).Min();

                AStarTarget = closedNodes.Count >0 ? closedNodes.Values.Where(node => node.hCost < minimumHCost + 0.1f).First().pos : Vector3Int.RoundToInt(transform.position);
                break;
            }
            float minimumFCost = openNodes.Values.Select(node => node.hCost + node.gCost).Min();
            Vector3Int currentIndex = openNodes.Values.Where(node => node.hCost + node.gCost < minimumFCost + 0.1f).First().pos;
            Node currentNode = openNodes[currentIndex];
            openNodes.Remove(currentIndex);
            closedNodes.Add(currentIndex,currentNode);

            if (currentNode.hCost < 0.1f)
            {
                //Debug.Log("Path is found");
                AStarTarget = currentNode.pos;
                break;
            }

            for(int x = -1; x <= 1; x++)
                for (int z = -1; z <= 1; z++)
                    for (int y = -1; y <= 1; y++)
                    {
                        blockPos = currentNode.pos + Vector3Int.right * x + Vector3Int.forward * z + Vector3Int.up *y;
                        if ((chunkGenerator.IsAnyBlockPalceExist(blockPos + Vector3Int.down)||y==-1)
                            && !chunkGenerator.IsAnyBlockPalceExist(blockPos)
                            && !closedNodes.ContainsKey(blockPos))
                        {
                            // is in open and if shorter
                            if (openNodes.ContainsKey(blockPos) && openNodes[blockPos].gCost > currentNode.gCost + (currentNode.pos - blockPos).magnitude)
                            {
                                openNodes[blockPos].gCost = currentNode.gCost + (currentNode.pos - blockPos).magnitude;
                                openNodes[blockPos].lastPos = currentNode.pos;
                            }
                            // if not in open
                            else if(!openNodes.ContainsKey(blockPos))
                            {
                                openNodes.Add(blockPos,
                                    new Node(blockPos,
                                        currentNode.pos, 
                                        (Vector3Int.RoundToInt(target.position) - Vector3Int.RoundToInt(blockPos)).magnitude,
                                        currentNode.gCost + (currentNode.pos - blockPos).magnitude));
                            }
                        }
                    }
        }
        path = new List<Vector3Int>();
        Vector3Int pathIndex = AStarTarget;
        while (closedNodes[pathIndex].lastPos != Vector3Int.RoundToInt(groundPosition))
        {
            path.Add(closedNodes[pathIndex].pos);
            pathIndex = closedNodes[pathIndex].lastPos;
        }
    }

    private protected class Node
    {
        public Vector3Int pos;
        public Vector3Int lastPos;
        public float hCost;
        public float gCost;

        public Node(Vector3Int pos, Vector3Int lastPos, float hCost, float gCost)
        {
            this.pos = pos;
            this.lastPos = lastPos;
            this.hCost = hCost;
            this.gCost = gCost;
        }
    }
}
