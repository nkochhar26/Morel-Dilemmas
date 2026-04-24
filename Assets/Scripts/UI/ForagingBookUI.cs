using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ForagingBookUI : MonoBehaviour
{
    public Image mushroomImage;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public bool isLookalike;
    public int pageIndex;
    public GameObject book;

    private List<MushroomItem> allMushroom = new List<MushroomItem>();
    private List<MushroomItem> lookalikeMushroom = new List<MushroomItem>();

    public void Start()
    {
        book.SetActive(false);
        LoadData();
    }

    public void LoadData()
    {
        allMushroom = GameManager.Instance.mushroomManager.GetAllMushrooms();
        foreach (MushroomItem mushroom in allMushroom)
        {
            if (mushroom.lookalikeName != "")
            {
                lookalikeMushroom.Add(mushroom);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OpenBook()
    {
        isLookalike = false;
        book.SetActive(true);
        LoadPage();
    }

    public void CloseBook()
    {
        book.SetActive(false);
    }

    public void LoadPage()
    {
        MushroomItem mushroom = null;
        if (!isLookalike)
        {
            mushroom = allMushroom[pageIndex];
            title.text = mushroom.itemName;
            description.text = mushroom.itemDescription;
            mushroomImage.sprite = mushroom.defaultSprite;
        }
        else
        {
            mushroom = lookalikeMushroom[pageIndex];
            title.text = mushroom.lookalikeName;
            description.text = mushroom.lookalikeDescription;
            mushroomImage.sprite = mushroom.lookalikeSprite;
        }
    }

    public void SwapLookalike()
    {
        pageIndex = 0;
        isLookalike = true;
        LoadPage();
    }

    public void SwapNormal()
    {
        pageIndex = 0;
        isLookalike = false;
        LoadPage();
    }

    public void NextPage()
    {
        if (!isLookalike)
        {
            if (pageIndex >= allMushroom.Count - 1)
            {
                return;
            }
        }
        else
        {
            if (pageIndex >= lookalikeMushroom.Count - 1)
            {
                return;
            }
        }
        pageIndex += 1;
        LoadPage();
    }

    public void PreviousPage()
    {
        if (pageIndex <= 0)
        {
            return;
        }
        pageIndex -= 1;
        LoadPage();
    }
}
