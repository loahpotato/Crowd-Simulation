using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{

    public float maxDistance;//������
    public float minDistance;//��С����
    public float scaleSpeed;//�����ٶ�
    public float mouseSpeed;//�����ٶ�
    private Touch oldTouch1;  //�ϴδ�����1(��ָ1)  
    private Touch oldTouch2;  //�ϴδ�����2(��ָ2)  

    // Use this for initialization
    void Start()
    {
        //FairyGUI.GObject a;
        //a.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        ZoomCamera();
        MouseZoomCamera();
    }

    /// <summary>
    /// ������Ļ����
    /// </summary>
    private void ZoomCamera()
    {
        //���ٵ�Ҫ2�������� 
        if (Input.touchCount < 2)
        {
            return;
        }

        //��㴥��, �Ŵ���С  
        Touch newTouch1 = Input.GetTouch(0);
        Touch newTouch2 = Input.GetTouch(1);

        //��2��տ�ʼ�Ӵ���Ļ, ֻ��¼����������  
        if (newTouch2.phase == TouchPhase.Began)
        {
            oldTouch2 = newTouch2;
            oldTouch1 = newTouch1;
            return;
        }

        //�����ϵ����������µ��������룬���Ҫ�Ŵ�ģ�ͣ���СҪ����ģ��  
        float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
        float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

        //��������֮�Ϊ����ʾ�Ŵ����ƣ� Ϊ����ʾ��С����  
        float offset = newDistance - oldDistance;

        Vector3 originalPos = transform.position;
        Quaternion originalRotation = transform.rotation;
        transform.position += offset * transform.forward * scaleSpeed * Time.deltaTime;

        //��ʱ�ж�ֵ
        float cameraY = transform.position.y;
        if (cameraY < minDistance || cameraY > maxDistance)
        {
            transform.position = originalPos;
            transform.rotation = originalRotation;
        }

        //���¼���
        cameraY = transform.position.y;

        //��ס���µĴ����㣬�´�ʹ��  
        oldTouch1 = newTouch1;
        oldTouch2 = newTouch2;
    }

    /// <summary>
    /// ����������
    /// </summary>
    private void MouseZoomCamera()
    {
        //��ȡ���ֵ�ֵ
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseScroll) > 0)
        {
            //print("hua");
            Vector3 originalPos = transform.position;
            Quaternion originalRotation = transform.rotation;

            transform.position += mouseScroll * transform.forward * mouseSpeed * Time.deltaTime;
            //��ʱ�ж�ֵ
            float cameraY = transform.position.y;
            if (cameraY < minDistance || cameraY > maxDistance)
            {
                transform.position = originalPos;
                transform.rotation = originalRotation;
            }

            //cameraY = transform.position.y;
            //print(distance.magnitude);
        }
    }
}