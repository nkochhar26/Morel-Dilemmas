using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Source Link: https://www.youtube.com/watch?v=mptVj9-I0gQ 

public class ReviewItem : MonoBehaviour
{
    float width;
    float pixelsPerSecond;
    RectTransform rt;

    public float GetXPosition { get { return rt.anchoredPosition.x; } }
    public float GetWidth { get { return rt.rect.width; }  }

    bool initialized = false;

    public void Initialize(float tickerWidth, float ppS, CustomerReview review)
    {
        this.width = tickerWidth; // Width of the ticker
        this.pixelsPerSecond = ppS; // The speed at which the pixels move across the screen
        // The code below formats the text from the review for presentation on the ticker
        rt = GetComponent<RectTransform>();
        GetComponent<TextMeshProUGUI>().text = " " + review.starsGiven + " Stars: " + review.writtenReview + " ||";
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized || rt == null) return; // Doesn't call if there is no object
        rt.position += Vector3.left * pixelsPerSecond * Time.deltaTime; // Moves the reviews across the screen in the left direction
        // If the review has moved off screen, then it will be destroyed (to conserve space)
        if (GetXPosition <= 0 - width - GetWidth)
        {
            Destroy(gameObject);
        }
    }
}