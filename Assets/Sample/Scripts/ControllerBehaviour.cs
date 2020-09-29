
using System;
using UnityEngine;
using UnityEngine.UI;
using UTJ.MLAPISample;

namespace UTJ.MLAPISample
{
    [NetworkUtility.RequireAtHeadless]
    public class ControllerBehaviour : MonoBehaviour
    {
        private class ButtonListner
        {
            private int index;
            private System.Action<int> action;
            public ButtonListner(System.Action<int> act, int idx)
            {
                this.action = act;
                this.index = idx;
            }
            public void Exec()
            {
                this.action(this.index);
            }
        }

        private static ControllerBehaviour instance;

        private KeyCode[] keys;
        private bool[] buttonClickedBuffer;
        private Vector2 touchedPos;
        private Vector2 currentPos;
        private bool isTouching = false;
        private int touchFingerId;
        private bool isEnabled;

        public GameObject virtualPadObject;

        public Button[] buttons;
        public RectTransform lpadPos;

        // 
        public static ControllerBehaviour Instance
        {
            get { return instance; }
        }
        // 初期化時
        void Awake()
        {
            instance = this;
            this.keys = new KeyCode[]
            {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            };
            this.SetupCallback();

            this.buttonClickedBuffer = new bool[buttons.Length];
        }

        // ボタンのコールバック処理
        void SetupCallback()
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                var listner = new ButtonListner(this.OnClickBtn, i);
                buttons[i].onClick.AddListener(listner.Exec);
            }
        }

        // ボタンが押された時の処理
        void OnClickBtn(int idx)
        {
            buttonClickedBuffer[idx] = true;
        }

        // 破棄時の処理
        private void OnDestroy()
        {
            instance = null;
        }

        // 左のパッドの入力
        public Vector3 LPadVector
        {
            get
            {
                Vector3 move;
                // 自動入力時の処理
#if ENABLE_AUTO_CLIENT
                if (NetworkUtility.IsBatchModeRun)
                {
                    return CalculateActualMoveVector(this.dummyInputStick);
                }
#endif
                if (isTouching)
                {
                    var delta = this.currentPos - this.touchedPos;
                    delta /= (Screen.dpi * 0.3f);
                    move = new Vector3(delta.x, 0, delta.y);
                }
                else
                {
                    move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                }
                return CalculateActualMoveVector(move);
            }
        }
        // 斜めに入れたときに移動速度が速くなってしまうので、その対策です
        private Vector3 CalculateActualMoveVector(Vector3 vec)
        {
            float speedValue = 0.0f;
            speedValue = vec.magnitude;
            if (speedValue > 1.0f)
            {
                vec /= speedValue;
            }
            return vec;
        }


        // ボタンを押したかどうか( PC 1-5キー、ボタン)
        public bool IsKeyDown(int idx)
        {
            bool result = Input.GetKeyDown(keys[idx]);
            result |= buttonClickedBuffer[idx];
            return result;
        }

        // アップデート処理後
        public void OnUpdateEnd()
        {
            for (int i = 0; i < buttonClickedBuffer.Length; ++i)
            {
                buttonClickedBuffer[i] = false;
            }
        }

        // コントローラーの有効化
        public void Enable()
        {
            if (Application.isMobilePlatform)
            {
                this.virtualPadObject.SetActive(true);
                this.isEnabled = true;
            }
        }

        // コントローラーの無効化
        public void Disable()
        {
            if (this.virtualPadObject)
            {
                this.virtualPadObject.SetActive(false);
                this.isEnabled = false;
                this.isTouching = false;
            }
        }

        // 更新処理
        private void Update()
        {
            this.UpdateTouchPos();
            this.UpdateLeftStickPosition();
            // バッチモード時のダミー移動処理
#if ENABLE_AUTO_CLIENT
            UpdateDummyInput();
#endif
        }

        // 左側のスティックの更新
        private void UpdateLeftStickPosition()
        {
            var vec = this.LPadVector * 40.0f;
            this.lpadPos.anchoredPosition = new Vector3(vec.x, vec.z, 0f);
        }

        // タッチの処理
        private void UpdateTouchPos()
        {
            if (!this.isEnabled) { return; }
            bool previewTouching = this.isTouching;
            int touchCnt = Input.touchCount;

            if (touchCnt <= 0)
            {
                this.isTouching = false;
                return;
            }


            Touch touch;
            if (!previewTouching)
            {
                for (int i = 0; i < touchCnt; ++i)
                {
                    touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began &&
                        touch.position.x < Screen.width * 0.5f)
                    {

                        this.touchedPos = touch.position;
                        this.currentPos = touch.position;
                        this.touchFingerId = touch.fingerId;
                        this.isTouching = true;
                        break;
                    }
                }
            }
            else
            {
                if (GetTouchFromFingerId(this.touchFingerId, out touch))
                {
                    this.currentPos = touch.position;
                }
                else
                {
                    isTouching = false;
                }
            }
        }

        private bool GetTouchFromFingerId(int fingerId, out Touch touch)
        {
            int cnt = Input.touchCount;
            for (int i = 0; i < cnt; ++i)
            {
                var tmp = Input.GetTouch(i);
                if (tmp.fingerId == fingerId)
                {
                    touch = tmp;
                    return true;
                }
            }
            touch = default;
            return false;
        }


        // バッチビルドのダミー用
#if ENABLE_AUTO_CLIENT

        private float dummyInputStickTimer;
        private float dummyInputVoiceTimer;
        private Vector3 dummyInputStick;

        private void UpdateDummyInput()
        {
            if (!NetworkUtility.IsBatchModeRun)
            {
                return;
            }

            if (dummyInputStickTimer <= 0.0f)
            {
                float rand = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
                dummyInputStick = new Vector3(Mathf.Cos(rand), 0.0f, Mathf.Sign(rand));
                dummyInputStickTimer = UnityEngine.Random.Range(1.0f, 4.0f);
            }
            dummyInputStickTimer -= Time.deltaTime;

            if (dummyInputVoiceTimer < 0.0f)
            {
                dummyInputVoiceTimer = UnityEngine.Random.Range(5.0f, 20.0f);
                this.OnClickBtn(UnityEngine.Random.Range(0, 4));
            }
            dummyInputVoiceTimer -= Time.deltaTime;
        }
#endif


    }
}