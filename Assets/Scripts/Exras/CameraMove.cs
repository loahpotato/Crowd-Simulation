using UnityEngine;
using System.Collections;

/// <summary>
/// 摄像机视角自由移动
/// </summary>
public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 10; // 设置相机移动速度    
    void Update()
    {
        // 当按住鼠标右键的时候    
        if (Input.GetMouseButton(0))
        {
            // 获取鼠标的x和y的值，乘以速度和Time.deltaTime是因为这个可以是运动起来更平滑    
            float h = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float v = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
            // 设置当前摄像机移动，y轴并不改变    
            // 需要摄像机按照世界坐标移动，而不是按照它自身的坐标移动，所以加上Spance.World  
            this.transform.Translate(-h, 0, -v, Space.World);
        }
    }
}