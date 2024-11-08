﻿using Unity.Netcode;
using UnityEngine;


namespace UTJ.NetcodeGameObjectSample
{
    // キャラクターの動きのコントローラー
    public class CharacterMoveController : Unity.Netcode.NetworkBehaviour
    {
        public TextMesh playerNameTextMesh;
        public ParticleSystem soundPlayingParticle;
        public AudioSource audioSouceComponent;


        public AudioClip[] audios;

        private Rigidbody rigidbodyComponent;
        private Animator animatorComponent;

        // Networkで同期する変数を作成します
        #region NETWORKED_VAR
        // Animationに流すスピード変数
        private NetworkVariable<float> speed = new NetworkVariable<float>( 0.0f,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        // プレイヤー名
        private NetworkVariable<Unity.Collections.FixedString64Bytes> playerName = 
            new NetworkVariable<Unity.Collections.FixedString64Bytes>("",NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
        #endregion NETWORKED_VAR


        private void Awake()
        {
            this.rigidbodyComponent = this.GetComponent<Rigidbody>();
            this.animatorComponent = this.GetComponent<Animator>();

            // Player名が変更になった時のコールバック指定
            this.playerName.OnValueChanged += OnChangePlayerName;

            // あとServer時に余計なものを削除します
#if UNITY_SERVER
            NetworkUtility.RemoveAllStandaloneComponents(this.gameObject);
#elif ENABLE_AUTO_CLIENT
            if (NetworkUtility.IsBatchModeRun)
            {
                NetworkUtility.RemoveAllStandaloneComponents(this.gameObject);
            }
#endif
        }


        private void Start()
        {
            if (IsOwner)
            {
                // Set player name
                this.playerName.Value = ConfigureConnectionBehaviour.playerName;
                // コントローラーの有効化をします
                ControllerBehaviour.Instance.Enable();
            }
        }
        private new void OnDestroy()
        {
            base.OnDestroy();
            if (IsOwner)
            {
                // コントローラーの無効化をします
                if (ControllerBehaviour.Instance)
                {
                    ControllerBehaviour.Instance.Disable();
                }
            }
        }

        // player名変更のコールバック
        void OnChangePlayerName(Unity.Collections.FixedString64Bytes prev,
            Unity.Collections.FixedString64Bytes current)
        {
            if (playerNameTextMesh != null)
            {
                playerNameTextMesh.text = current.Value;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // TODO:::なんか OnValueChangedがおかしい…。
            // 自分より前にSpawnされた人の名前取れないんで Workaround
            playerNameTextMesh.text = this.playerName.Value.Value;
            // Animatorの速度更新(歩き・走り・静止などをSpeedでコントロールしてます)
            animatorComponent.SetFloat("Speed", speed.Value);
            // 音量調整
            this.audioSouceComponent.volume = SoundVolume.VoiceValue;

            // オーナーとして管理している場合、ここのUpdateを呼びます
            if (IsOwner)
            {
                UpdateAsOwner();
            }
        }


        // オーナーとしての処理
        private void UpdateAsOwner()
        {
            // 移動処理
            Vector3 move = ControllerBehaviour.Instance.LPadVector;
            float speedValue = move.magnitude;
            this.speed.Value = speedValue;
            move *= Time.deltaTime * 4.0f;
            rigidbodyComponent.position += move;

            // 移動している方角に向きます
            if (move.sqrMagnitude > 0.00001f)
            {
                rigidbodyComponent.rotation = Quaternion.LookRotation(move, Vector3.up);
            }
            // 底に落ちたら適当に復帰します。
            if (transform.position.y < -10.0f)
            {
                var randomPosition = new Vector3(Random.Range(-7, 7), 5.0f, Random.Range(-7, 7));
                transform.position = randomPosition;
            }
            // キーを押して音を流します
            for (int i = 0; i < this.audios.Length; ++i)
            {
                if (ControllerBehaviour.Instance.IsKeyDown(i))
                {
                    PlayAudioRpc(i);
                }
            }
            // 入力の通知を通知します
            ControllerBehaviour.Instance.OnUpdateEnd();
        }


        [Rpc(SendTo.Everyone)]
        private void PlayAudioRpc(int idx) { 
            this.audioSouceComponent.clip = audios[idx];
            this.audioSouceComponent.Play();

            this.soundPlayingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var mainModule = soundPlayingParticle.main;
            mainModule.duration = audios[idx].length;

            this.soundPlayingParticle.Play();
        }
    }
}