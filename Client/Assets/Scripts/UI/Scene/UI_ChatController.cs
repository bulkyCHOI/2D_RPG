using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatController : UI_Base
{
    [SerializeField] private GameObject textChatPrefab; //��ȭ�� ����ϴ� Text UI ������
    [SerializeField] private GameObject scrollView; //��ȭ�� ����ϴ� Text UI ������
    [SerializeField] private Transform parentContent;  //��ȭ�� ��µǴ� Scroll View�� Content
    [SerializeField] private TMP_InputField inputField; //��ȭ �Է�â
    [SerializeField] private Scrollbar scrollbar; //��ȭ ��ũ�ѹ�

    private string ID = "Player"; //�÷��̾��� ID(�ӽ�)

    private void Update()
    {
        //��ȭ �Է�â�� ��Ŀ�� �Ǿ����� ���� �� EnterŰ�� ������
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
            // ��ȭ �Է�â�� Ȱ��ȭ ��Ų��.
            inputField.ActivateInputField();
    }

    public void OnEndEditEventMethod()
    {
        //��ȭ �Է�â���� EnterŰ�� ������
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();
        }
    }

    public void UpdateChat()
    {
        //��ȭ �Է�â�� �ƹ��͵� �ԷµǾ����� ������
        if (inputField.text.Equals("")) return;

        ////��ȭ�� ����ϴ� Text UI �������� �����Ѵ�.
        //GameObject clone = Instantiate(textChatPrefab, parentContent);
        ////��ȭ�� ����ϴ� Text UI �������� Text UI ������Ʈ�� �����´�.
        //clone.GetComponent<TextMeshProUGUI>().text = $"{ID} : {inputField.text}";

        //������ ��ȭ�� �����Ѵ�.
        C_Chat chat = new C_Chat();
        chat.Chat = inputField.text;
        Managers.Network.Send(chat);

        //��ȭ �Է�â�� �ʱ�ȭ ��Ų��.
        inputField.text = "";
    }

    public void ReceiveChat(S_Chat chat)
    {
        bool isBottom = scrollbar.value < 0.00001f; //��ũ�ѹٰ� ���� �Ʒ��� �������ִ��� Ȯ���Ѵ�.
        Debug.Log(scrollbar.value);
        //��ȭ�� ����ϴ� Text UI �������� �����Ѵ�.
        GameObject clone = Instantiate(textChatPrefab, parentContent);
        //��ȭ�� ����ϴ� Text UI �������� Text UI ������Ʈ�� �����´�.
        clone.GetComponent<TextMeshProUGUI>().text = $"{chat.Name}: {chat.Chat}";
        
        if (isBottom)
            Invoke("ScrollDelay", 0.5f);
    }

    private void ScrollDelay() => scrollbar.value = 0; //���� �Ʒ��� ������.

    public override void Init()
    {
        //scrollView.SetActive(false);
        //ID = Managers.Object.MyPlayer.name; //�ϰ� ������ �ʱ�ȭ�� �ȵǾ� �־� null Exception�� �߻��Ѵ�.
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
