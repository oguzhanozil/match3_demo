using UnityEngine;

[CreateAssetMenu(menuName = "Match3/CandyType")]
public class CandyType : ScriptableObject
{
    public string typeName;
    public Sprite sprite;
    public int id; // eşleştirme için
    public int scoreValue = 10;
}