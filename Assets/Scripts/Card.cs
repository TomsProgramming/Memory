using Unity.VisualScripting;
using UnityEngine;

public enum CardStatus
{
    show_back = 0,
    show_front,
    rotating_to_back,
    rotating_to_front
}

public class Card : MonoBehaviour
{
    [SerializeField] private CardStatus status;

    [SerializeField] private float turnTargetTime;
    private float turnTimer;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;

    [SerializeField] private Game game;

    void Start()
    {
      
    }

    void Update()
    {
        if (status == CardStatus.rotating_to_front || status == CardStatus.rotating_to_back)
        {
            turnTimer += Time.deltaTime;
            float percentage = turnTimer / turnTargetTime;

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, percentage);
            if (percentage >= 1)
            {
                if (status == CardStatus.rotating_to_back)
                {
                    status = CardStatus.show_back;
                }
                else if (status == CardStatus.rotating_to_front)
                {
                    status = CardStatus.show_front;
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if(game.AllowedToSelectCard(this) == true)
        {
            if (status == CardStatus.show_back)
            {
                game.SelectCard(gameObject);
                TurnToFront();
            }
            else if (status == CardStatus.show_front)
            {
                TurnToBack();
            }
        }
    }

    public void TurnToFront()
    {
        status = CardStatus.rotating_to_front;
        turnTimer = 0;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 180, 0);
    }

    public void TurnToBack() {
        status = CardStatus.rotating_to_back;
        turnTimer = 0;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 0, 0);
    }

    private void Awake()
    {
        status = CardStatus.show_back;
        GetFrontAndBackSpriteRenderers();

        game = FindObjectOfType<Game>();
    }

    private void GetFrontAndBackSpriteRenderers()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "Front")
            {
                frontRenderer = t.GetComponent<SpriteRenderer>();
            }
            else if (t.name == "Back")
            {
                backRenderer = t.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void SetFront(Sprite sprite)
    {
        if(frontRenderer != null)
        {
            frontRenderer.sprite = sprite;
        }
    }

    public void SetBack(Sprite sprite)
    {
        if(backRenderer != null)
        {
            backRenderer.sprite = sprite;
        }
    }

    public Vector2 GetFrontSize()
    {
        if(frontRenderer == null)
        {
            Debug.LogError("Er is geen frontRenderer gevonden");
        }
        return frontRenderer.bounds.size;
    }

    public Vector2 GetBackSize()
    {
        if (backRenderer == null)
        {
            Debug.LogError("Er is geen backRenderer gevonden");
        }
        return backRenderer.bounds.size;
    }
}
