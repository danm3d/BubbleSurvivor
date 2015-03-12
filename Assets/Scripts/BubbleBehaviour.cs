using UnityEngine;
using System.Collections;

public class BubbleBehaviour : MonoBehaviour
{

    public Material redMat, greenMat, blueMat;
    public enum BubbleType
    {
        Red = 0,
        Green = 1,
        Blue = 2
    }
    public BubbleType bubbleType;
    public BubbleManager bubbleManager;
    private CircleCollider2D circleCollider;
    private CircleCollider2D MyCollider
    {
        get
        {
            if (circleCollider == null)
                circleCollider = GetComponent<CircleCollider2D>();
            return circleCollider;
        }
    }
    private Renderer myRend;//reference to the renderer on this object
    private bool isWrappingX, isWrappingY;

    void Awake()
    {
        myRend = GetComponent<Renderer>();
        Spawn();
    }

    public void Spawn()
    {
        StartCoroutine(SpawnScale());
    }

    public void Respawn()
    {
        StartCoroutine(RespawnScale());
    }

    IEnumerator SpawnScale()
    {
        MyCollider.enabled = false;
        transform.localScale = Vector3.zero;
        bubbleType = (BubbleType)Random.Range(0, 3);
        if (bubbleType == BubbleType.Red)
            GetComponent<Renderer>().material = redMat;
        else if (bubbleType == BubbleType.Green)
            GetComponent<Renderer>().material = greenMat;
        else
            GetComponent<Renderer>().material = blueMat;

        float scale = Random.Range(0.5f, 1.5f);
        Vector3 oldScale = transform.localScale;
        Vector3 newScale = new Vector3(scale, scale, scale);
        float time = 0.0f;
        while (time < 1.0f)
        {
            transform.localScale = Vector3.Lerp(oldScale, newScale, time);
            yield return new WaitForSeconds(0.02f);
            time += Time.deltaTime * 0.5f;
        }
        MyCollider.radius = scale / 2f;
        MyCollider.enabled = true;
        GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
    }

    IEnumerator RespawnScale()
    {
        MyCollider.enabled = false;
        transform.localScale = Vector3.zero;
        bubbleType = (BubbleType)Random.Range(0, 3);
        if (bubbleType == BubbleType.Red)
            GetComponent<Renderer>().material = redMat;
        else if (bubbleType == BubbleType.Green)
            GetComponent<Renderer>().material = greenMat;
        else
            GetComponent<Renderer>().material = blueMat;
        
        float scale = Random.Range(0.5f, 1.5f);
        Vector3 oldScale = transform.localScale;
        Vector3 newScale = new Vector3(scale, scale, scale);
        float time = 0.0f;
        while (time < 1.0f)
        {
            transform.localScale = Vector3.Lerp(oldScale, newScale, time);
            yield return new WaitForSeconds(0.02f);
            time += Time.deltaTime * 2f;
        }
        MyCollider.radius = scale / 2f;
        MyCollider.enabled = true;
        GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-10, 10), Random.Range(-10, 10));
    }

    /// Called by the player when he absorbs a bubble.
    public void Absorb()
    {
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        StartCoroutine(AbsorbScale());
    }

    IEnumerator AbsorbScale()
    {
        MyCollider.enabled = false;
        float time = 0.0f;
        while (time < 1f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, time);
            yield return new WaitForSeconds(0.02f);
            time += Time.deltaTime * 2f;
        }
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        bubbleManager.SaveBubble(gameObject);
    }

    void Update()
    {
        ScreenWrap();
    }

    private void ScreenWrap()
    {
        if (!myRend.isVisible)
        {
            if (isWrappingX && isWrappingY)
            {
                return;
            }
            Vector3 newPos = transform.position;
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            if (!isWrappingX && (viewportPos.x > 1 || viewportPos.x < 0))
            {
                newPos.x = -newPos.x;                
                isWrappingX = true;
            }
            
            if (!isWrappingY && (viewportPos.y > 1 || viewportPos.y < 0))
            {
                newPos.y = -newPos.y;                
                isWrappingY = true;
            }
            transform.position = newPos;
        } else
        {
            isWrappingX = false;
            isWrappingY = false;
            return;
        }
    }

}
