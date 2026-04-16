using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the NPC's name above their head using a world-space canvas.
/// Attach to the NPC GameObject — it creates the label automatically.
/// </summary>
public class NPCNameDisplay : MonoBehaviour
{
    [SerializeField] private string _npcName = "Villager";
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Font _font;

    private Canvas _worldCanvas;
    private Text _nameText;

    void Start()
    {
        CreateNameLabel();
    }

    private void CreateNameLabel()
    {
        // create a child with a world-space canvas
        GameObject canvasObj = new GameObject("NameCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = _offset;

        _worldCanvas = canvasObj.AddComponent<Canvas>();
        _worldCanvas.renderMode = RenderMode.WorldSpace;
        _worldCanvas.sortingOrder = 10;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2f, 0.5f);
        canvasRect.localScale = Vector3.one * 0.01f;

        // create the text element
        GameObject textObj = new GameObject("NameText");
        textObj.transform.SetParent(canvasObj.transform, false);

        _nameText = textObj.AddComponent<Text>();
        _nameText.text = _npcName;
        _nameText.alignment = TextAnchor.MiddleCenter;
        _nameText.fontSize = 32;
        _nameText.color = Color.white;
        _nameText.horizontalOverflow = HorizontalWrapMode.Overflow;

        if (_font != null) _nameText.font = _font;
        else _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(200f, 50f);
        textRect.anchoredPosition = Vector2.zero;
    }

    void LateUpdate()
    {
        // billboard — always face camera, and counteract parent scale flip
        if (_worldCanvas != null && Camera.main != null)
        {
            _worldCanvas.transform.forward = Camera.main.transform.forward;

            // If parent is flipped (negative x scale), counter-flip the canvas
            Vector3 canvasScale = _worldCanvas.transform.localScale;
            float parentScaleX = transform.lossyScale.x;
            canvasScale.x = Mathf.Abs(canvasScale.x) * Mathf.Sign(parentScaleX);
            _worldCanvas.transform.localScale = canvasScale;
        }
    }

    public void SetName(string newName)
    {
        _npcName = newName;
        if (_nameText != null) _nameText.text = newName;
    }

    public string GetName() => _npcName;
}
