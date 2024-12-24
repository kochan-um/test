using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour
{
    public Item[] items; // �����A�C�e��
    public GameObject itemButtonPrefab; // �{�^��Prefab
    public Transform contentPanel; // �{�^����z�u����p�l��

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
        Debug.Log($"�A�C�e��: {item.itemName}, ����: {item.description}");
        // �ڍו\�����X�V����R�[�h�������ɒǉ�
    }
}
