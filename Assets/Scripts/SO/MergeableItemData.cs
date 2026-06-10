using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New MergeableItem", menuName = "Merge Game/Mergeable Item")]
public class MergeableItemData : ScriptableObject
{
   [Header("Save ve Yükleme İçin Benzersiz ID")]
    public string ItemID; // Her eşya için benzersiz ID

    [Header ("Temel Bilgiler")]
    public string ItemName;
    public Sprite MarketIcon;

    
    public int Level;

   
    public GameObject Prefab;

   
    public MergeableItemData NextLevelItem;

    [Header("ekonomi ve ilerleme(GDD V.0.4)")]
    public int SellPrice; //Gdd:Zaman kredisi değeri (satış)
    public int BuyPrice; //marketten alış
    
    public int FacilityPoint; //üs genişletme puanı 

}
