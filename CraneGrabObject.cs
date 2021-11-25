using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraneGrabObject : MonoBehaviour
{

    public List<GameObject> buildingBlocksNotUsed = new List<GameObject>();
    public List<GameObject> buildingBlocksUsed = new List<GameObject>();
    public GameObject blockStartPoint;
    public GameObject topOfCrane;
    public GameObject rotatePoint;
    public BoxCollider CamMoveLine;
   // public GameObject endOfLine;
    public LineRenderer lr;
    public FixedJoint sj;
    public GameObject magnet;
    public float downForce;
    public float rotateForce;
    private FixedJoint fj;
    private bool hasBlock;
    private GameObject chosenBlock;
    private GameObject previousBlock;
    private GameObject oldBlock;
    private GameObject olderBlock;
    private int index;
    private bool attatchLine;
    public bool boxFalling;
    private bool canDropBlock;
    private bool swayCraneLeft;
    private bool swayCraneRight;
    private Rigidbody rb;
    private Vector3 chosenBlockRotation;
    public int blocksDropped;
    // Start is called before the first frame update


    //SOMETIMES THE COLLIDIERS FALL THROUGH EACH OTHER WHEN STACKED, THEY ALSO SLIDE SLIGHTLY TO A DIRECTION AND CANNOT BE STOPPED


    void Start()
    {
        blocksDropped = 0;
        swayCraneLeft = true;
        swayCraneRight = false;
        canDropBlock = false;
        hasBlock = false;
        attatchLine = false;
        boxFalling = false;
        //detatchLine = false;
        if (buildingBlocksUsed.Count >= 1)
        {
            foreach (GameObject blocks in buildingBlocksUsed)
            {
                chosenBlock = blocks;
                buildingBlocksNotUsed.Add(chosenBlock);
                buildingBlocksUsed.Remove(chosenBlock);
            }
        }
        StartCoroutine(GetBlock());
        Physics.gravity = new Vector3(0, -downForce, 0);
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(0, topOfCrane.transform.position);
        lr.SetPosition(1, blockStartPoint.transform.position);
        sj.connectedBody = blockStartPoint.GetComponent<Rigidbody>();
        if (attatchLine)
        {
            rb.mass = 0.1f;
            if (chosenBlock != null)
            {
                chosenBlock.transform.position = new Vector3(blockStartPoint.transform.position.x, blockStartPoint.transform.position.y - 20f, blockStartPoint.transform.position.z);
                chosenBlock.transform.eulerAngles = chosenBlockRotation;
            } 
        }
        if (oldBlock != null)
        {
            if (!oldBlock.TryGetComponent<FixedJoint>(out FixedJoint fj))
            {
                AddJoint();
            }
        }
        if (olderBlock != null)
        {
            if (olderBlock.TryGetComponent<FixedJoint>(out FixedJoint oldFJ))
            {
                Destroy(oldFJ);
            }
        }
    }

    private void FixedUpdate()
    {

        //magnet.transform.eulerAngles = new Vector3(0, transform.rotation.y, transform.rotation.z);
        if (magnet.GetComponent<magnetCollision>().swayCraneLeft)
        {
            blockStartPoint.transform.RotateAround(topOfCrane.transform.position, transform.forward, rotateForce * Time.fixedDeltaTime);
        }
        else if (magnet.GetComponent<magnetCollision>().swayCraneRight)
        {
            blockStartPoint.transform.RotateAround(topOfCrane.transform.position, transform.forward, -rotateForce * Time.fixedDeltaTime);
        }


         if (boxFalling)
        {
            //rb.AddForce(new Vector3(0, downForce, 0) * rb.mass, ForceMode.Impulse);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            rb.mass = 10f;
            chosenBlock.transform.eulerAngles = chosenBlockRotation;
            rb.constraints = RigidbodyConstraints.FreezePositionZ; //RigidbodyConstraints.FreezeRotationY;
            chosenBlock.GetComponent<Block>().turnOffVel = true;
            StartCoroutine(TurnOffBoxFalling());
        }
    }

    private void AddJoint()
    {
        oldBlock.AddComponent<FixedJoint>();
        oldBlock.GetComponent<FixedJoint>().connectedBody = previousBlock.GetComponent<Rigidbody>();
    }

    IEnumerator TurnOffBoxFalling()
    {
        yield return new WaitForSeconds(1f);
        boxFalling = false;
    }

    IEnumerator CanDropNextBlock()
    {
        yield return new WaitForSeconds(1.5f);
        canDropBlock = true;
    }

    IEnumerator AssignTag()
    {
        yield return new WaitForSeconds(1f);
        if (chosenBlock != null && previousBlock == null && oldBlock == null)
        {
            previousBlock = chosenBlock;
            previousBlock.tag = "PreviousBlock";
            //yield return new WaitForSeconds(1f);
            //previousBlock.tag = "OldBlock";
            //NEED TO DELETE THE PREVIOUS FIXEDJOINT 
        }
        else if (chosenBlock != null && previousBlock != null && oldBlock == null)
        {
            oldBlock = previousBlock;
            oldBlock.tag = "OldBlock";
            previousBlock = chosenBlock;
            previousBlock.tag = "PreviousBlock";
        }
        else if (chosenBlock != null && previousBlock != null && oldBlock != null)
        {
            olderBlock = oldBlock;
            olderBlock.tag = "OlderBlock";
            oldBlock = previousBlock;
            oldBlock.tag = "OldBlock";
            previousBlock = chosenBlock;
            previousBlock.tag = "PreviousBlock";

        }
    }
    IEnumerator GetBlock()
    {
        yield return new WaitForSeconds(1.5f);
        canDropBlock = true;
        if (buildingBlocksNotUsed.Count > 1)
        {
            
            index = Random.Range(0, buildingBlocksNotUsed.Count);
            chosenBlock = buildingBlocksNotUsed[index];
            chosenBlock.tag = "CurrentBlock";
            chosenBlockRotation = chosenBlock.transform.eulerAngles;
            print(chosenBlockRotation);
            chosenBlock.transform.position = new Vector3(blockStartPoint.transform.position.x, blockStartPoint.transform.position.y - 20f, blockStartPoint.transform.position.z);
            buildingBlocksUsed.Add(chosenBlock);
            buildingBlocksNotUsed.Remove(chosenBlock);
            //detatchLine = false;
            fj = magnet.AddComponent<FixedJoint>();
            fj.connectedBody = chosenBlock.GetComponent<Rigidbody>();
            attatchLine = true;
            rb = chosenBlock.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            rb.drag = 2;
        }
        else
        {
             foreach(GameObject blocks in buildingBlocksUsed.ToArray())
            {
                buildingBlocksNotUsed.Add(blocks);
                buildingBlocksUsed.Remove(blocks);
            }
            index = Random.Range(0, buildingBlocksNotUsed.Count);
            chosenBlock = buildingBlocksNotUsed[index];
            chosenBlockRotation = chosenBlock.transform.eulerAngles;
            chosenBlock.transform.position = new Vector3(blockStartPoint.transform.position.x, blockStartPoint.transform.position.y - 20f, blockStartPoint.transform.position.z);
            buildingBlocksUsed.Add(chosenBlock);
            buildingBlocksNotUsed.Remove(chosenBlock);
            fj = magnet.AddComponent<FixedJoint>();
            attatchLine = true;
            rb = chosenBlock.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            rb.drag = 2;
        }

    }
    IEnumerator TurnOnCamLine()
    {
        yield return new WaitForSeconds(1f);
        CamMoveLine.enabled = true;
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && canDropBlock)
        {
            Destroy(fj);
            attatchLine = false;
            boxFalling = true;
            CamMoveLine.enabled = false;
            StartCoroutine(TurnOnCamLine());
            StartCoroutine(GetBlock());
            StartCoroutine(AssignTag());
            StartCoroutine(CanDropNextBlock());
            blocksDropped++;
            canDropBlock = false;
        }
        
    }
}
