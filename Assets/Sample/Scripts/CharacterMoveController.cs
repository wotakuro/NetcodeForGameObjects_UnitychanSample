using MLAPI.NetworkedVar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UTJ.MLAPISample
{
    // キャラクターの動きのコントローラー
    public class CharacterMoveController : MLAPI.NetworkedBehaviour
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
        private NetworkedVar<float> speed = new NetworkedVar<float>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.OwnerOnly }, 0.0f);
        // プレイヤー名
        private NetworkedVar<string> playerName = new NetworkedVar<string>(new NetworkedVarSettings { WritePermission = NetworkedVarPermission.OwnerOnly }, "");
        #endregion NETWORKED_VAR

        private void Awake()
        {
            this.rigidbodyComponent = this.GetComponent<Rigidbody>();
            this.animatorComponent = this.GetComponent<Animator>();

            // Player名が変更になった時のコールバック指定
            this.playerName.OnValueChanged += OnChangePlayerName;
        }
        private void Start()
        {
            if (IsOwner)
            {
                // プレイヤー名をセットします
                this.playerName.Value = ConfigureConnectionBehaviour.playerName;
                // コントローラーの有効化をします
                ControllerBehaviour.Instance.Enable();
            }
        }
        private void OnDestroy()
        {
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
        void OnChangePlayerName(string prev, string current)
        {
            if (playerNameTextMesh != null)
            {
                playerNameTextMesh.text = current;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Animatorの速度更新(歩き・走り・静止などをSpeedでコントロールしてます)
            animatorComponent.SetFloat("Speed", speed.Value);
            // 音量調整
            this.audioSouceComponent.volume = SoundVolume.VoiceValue;

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
                    // 他の人に流してもらうために、サーバーにRPCします。
                    InvokeServerRpc(PlayAudioRequestOnServer, i);
                    // 自分だけは先行して流しておきます                
                    PlayAudio(i);
                }
            }
            // 入力の通知を通知します
            ControllerBehaviour.Instance.OnUpdateEnd();
        }

        // Clientからサーバーに呼び出されるRPCです。
        [MLAPI.Messaging.ServerRPC(RequireOwnership = true)]
        private void PlayAudioRequestOnServer(int idx)
        {
            // このオブジェクトのオーナー以外に対して PlayAudioを呼び出します
            InvokeClientRpcOnEveryoneExcept(PlayAudio, this.OwnerClientId, idx);
        }

        // 音を再生します。付随してParticleをPlayします
        [MLAPI.Messaging.ClientRPC]
        private void PlayAudio(int idx)
        {
            this.audioSouceComponent.clip = audios[idx];
            this.audioSouceComponent.Play();

            this.soundPlayingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var mainModule = soundPlayingParticle.main;
            mainModule.duration = audios[idx].length;

            this.soundPlayingParticle.Play();
        }
    }
}