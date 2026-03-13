using UnityEngine;
using System.Collections.Generic;

public class ReviewManager : MonoBehaviour
{
    public static ReviewManager Instance { get; private set; }
    public CustomerReview[] allReviews; // Stores all of the possible reviews
    public List<CustomerReview> currentReviews = new List<CustomerReview>(); // Stores the reviews for the game day
    public int dayNumber; // Variable that signifies what day in the game it is

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
        allReviews = Resources.LoadAll<CustomerReview>("Reviews"); // Loads in the reviews from the resources folder
    }

    /// <summary>
    /// This function gets called from the Customer Manager when a customer leaves their table.
    /// The function calls the AddReview function that will add it to current reviews.
    /// Right now, it randomly assigns the rating, but this will change once more information is introduced.
    /// </summary>
    public void HandleCustomerRemoved()
    {
        float starsGiven = Random.Range(1, 5 + 1);
        AddReview(starsGiven);
    }

    /// <summary>
    /// When the manager runs up, it will set the day number to zero to signify a new game.
    /// </summary>
    void Start()
    {
        dayNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// When it is time for a game day to be advanced, this function will be called.
    /// It also clears the current reviews and the reviews in the review ticker.
    /// </summary>
    void NextDay()
    {
        dayNumber += 1;
        currentReviews.Clear();
        ReviewTicker.Instance.reviews.Clear();
    }

    /// <summary>
    /// For the amount of starsGiven for the function, it will go through
    /// all of the reviews loaded and pick out all the reviews that have the
    /// same rating as provided. After this, it will randomly select a review
    /// from that list and then add that review to the current reviews and the ticker.
    /// </summary>
    /// <param name="starsGiven"></param>
    void AddReview(float starsGiven)
    {
        List<CustomerReview> potentialReviews = new List<CustomerReview>();
        foreach (CustomerReview review in allReviews)
        {
            if (Mathf.Approximately(review.starsGiven, starsGiven))
            {
                potentialReviews.Add(review);
            }
        }
        CustomerReview selectedReview = potentialReviews[Random.Range(0, potentialReviews.Count)];
        currentReviews.Add(selectedReview);
        ReviewTicker.Instance.HandleReviewAdded(selectedReview);
    }
}
