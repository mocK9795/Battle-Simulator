using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorDisplay : MonoBehaviour
{
    public GameObject warningMenu;
    public Image background;
    public TMP_Text text;

    public float showTime;
    public float fadeIn;
    public float fadeOut;
    bool inUse = false;

    public void ShowError(string message)
    {
        if (inUse) return;
        StartCoroutine(ShowErrorPrivate(message));
    }

    IEnumerator ShowErrorPrivate(string message)
    {
        inUse = true;
        warningMenu.SetActive(true);
        text.text = message;
        Color bgColor = background.color;
        float step = 1f / 64f;

        float timer = 0;
        while (timer < fadeIn)
        { bgColor.a = timer / fadeIn; background.color = bgColor; 
            yield return new WaitForSeconds(step); timer += step; }

        yield return new WaitForSeconds(showTime);

		timer = 0;
		while (timer < fadeOut)
		{
			bgColor.a = timer / fadeOut; background.color = bgColor;
			yield return new WaitForSeconds(step); timer += step;
		}

		warningMenu.SetActive(false);
        inUse = false;
    }
}
