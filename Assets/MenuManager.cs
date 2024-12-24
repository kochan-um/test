using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject[] menuPanels; // メニューパネルのリスト
    private int currentMenuIndex = -1; // 現在のメニュー


    public void OpenMenu(int index)
    {
        // 現在のメニューを閉じる
        if (currentMenuIndex >= 0) 
            menuPanels[currentMenuIndex].SetActive(false);

        // 新しいメニューを開く
        menuPanels[index].SetActive(true);
        currentMenuIndex = index;
    }

    public void CloseMenu()
    {
        if (currentMenuIndex >= 0) 
        {
            menuPanels[currentMenuIndex].SetActive(false);
            currentMenuIndex = -1;
        }
    }
}
