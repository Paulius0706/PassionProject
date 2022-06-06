using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemHolder : MonoBehaviour
{
    public GameObject[] placeHolder;
    private int cursor;


    public RectTransform itemsScrollbarCursor;
    public RectTransform itemsScrollbarBackgruond;
    public GameObject itemsScrollbarFrame;
    public GameObject itemSlotFrame;
    public float itemsScrollSize;

    public Animator itemAnimator;
    public Animator playerAnimator;
    public PlayerMovement playerMovement;
    public ChunkGenerator chunkGenerator;



    public bool itemActivationMoveStop;
    public bool itemIsActive;
    public bool canBeChanged;
    
    public ItemType itemType;

    public enum ItemType
    {
        none = 0,
        sword = 1,
        picaxe = 2,
        axe = 3,
        block = 4
    }
    
    // Start is called before the first frame update
    void Start()
    {
        cursor = 0;
        DrawScroolBar();
        ChangeItem();
        itemActivationMoveStop = false;
        canBeChanged = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            switch (itemType)
            {
                case ItemType.sword:
                    transform.GetChild(0).GetComponent<SwordController>().Activate(true);
                    break;
            }
        }
        else
        {
            switch (itemType)
            {
                case ItemType.sword:
                    transform.GetChild(0).GetComponent<SwordController>().Activate(false);
                    
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            switch (itemType)
            {
                case ItemType.block:
                    AddBlock();
                    break;
            }
        }
        Scrolling();
    }
    private void Scrolling()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && canBeChanged)
        {
            cursor--;
            cursor = cursor < 0 ? placeHolder.Length - 1 : cursor;
            ChangeItem();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && canBeChanged)
        {
            cursor++;
            cursor = cursor >= placeHolder.Length ? 0 : cursor;
            ChangeItem();
        }
    }
    private void ChangeItem()
    {
        canBeChanged = true;
        itemActivationMoveStop = false;
        RemoveHeldItem();
        PlaceHeldItem();
        SetScroolBar();
    }
    private void RemoveHeldItem()
    {
        if (transform.childCount != 0)
        {
            //Debug.Log("Removing: " + transform.GetChild(0).gameObject.name);
            Destroy(transform.GetChild(0).gameObject);
        }
    }
    private void PlaceHeldItem()
    {
        if(placeHolder[cursor] != null)
        {
            GameObject item = Instantiate(placeHolder[cursor], Vector3.zero, Quaternion.identity);
            item.transform.parent = transform;
            item.transform.localPosition = placeHolder[cursor].transform.localPosition;
            item.transform.localRotation = placeHolder[cursor].transform.localRotation;
            if (item.GetComponent<SwordController>() != null)
            {
                itemType = ItemType.sword;
                itemAnimator = item.GetComponent<Animator>();
                item.GetComponent<SwordController>().chunkGenerator = chunkGenerator;

                SetSwordAnimationSpeed(item);
            }
            else if(item.tag == "Block")
            {
                itemType = ItemType.block;
                item.GetComponent<Collider>().enabled = false;
                item.transform.localPosition = new Vector3(0.5f, 0f, 0.5f);
                item.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                
            }
            else
            {
                itemType = ItemType.none;
            }

        }
        else { itemType = ItemType.none; }
    }
    private void SetScroolBar()
    {
        float delta = itemsScrollSize;
        float ancor = itemsScrollbarBackgruond.position.x - itemsScrollbarBackgruond.sizeDelta.x / 2f + itemsScrollSize/2f;
        itemsScrollbarCursor.position = new Vector3(ancor + delta*cursor, itemsScrollbarBackgruond.position.y, itemsScrollbarCursor.position.z);

    }
    private void DrawScroolBar()
    {
        itemsScrollbarBackgruond.sizeDelta = new Vector2(itemsScrollSize * placeHolder.Length, itemsScrollSize);
        itemsScrollbarCursor.sizeDelta = new Vector2(itemsScrollSize, itemsScrollSize);


        float ancor =  - itemsScrollbarBackgruond.sizeDelta.x / 2f + itemsScrollSize / 2f;
        float delta = itemsScrollSize;
        for (int i = 0; i < placeHolder.Length; i++)
        {
            GameObject frame = Instantiate(itemSlotFrame);
            frame.GetComponent<RectTransform>().SetParent(itemsScrollbarFrame.GetComponent<RectTransform>());
            frame.GetComponent<RectTransform>().localPosition = new Vector3(ancor+delta*i ,0f,0f);
        }
    }


    private void SetSwordAnimationSpeed(GameObject item)
    {
        int index = transform.childCount - 1;
        playerAnimator.SetFloat("AttackMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        playerAnimator.SetFloat("BackAttackMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        playerAnimator.SetFloat("RetrackSwordMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        playerAnimator.SetFloat("RetrackBackSwordMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        playerAnimator.SetFloat("SwordPreperationMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        itemAnimator.SetFloat("AttackMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        itemAnimator.SetFloat("BackAttackMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        itemAnimator.SetFloat("RetrackSwordMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        itemAnimator.SetFloat("RetrackBackSwordMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
        itemAnimator.SetFloat("SwordPreperationMul", transform.GetChild(index).GetComponent<SwordController>().attackSpeed);
    }

    private void AddBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 hitLocation = hit.point;
            Vector3 blockLocation = hit.transform.position;
            Vector3 des = hitLocation - blockLocation;
            if (Mathf.Abs(des.x) <0.5f && Mathf.Abs(des.z) <0.5f)
            {
                if (des.y > 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.up  , placeHolder[cursor].name);
                if (des.y < 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.down, placeHolder[cursor].name);
            }
            if (Mathf.Abs(des.y) < 0.5f && Mathf.Abs(des.z) < 0.5f)
            {
                if (des.x > 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.right, placeHolder[cursor].name);
                if (des.x < 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.left , placeHolder[cursor].name);
            }
            if (Mathf.Abs(des.y) < 0.5f && Mathf.Abs(des.x) < 0.5f)
            {
                if (des.z > 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.forward, placeHolder[cursor].name);
                if (des.z < 0) chunkGenerator.PlayerAddBlock(Vector3Int.RoundToInt(blockLocation) + Vector3Int.back   , placeHolder[cursor].name);
            }
        }
    }
}
