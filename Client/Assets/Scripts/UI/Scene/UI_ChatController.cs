using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatController : UI_Base
{
    [SerializeField] private GameObject textChatPrefab; //대화를 출력하는 Text UI 프리팹
    [SerializeField] private GameObject scrollView; //대화를 출력하는 Text UI 프리팹
    [SerializeField] private Transform parentContent;  //대화가 출력되는 Scroll View의 Content
    [SerializeField] private TMP_InputField inputField; //대화 입력창
    [SerializeField] private Scrollbar scrollbar; //대화 스크롤바

    private string ID = "Player"; //플레이어의 ID(임시)

    private void Update()
    {
        //대화 입력창이 포커스 되어있지 않을 때 Enter키를 누르면
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
            // 대화 입력창을 활성화 시킨다.
            inputField.ActivateInputField();
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
        if (inputField.text.Equals("")) return;

        ////대화를 출력하는 Text UI 프리팹을 생성한다.
        //GameObject clone = Instantiate(textChatPrefab, parentContent);
        ////대화를 출력하는 Text UI 프리팹의 Text UI 컴포넌트를 가져온다.
        //clone.GetComponent<TextMeshProUGUI>().text = $"{ID} : {inputField.text}";

        //서버로 대화를 전송한다.
        C_Chat chat = new C_Chat();
        chat.Chat = inputField.text;
        Managers.Network.Send(chat);

        //대화 입력창을 초기화 시킨다.
        inputField.text = "";
    }

    public void ReceiveChat(S_Chat chat)
    {
        bool isBottom = scrollbar.value < 0.00001f; //스크롤바가 제일 아래로 내려와있는지 확인한다.
        Debug.Log(scrollbar.value);
        //대화를 출력하는 Text UI 프리팹을 생성한다.
        GameObject clone = Instantiate(textChatPrefab, parentContent);
        //대화를 출력하는 Text UI 프리팹의 Text UI 컴포넌트를 가져온다.
        clone.GetComponent<TextMeshProUGUI>().text = $"{chat.Name}: {chat.Chat}";
        
        if (isBottom)
            Invoke("ScrollDelay", 0.5f);
    }

    private void ScrollDelay() => scrollbar.value = 0; //제일 아래로 내린다.

    public override void Init()
    {
        //scrollView.SetActive(false);
        //ID = Managers.Object.MyPlayer.name; //하고 싶지만 초기화가 안되어 있어 null Exception이 발생한다.
        UpdateChat();
        Invoke("ScrollDelay", 0.5f);
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
