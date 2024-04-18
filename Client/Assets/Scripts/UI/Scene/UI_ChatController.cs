using TMPro;
using UnityEngine;

public class UI_ChatController : UI_Base
{
    [SerializeField] private GameObject textChatPrefab; //��ȭ�� ����ϴ� Text UI ������
    [SerializeField] private GameObject scrollView; //��ȭ�� ����ϴ� Text UI ������
    [SerializeField] private Transform parentContent;  //��ȭ�� ��µǴ� Scroll View�� Content
    [SerializeField] private TMP_InputField inputField; //��ȭ �Է�â

    private string ID = "Player"; //�÷��̾��� ID(�ӽ�)

    private void Update()
    {
        //��ȭ �Է�â�� ��Ŀ�� �Ǿ����� ���� �� EnterŰ�� ������
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
        {
            
            // ��ȭ �Է�â�� Ȱ��ȭ ��Ų��.
            inputField.ActivateInputField();
            
            // ��ȭ �Է�â�� �����Ѵ�.
            if (inputField.text.Length > 0)
            {
                //Managers.Network.Send(new Define.ChatMessage()
                //{
                //    chat = _inputField.text
                //});

                inputField.text = "";
            }
        }
        //��ȭ �Է�â�� ��Ŀ�� �Ǿ����� ���� �� EnterŰ�� ������
        if (Input.GetKeyDown(KeyCode.Return) && inputField.isFocused == false)
        {
            // ��ȭ �Է�â�� Ȱ��ȭ ��Ų��.
            inputField.ActivateInputField();
            // ��ȭ �Է�â�� �����Ѵ�.
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
        //��ȭ �Է�â���� EnterŰ�� ������
        if (Input.GetKeyDown(KeyCode.Return))
        {
            UpdateChat();
        }
    }

    public void UpdateChat()
    {
        //��ȭ �Է�â�� �ƹ��͵� �ԷµǾ����� ������
        if (inputField.text.Length == 0)
            return;

        //��ȭ�� ����ϴ� Text UI �������� �����Ѵ�.
        GameObject clone = Instantiate(textChatPrefab, parentContent);
        //��ȭ�� ����ϴ� Text UI �������� Text UI ������Ʈ�� �����´�.
        clone.GetComponent<TextMeshProUGUI>().text = $"{ID} : {inputField.text}";

        //��ȭ �Է�â�� �ʱ�ȭ ��Ų��.
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
