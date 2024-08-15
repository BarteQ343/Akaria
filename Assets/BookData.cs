using UnityEngine;

[CreateAssetMenu(fileName = "New Book", menuName = "Book")]
public class BookData : ScriptableObject
{
    [TextArea(10, 50)]
    public string bookText;
}
