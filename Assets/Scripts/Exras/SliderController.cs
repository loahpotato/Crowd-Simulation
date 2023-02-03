using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Slider _slider;
    public GameObject manager;
    public Camera Pcamera;
    CameraMove m;
    void Start()
    {
        m = Pcamera.GetComponent<CameraMove>();
        _slider.onValueChanged.AddListener((v) => {
            text.text = v.ToString();
            if(manager.TryGetComponent<SpatialManager>(out SpatialManager c))
                SpatialManager.Instance.initalNumber = (int)v;
            if (manager.TryGetComponent<FlockManager>(out FlockManager f))
                FlockManager.Instance.initalNumber = (int)v;
        });
        
    }

    private void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject() == false){
            m.enabled = true;
        }
        else
        {
            m.enabled = false;
        }
        
    }
}
