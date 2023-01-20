using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BreathImageController : MonoBehaviour
{
	public GameObject BreathImage;
	
	private const float period = 4.0f;
	private float offsetX = 0.0f;

	void Update()
	{
		var image = BreathImage.GetComponent<Image>();
		float delta_x = 0.25f * Time.deltaTime / period;
		offsetX += delta_x;
		if (offsetX > 1.0f)
        {
			offsetX -= 1.0f;
        }
		image.material.mainTextureOffset = new Vector2(offsetX, 0);
	}

	void OnDestroy()
    {
		var image = BreathImage.GetComponent<Image>();
		image.material.mainTextureOffset = new Vector2(0, 0);
	}
}