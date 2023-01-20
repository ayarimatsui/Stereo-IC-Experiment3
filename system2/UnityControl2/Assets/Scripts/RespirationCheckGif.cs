using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespirationCheckGif : MonoBehaviour
{
    public Sprite[] sprites;
    public float speed;
    private Image image;
    private float current;

    void Start()
    {
        image = GetComponent<Image>();
        image.sprite = sprites[0];
    }

    public void makeCheck()
    {
        current = 0f;
        IEnumerator coroutine = updateImg();
        StartCoroutine(coroutine);
    }

    public void cancelCheck()
    {
        image.sprite = sprites[0];
    }

    public void destroyCheck()
    {
        Destroy(gameObject);
    }

    private IEnumerator updateImg()
    {
        int index = 0;
        while (index < sprites.Length - 1)
        {
            current += Time.deltaTime * speed;
            index = (int)(current) % sprites.Length;
            if (index > sprites.Length - 1) index = sprites.Length - 1;
            image.sprite = sprites[index];
            yield return new WaitForSeconds(0.01f);
        }
    }
}
