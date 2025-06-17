using UnityEngine;
using UnityEngine.Events;
public class Lever : MonoBehaviour
{
    public Sprite offSprite;
    public Sprite onSprite;

    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;

    [Header("Events")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //spriteRenderer.sprite = offSprite;
        //GetComponent<SpriteRenderer>().color = Color.red;
    }

    public void Activate()
    {
        ToggleLever();
    }

    private void ToggleLever()
    {
        isActivated = !isActivated;
        spriteRenderer.sprite = isActivated ? onSprite : offSprite;
        if (isActivated)
        {
            Debug.Log("Palanca activada");
            //GetComponent<SpriteRenderer>().color = Color.green;
            OnActivated.Invoke();
        }
        else
        {
            Debug.Log("Palanca desactivada");
            //GetComponent<SpriteRenderer>().color = Color.red;
            OnDeactivated.Invoke();
        }
        
    }
}
