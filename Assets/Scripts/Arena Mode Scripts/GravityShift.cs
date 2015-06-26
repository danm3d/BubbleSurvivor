using UnityEngine;
using System.Collections;

public class GravityShift : MonoBehaviour
{

    public Sprite[] directions = new Sprite[4];
    private AreaEffector2D areaEffect;//reference to the area effector
    public SpriteRenderer directionSprite;//sprite which will indicate the direction

    // Use this for initialization
    void Start()
    {
        areaEffect = GetComponent<AreaEffector2D>();
        StartCoroutine(GravShift());
    }
	
    IEnumerator GravShift()
    {
        int direction = 0;
        for (;;)
        {
            direction = Random.Range(0, 4);
            areaEffect.forceAngle = 90f * direction;
            //directionSprite.sprite = directions [direction];
            directionSprite.transform.rotation = Quaternion.Euler(0, 0, 90f * direction);
            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

}
