using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.MLAPISample
{
    // カメラの方を向くコンポーネント、名前表示用
    public class NameLookAtCamera : MonoBehaviour
    {
        Camera mainCamera;
        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            var pos = transform.position - mainCamera.transform.position;
            this.transform.LookAt(pos);
            this.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }
}