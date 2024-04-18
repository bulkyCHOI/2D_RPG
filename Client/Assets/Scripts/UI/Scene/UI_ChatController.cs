using TMPro;
using UnityEngine;

public class UI_ChatController : UI_Base
{
    [SerializeField] private GameObject textChatPrefab; //대화를 출력하는 Text UI 프리팹
    [SerializeField] private GameObject scrollView; //대화를 출력하는 Text UI 프리팹
    [SerializeField] private Transform parentContent;  //대화가 출력되는 Scroll View의 Content
    [SerializeField] private TMP_InputField inputField; //대화 입력창

    private string ID = "Player"; //플레이어의 ID(임시)

    private void Update()
    {
        //대화 입력창이 포커스 되어있지 않을 때 Enter키를 누르면
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
        {
            
            // 대화 입력창을 활성화 시킨다.
            inputField.ActivateInputField();
            
            // 대화 입력창을 선택한다.
            if (inputField.text.Length > 0)
            {
                //Managers.Network.Send(new Define.ChatMessage()
                //{
                //    chat = _inputField.text
                //});

                inputField.text = "";
            }
        }
        //대화 입력창이 포커스 되어있지 않을 때 Enter키를 누르면
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
        {
            // 대화 입력창을 활성화 시킨다.
            inputField.ActivateInputField();
            // 대화 입력창을 선택한다.
            if (inputField.text.Length > 0)
            {
                //Managers.Network.Send(new Define.ChatMessage()
                //{
                //    chat = _inputField.text
                //});

                inputField.text = "";
            }
        }
    }

    public void OnEndEditEventMethod()
    {
        //대화 입력창에서 Enter키를 누르면
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();
        }
    }

    public void UpdateChat()
    {
        //대화 입력창에 아무것도 입력되어있지 않으면
        if (inputField.text.Length == 0)
            return;

        //대화를 출력하는 Text UI 프리팹을 생성한다.
        GameObject clone = Instantiate(textChatPrefab, parentContent);
        //대화를 출력하는 Text UI 프리팹의 Text UI 컴포넌트를 가져온다.
        clone.GetComponent<TextMeshProUGUI>().text = $"{ID} : {inputField.text}";

        //대화 입력창을 초기화 시킨다.
        inputField.text = "";
    }

    public override void Init()
    {
        scrollView.SetActive(false);
        UpdateChat();
    }

    public void OnSelected()
    {
        scrollView.SetActive(true);
    }

    public void OnDeselected()
    {
        scrollView.SetActive(false);
    }
}
