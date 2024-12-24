using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    public Item[] items; // 所持アイテム
    public GameObject itemButtonPrefab; // ボタンPrefab
    public Transform contentPanel; // ボタンを配置するパネル

    void Start()
    {
        PopulateInventory();
    }

    void PopulateInventory()
    {
        foreach (Item item in items)
        {
            GameObject button = Instantiate(itemButtonPrefab, contentPanel);
            button.GetComponentInChildren<Text>().text = item.itemName;
            button.GetComponent<Button>().onClick.AddListener(() => ShowItemDetails(item));
        }
    }

    void ShowItemDetails(Item item)
    {
        Debug.Log($"アイテム: {item.itemName}, 説明: {item.description}");
        // 詳細表示を更新するコードをここに追加
    }
}
