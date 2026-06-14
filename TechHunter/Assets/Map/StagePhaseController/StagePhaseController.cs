
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StagePhaseController : MonoBehaviour
{

    [SerializeField] Color NoChoiceColor;
    [SerializeField] Color ChoiceColor;
    [SerializeField] Color FrameColor;

    [SerializeField] Animator animator;
    [SerializeField] TextMeshProUGUI PhaseText;
    [SerializeField] string DefaltText;

    [System.Serializable]
    public struct StagePhaseSpots 
    {
        public GameObject Spot;
        public GameObject FrameSpot;
    }
    public List<StagePhaseSpots> stagePhase = new List<StagePhaseSpots>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeUI(int Phase) 
    {
        int CulcaratePhase = Phase - 1;
        PhaseText.text = DefaltText + (Phase).ToString() + "/3";
        animator.Play("èoåª");
        for (int i = 0; i < stagePhase.Count; i++) 
        {
            if (i == CulcaratePhase)
            {
                Image image = stagePhase[i].Spot.GetComponent<Image>();
                Image frameimage = stagePhase[i].FrameSpot.GetComponent<Image>();
                image.color = Color.red;
                frameimage.color = Color.white;
                Debug.Log("A");
            }
            else 
            {
                Image image = stagePhase[i].Spot.GetComponent<Image>();
                Image frameimage = stagePhase[i].FrameSpot.GetComponent<Image>();
                image.color = NoChoiceColor;
                frameimage.color = Color.white;
            }
        }
    }
}
