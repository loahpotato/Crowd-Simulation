using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{

    public float maxDistance;//最大距离
    public float minDistance;//最小距离
    public float scaleSpeed;//缩放速度
    public float mouseSpeed;//缩放速度
    private Touch oldTouch1;  //上次触摸点1(手指1)  
    private Touch oldTouch2;  //上次触摸点2(手指2)  

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
    /// 手势屏幕缩放
    /// </summary>
    private void ZoomCamera()
    {
        //至少得要2个触摸点 
        if (Input.touchCount < 2)
        {
            return;
        }

        //多点触摸, 放大缩小  
        Touch newTouch1 = Input.GetTouch(0);
        Touch newTouch2 = Input.GetTouch(1);

        //第2点刚开始接触屏幕, 只记录，不做处理  
        if (newTouch2.phase == TouchPhase.Began)
        {
            oldTouch2 = newTouch2;
            oldTouch1 = newTouch1;
            return;
        }

        //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型  
        float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
        float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

        //两个距离之差，为正表示放大手势， 为负表示缩小手势  
        float offset = newDistance - oldDistance;

        Vector3 originalPos = transform.position;
        Quaternion originalRotation = transform.rotation;
        transform.position += offset * transform.forward * scaleSpeed * Time.deltaTime;

        //临时判断值
        float cameraY = transform.position.y;
        if (cameraY < minDistance || cameraY > maxDistance)
        {
            transform.position = originalPos;
            transform.rotation = originalRotation;
        }

        //重新计算
        cameraY = transform.position.y;

        //记住最新的触摸点，下次使用  
        oldTouch1 = newTouch1;
        oldTouch2 = newTouch2;
    }

    /// <summary>
    /// 鼠标滚轮缩放
    /// </summary>
    private void MouseZoomCamera()
    {
        //获取滚轮的值
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseScroll) > 0)
        {
            //print("hua");
            Vector3 originalPos = transform.position;
            Quaternion originalRotation = transform.rotation;

            transform.position += mouseScroll * transform.forward * mouseSpeed * Time.deltaTime;
            //临时判断值
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