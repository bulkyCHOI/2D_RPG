using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EventDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    public CanvasGroup canvasGroup;

    //�ʱ�ȭ
    private void Awake()
    {
        Debug.Log("Awake");
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag");
        parentAfterDrag = transform.parent;
        //Debug.Log(parentAfterDrag.position);
        transform.SetParent(transform.parent.parent);
        transform.SetAsLastSibling();   //�巡���ϴ� �������� �� ���� �ø���.
        //gameObject.GetComponent<Image>().raycastTarget = false; //raycastTarget�� false�� �ؾ� OnDrop�Ҷ� �巡������ �������� �ƴ϶� �ؿ� ������ �������� detect�ȴ�. >> �ȵ�
        canvasGroup.blocksRaycasts = false; //blocksRaycasts�� false�� �ؾ� OnDrop�Ҷ� �巡������ �������� �ƴ϶� �ؿ� ������ �������� detect�ȴ�.

    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        //Debug.Log(transform.GetChild(0).gameObject.name);
        //Debug.Log(transform.GetChild(0).gameObject.activeSelf);

        //���� ���ӿ�����Ʈ�� ItemIcon�� Ȱ��ȭ �� ��츸 �巡�װ� �ǵ��� �Ѵ�.
        if (transform.GetChild(0).gameObject.activeSelf == true)
            transform.position = Input.mousePosition;

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("=====OnEndDrag=====");

        //drag�� �������� ������ �θ�� �������´�.
        //gameObject.GetComponent<Image>().raycastTarget = true;
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(parentAfterDrag); //������ �θ��̱� ������ �̵��� ������ ����.
        //���� ��ġ�� ���ư� �ʿ䰡 ���ٸ�
        //transform.position = parentAfterDrag.position;

        //Debug.Log($"transform: {transform.parent.gameObject.name}");

        //transform.SetParent(parentAfterDrag);
        //transform.SetAsLastSibling();
        //if(dropped.GetChild(0).gameObject.activeSelf == false)
        //    transform.SetParent(parentAfterDrag);
        //else
        //{
        //    transform.SetParent(dropped.parent);
        //    dropped.SetParent(parentAfterDrag);
        //}

    }
}
