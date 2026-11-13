using UnityEngine;

public class Table : MonoBehaviour, IInteractable
{
    public int tableNum;
    private Customer customer;

    [Header("Open-table indicator")]
    [Tooltip("Optional. Leave empty to auto-create a soft yellow glow behind the table.")]
    [SerializeField] private SpriteRenderer openIndicator;
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.2f, 0.75f);
    [SerializeField] private float glowSize = 1.4f;      // relative to the table's sprite size
    [SerializeField] private float pulseSpeed = 3f;
    [Tooltip("If on, the glow only shows while you are escorting a customer.")]
    [SerializeField] private bool onlyWhileEscorting = true;

    private FollowTed player;
    private static Sprite glowSprite;

    private void Start()
    {
        player = FindFirstObjectByType<FollowTed>();
        if (openIndicator == null)
        {
            openIndicator = CreateGlow();
        }
    }

    private void Update()
    {
        if (openIndicator == null) return;

        bool isOpen = !GameManager.Instance.customerManager.CheckTableFull(tableNum);
        bool escorting = player != null && player.GetIsGuiding();
        bool show = isOpen && (escorting || !onlyWhileEscorting);

        openIndicator.enabled = show;
        if (show)
        {
            // gentle pulse (freezes with the game when paused)
            float t = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            Color c = glowColor;
            c.a *= Mathf.Lerp(0.5f, 1f, t);
            openIndicator.color = c;
        }
    }

    private SpriteRenderer CreateGlow()
    {
        SpriteRenderer tableSprite = GetComponentInChildren<SpriteRenderer>();

        GameObject glow = new GameObject("OpenTableGlow");
        glow.transform.SetParent(transform, false);

        SpriteRenderer sr = glow.AddComponent<SpriteRenderer>();
        sr.sprite = GetGlowSprite();
        sr.enabled = false;

        if (tableSprite != null)
        {
            // sit just behind the table, centred on it, scaled to its size
            sr.sortingLayerID = tableSprite.sortingLayerID;
            sr.sortingOrder = tableSprite.sortingOrder - 1;
            Bounds b = tableSprite.bounds;
            glow.transform.position = b.center;
            Vector3 parentScale = transform.lossyScale;
            float w = b.size.x * glowSize / Mathf.Max(parentScale.x, 0.0001f);
            float h = b.size.y * glowSize / Mathf.Max(parentScale.y, 0.0001f);
            glow.transform.localScale = new Vector3(w, h, 1f);
        }
        return sr;
    }

    // A 1x1-unit soft radial gradient, generated once and shared by all tables
    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null) return glowSprite;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        float half = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                float a = Mathf.Clamp01(1f - d);
                pixels[y * size + x] = new Color(1f, 1f, 1f, a * a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        glowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return glowSprite;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnInteract(GameObject player)
    {
                //check tablefull
        if (GameManager.Instance.customerManager.CheckTableFull(tableNum))
        {
            customer.OnInteract(player);
            return; //can make this trigger customer stuff??
        }

        //check if following
        FollowTed followInfo = player.GetComponent<FollowTed>();
        if (followInfo.GetIsGuiding())
        {
            // set customer to this table
            Customer customer = followInfo.GetCurrentFollow().GetComponent<Customer>();
            followInfo.StopFollow();
            SetCustomerToTable(customer);
        }
    }

    private void SetCustomerToTable(Customer customer)
    {
        GameManager.Instance.customerManager.AddCustomer(customer.GetCustomerType(), tableNum);
        customer.SetTableNum(tableNum);

        customer.gameObject.transform.position = this.gameObject.transform.position + new Vector3(0, 1f, 0);
        GameManager.Instance.customerManager.AddCustomer(customer.GetCustomerType(), tableNum);

        CustomerSpawner.Instance.SetIsOccupied(false);
        this.customer = customer;
    }
}
