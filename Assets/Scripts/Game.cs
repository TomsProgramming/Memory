using UnityEngine;
using System.Collections.Generic;

public enum GameStatus
{
    waiting_on_first_card,
    waiting_on_second_card,
    match_found,
    no_match_found
}

public class Game : MonoBehaviour
{
    [SerializeField] private GameStatus status;

    [SerializeField] private GameObject[] selectedCards;

    [SerializeField] private int rows;
    [SerializeField] private int columns;

    [SerializeField] private float totalPairs;

    [SerializeField] private string frontsidesFolder = "Sprites/Frontsides/";
    [SerializeField] private string backsidesFolder = "Sprites/Backsides/";

    [SerializeField] private Sprite[] frontSprites;
    [SerializeField] private Sprite[] backSprites;

    [SerializeField] private List<Sprite> selectedFrontSprites;
    [SerializeField] private Sprite selectedBackSprite;

    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Stack<GameObject> stackOfCards;
    [SerializeField] private GameObject[,] placedCards;

    [SerializeField] private Transform fieldAnchor;

    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    [SerializeField] private float timeoutTimer;
    [SerializeField] private float timeoutTarget;


    void Start()
    {
        MakeCards();
        DistributeCards();

        selectedCards = new GameObject[2];
        status = GameStatus.waiting_on_first_card;
    }

    void Update()
    {
        if (status == GameStatus.match_found || status == GameStatus.no_match_found)
        {
            RotateBackOrRemovePair();
        }
    }

    private void MakeCards()
    {
        CalculateAmountOfPairs();
        LoadSprites();
        SelectFrontSprites();
        SelectBackSprite();
        ConstructCards();
    }


    private void DistributeCards()
    {
        placedCards = new GameObject[rows, columns];
        ShuffleCards();
        PlaceCardsOnField();
    }

    private void CalculateAmountOfPairs()
    {
        if (rows * columns % 2 == 0)
        {
            totalPairs = rows * columns / 2;
        }
        else
        {
            Debug.LogError("Je kunt geen memory spelen met een oneven hoeveelheid kaarten.");
        }
    }

    private void LoadSprites()
    {
        frontSprites = Resources.LoadAll<Sprite>(frontsidesFolder);
        backSprites = Resources.LoadAll<Sprite>(backsidesFolder);
    }

    private void SelectFrontSprites()
    {
        if (frontSprites.Length < totalPairs)
        {
            Debug.LogError("Er zijn te weinig plaatjes om " + totalPairs + " paren te maken");
        }

        selectedFrontSprites = new List<Sprite>();

        while (selectedFrontSprites.Count < totalPairs)
        {
            int rnd = Random.Range(0, frontSprites.Length);
            if (selectedFrontSprites.Contains(frontSprites[rnd]) == false)
            {
                selectedFrontSprites.Add(frontSprites[rnd]);
            }
        }
    }

    private void SelectBackSprite()
    {
        if (backSprites.Length == 0)
        {
            Debug.LogError("Er zijn geen achterkant plaatjes om te selecteren.");
        }

        int rnd = Random.Range(0, backSprites.Length);
        selectedBackSprite = backSprites[rnd];
    }

    private void ConstructCards()
    {
        stackOfCards = new Stack<GameObject>();

        foreach (Sprite selectedFrontSprite in selectedFrontSprites)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject go = Instantiate(cardPrefab);
                Card cardScript = go.GetComponent<Card>();

                cardScript.SetBack(selectedBackSprite);
                cardScript.SetFront(selectedFrontSprite);

                go.name = selectedFrontSprite.name;

                stackOfCards.Push(go);
            }
        }
    }

    private void ShuffleCards()
    {
        while (stackOfCards.Count > 0)
        {
            int randX = Random.Range(0, columns);
            int randY = Random.Range(0, rows);

            if (placedCards[randY, randX] == null)
            {
                Debug.Log("kaart " + stackOfCards.Peek().name + " is geplaatst op x: " + randX + " y: " + randY);

                placedCards[randY, randX] = stackOfCards.Pop();
            }
        }
    }

    private void PlaceCardsOnField()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject card = placedCards[y, x];
                Card cardScript = card.GetComponent<Card>();

                Vector2 cardSize = cardScript.GetBackSize();

                float posX = fieldAnchor.position.x + (x * (cardSize.x + offsetX));
                float posY = fieldAnchor.position.y + (y * (cardSize.y + offsetY));

                placedCards[y, x].transform.position = new Vector3(posX, posY, 0f);
            }
        }
    }

    public void SelectCard(GameObject card)
    {
        if (status == GameStatus.waiting_on_first_card)
        {
            selectedCards[0] = card;
            status = GameStatus.waiting_on_second_card;
        }
        else if (status == GameStatus.waiting_on_second_card)
        {
            selectedCards[1] = card;
            CheckForMatchingPair();
        }
    }

    private void CheckForMatchingPair()
    {
        timeoutTimer = 0f;

        if (selectedCards[0].name == selectedCards[1].name)
        {
            status = GameStatus.match_found;
        }
        else
        {
            status = GameStatus.no_match_found;
        }
    }

    private void RotateBackOrRemovePair()
    {
        timeoutTimer += Time.deltaTime;

        if (timeoutTimer >= timeoutTarget)
        {
            if (status == GameStatus.match_found)
            {
                selectedCards[0].SetActive(false);
                selectedCards[1].SetActive(false);
            }
            else if (status == GameStatus.no_match_found)
            {
                selectedCards[0].GetComponent<Card>().TurnToBack();
                selectedCards[1].GetComponent<Card>().TurnToBack();
            }

            selectedCards[0] = null;
            selectedCards[1] = null;
            status = GameStatus.waiting_on_first_card;
        }
    }

    public bool AllowedToSelectCard(Card card)
    {
        if (selectedCards[0] == null)
        {
            return true;
        }

        if (selectedCards[1] == null)
        {
            if (selectedCards[0] != card.gameObject)
            {
                return true;
            }
        }

        return false;
    }
}
