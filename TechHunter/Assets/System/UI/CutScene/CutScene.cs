using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutScene : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI StageName;
    [SerializeField] Image CharacterImage;
    public Animator animator;
    public void ChangeImage(CutSceneData data) 
    {

        Name.text = data.CharaName;
        StageName.text = data.StageName;
        CharacterImage.sprite = data.CharaImage128;

    }
}
