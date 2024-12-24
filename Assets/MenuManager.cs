using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject[] menuPanels; // ���j���[�p�l���̃��X�g
    private int currentMenuIndex = -1; // ���݂̃��j���[


    public void OpenMenu(int index)
    {
        // ���݂̃��j���[�����
        if (currentMenuIndex >= 0) 
            menuPanels[currentMenuIndex].SetActive(false);

        // �V�������j���[���J��
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
