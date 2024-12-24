using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusPanel : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text hpText;
    public Text mpText;

    public void UpdateStatus(Character character)
    {
        nameText.text = character.name;
        levelText.text = "Lv " + character.level.ToString();
        hpText.text = "HP: " + character.hp.ToString();
        mpText.text = "MP: " + character.mp.ToString();
    }
}

[System.Serializable]
public class Character
{
    public string name;
    public int level;
    public int hp;
    public int mp;
}
