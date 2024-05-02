using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Heal,
    Bullet,
    ActiveItem,
    
}

public enum ItemEffect
{
    Small,
    Midlle,
    Big,
    Light,
    Key,
    Quest
}

public enum QuestItem
{
    Defalt,
    Book,
    Phone
}

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public ItemEffect itemEffect;
    public QuestItem questItem;

    int effectValue;
    public int Effect() // �� ������ �� ȿ��
    {
        switch (itemType)
        {
            case ItemType.Heal: // ȸ�� ����
                switch (itemEffect)
                {
                    case ItemEffect.Small:
                        effectValue = 20;
                        break;
                    case ItemEffect.Midlle:
                        effectValue = 50;
                        break;
                    case ItemEffect.Big:
                        effectValue = 80;
                        break;
                }
                break;

            case ItemType.Bullet: // ź�� ����
                switch (itemEffect)
                {
                    case ItemEffect.Small:
                        effectValue = 20;
                        break;
                    case ItemEffect.Midlle:
                        effectValue = 50;
                        break;
                    case ItemEffect.Big:
                        effectValue = 80;
                        break;
                }
                break;
            case ItemType.ActiveItem:
                break;
        }
        return effectValue;
    }
   
}
