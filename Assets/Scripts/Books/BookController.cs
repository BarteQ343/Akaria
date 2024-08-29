using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookController : MonoBehaviour
{
    public GameObject darkPanel;
    public GameObject bookPanel;
    public TMP_Text bookText;
    public BookData bookData;  // Reference to the ScriptableObject
    public bool isReading = false;
    private bool isInRange = false;
    public Rigidbody2D player;

    private void Start()
    {
        // Ensure the panels are initially disabled
        darkPanel.SetActive(false);
        bookPanel.SetActive(false);
    }

    private void Update()
    {
        // For testing purposes, we will use the "E" key to interact with the book
        if (isInRange == true && Input.GetKeyDown(KeyCode.E))
        {
            if (isReading)
            {
                CloseBook();
                player.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
			}
            else
            {
                OpenBook(bookData.bookText);
				player.constraints = RigidbodyConstraints2D.FreezeAll;
			}
        }
    }

    private void OpenBook(string text)
    {
        isReading = true;
        darkPanel.SetActive(true);
        bookPanel.SetActive(true);
        bookText.text = text;
    }

    private void CloseBook()
    {
        isReading = false;
        darkPanel.SetActive(false);
        bookPanel.SetActive(false);
        bookText.text = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            // Show an indicator to the player that they can interact with the book
            Debug.Log("Press 'E' to read the book.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            // Hide the interaction indicator
            Debug.Log("Left the book's interaction range.");
        }
    }
}
