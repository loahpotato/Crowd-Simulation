using UnityEngine;
using System.Collections;

/// <summary>
/// ������ӽ������ƶ�
/// </summary>
public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 10; // ��������ƶ��ٶ�    
    void Update()
    {
        // ����ס����Ҽ���ʱ��    
        if (Input.GetMouseButton(0))
        {
            // ��ȡ����x��y��ֵ�������ٶȺ�Time.deltaTime����Ϊ����������˶�������ƽ��    
            float h = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float v = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
            // ���õ�ǰ������ƶ���y�Ტ���ı�    
            // ��Ҫ������������������ƶ��������ǰ���������������ƶ������Լ���Spance.World  
            this.transform.Translate(-h, 0, -v, Space.World);
        }
    }
}