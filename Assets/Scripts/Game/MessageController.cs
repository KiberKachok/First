using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    public float lifetime = 3f;
    public float fadeTime = 0.3f;
    public float startHeight;
    private LayoutElement _layoutElement;
    private TextMeshProUGUI _text;
    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        _image = transform.GetChild(0).GetComponent<Image>();
        _text = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        _layoutElement = GetComponent<LayoutElement>();
        startHeight = _layoutElement.preferredHeight;
        StartCoroutine(Life());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Life()
    {
        yield return new WaitForSeconds(lifetime);
        float t = fadeTime;

        while (t > 0)
        {
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _image.color.a * t / fadeTime);
            _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _text.color.a * t / fadeTime);
            _layoutElement.preferredHeight = startHeight * t / fadeTime;
            t -= Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
