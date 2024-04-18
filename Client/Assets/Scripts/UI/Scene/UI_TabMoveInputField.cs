using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_TabMoveInputField : MonoBehaviour
{
    [SerializeField] public TMP_InputField inputField1;
    [SerializeField] public TMP_InputField inputField2;

    public int InputSelected;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
        {
            InputSelected--;
            if (InputSelected < 0)
                InputSelected = 1;
            SelectInputField();
        }
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            InputSelected++;
            if(InputSelected > 1)
                InputSelected = 0;
            SelectInputField();
        }

        void SelectInputField()
        {
            if(InputSelected == 0)
            {
                inputField1.Select();
            }
            else
            {
                inputField2.Select();
            }
        }
    }
}
