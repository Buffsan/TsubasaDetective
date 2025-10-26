using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/RelicData")]
public class RelicData : ScriptableObject
{
    public string RelicName;
    [Space]
    public GameObject RelicDataObject;
    [Space]
    public Sprite RelicImage;
    [TextArea]
    public string Explanation;
    public enum Stage
    {

        Ferust,
        Diana,
        Garewo,
        Kardia,
        Pastal,
        Teruseto,
        Shuela

    }
    public Stage stage = Stage.Ferust;
}
