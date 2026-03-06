using UnityEngine;
using System.Collections.Generic;

public class ReviewTicker : MonoBehaviour
{
    public static ReviewTicker Instance { get; private set; }

    public ReviewItem reviewPrefab;
    public float reviewDuration = 4.0f;
    public List<CustomerReview> reviews = null;

    float width;
    float pixelsPerSecond;
    int rightIndex = 0;

    ReviewItem currReview;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
        TryAssignReviewsFromManager();
    }

    /// <summary>
    /// This function initializes the variables by assigning reviews from the manager,
    /// getting the width of the ticker, the speed and automatically 
    /// adds a review to the ticker if there is one.
    /// </summary>
    void Start()
    {
        TryAssignReviewsFromManager();

        width = GetComponent<RectTransform>().rect.width;
        pixelsPerSecond = width / reviewDuration;
        if (reviews != null && reviews.Count > 0)
        {
            AddToTicker(reviews[0]);
        }
    }

    /// <summary>
    /// This function ensures that the ticker continues to loop on forever.
    /// What it does is that it looks at the current review, and if that review leaves
    /// the screen, then it will create a new one to take its place.
    /// </summary>
    void Update()
    {
        if (currReview != null && (currReview.GetWidth != 0) && (currReview.GetXPosition <= -currReview.GetWidth) && reviews != null && reviews.Count > 0)
        {
            rightIndex = (rightIndex + 1) % reviews.Count;
            AddToTicker(reviews[rightIndex]);
        }
    }

    /// <summary>
    /// This function attempts to make a connection between the ReviewManager and the ReviewTicker.
    /// If the ReviewManager is not null and the current reviews are not null, then it will assign
    /// the current reviews from the reviews. If not, it will just be an empty list.
    /// </summary>
    void TryAssignReviewsFromManager()
    {
        if (ReviewManager.Instance != null && ReviewManager.Instance.currentReviews != null)
        {
            reviews = ReviewManager.Instance.currentReviews;
        }
        else if (reviews == null)
        {
            reviews = new List<CustomerReview>();
        }
    }

    /// <summary>
    /// This creates a new object that contains the review and places it on the ticker.
    /// </summary>
    /// <param name="review">
    /// The review to be added from the ticker.
    /// </param>
    void AddToTicker(CustomerReview review)
    {
        currReview = Instantiate(reviewPrefab, transform);
        currReview.Initialize(width, pixelsPerSecond, review);
    }

    /// <summary>
    /// This function is called from the ReviewManager.
    /// If reviews is not equal to the one in ReviewManager, then it will add to the local reviews variables.
    /// If there is no currReview and there are actually reviews, then it will find that review
    /// and add it to the ticker.
    /// </summary>
    /// <param name="review"></param>
    public void HandleReviewAdded(CustomerReview review)
    {
        if (!ReferenceEquals(reviews, ReviewManager.Instance.currentReviews))
        {
            reviews.Add(review);
        }

        if (currReview == null && reviews.Count > 0)
        {
            rightIndex = reviews.Count - 1;
            AddToTicker(reviews[rightIndex]);
        }
    }
}
