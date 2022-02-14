using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace SuisuiTetris
{
    public class SoundManager : MonoBehaviour
    {
        private AudioSource _bgmAudioSource;
        private AudioSource _jingleAudioSource;

        private AudioClip _seGameMove;

        private bool _startFlg = true;
        private string _beforSceneName;

        void Start()
        {
            // 既にサウンドマネージャーが存在していれば自滅させる
            GameObject[] soundManagers = GameObject.FindGameObjectsWithTag("SoundManager");
            if (1 < soundManagers.Length) {
                Destroy(this.gameObject);
                return;
            }

            // Sceneを遷移してもオブジェクトが消えないようにする
            DontDestroyOnLoad(this);

            // シーンを読み込んだときのイベント処理
            SceneManager.sceneUnloaded += (Scene currentScene) => {
                this._beforSceneName = currentScene.name;
            };

            this.initSoundManager();
        }

        private void initSoundManager()
        {
            // AudioSourceを保持しておく
            if (this._bgmAudioSource == null) {
                this._bgmAudioSource = this.gameObject.AddComponent<AudioSource>();
            }
            if (this._jingleAudioSource == null) {
                this._jingleAudioSource = this.gameObject.AddComponent<AudioSource>();
            }

            // SEは予め全種類保持しておく
            if (this._seGameMove == null) {
                this._seGameMove = Resources.Load<AudioClip>("Sound/SE_Game_Move");
            }
        }

        // BGMの再生
        public void StartBgm()
        {
            this.initSoundManager();

            // BGMを再生させる
            if (this.CheckChangeBgm()) {
                string sceneName = SceneManager.GetActiveScene().name;

                string bgmResourceName = "Sound/BGM_TopScene";
                switch (sceneName) {
                    case "GameScene":
                        bgmResourceName = "Sound/BGM_GameScene";
                        break;
                    default:
                        bgmResourceName = "Sound/BGM_TopScene";
                        break;
                }

                this.playBgm(bgmResourceName);
            }
        }

        // BGMの変更が必要かをチェックする
        public bool CheckChangeBgm()
        {
            // 初回は無条件でBGMを再生させる
            if (this._startFlg) {
                this._startFlg = false;
                return true;
            }

            string currentSceneName = SceneManager.GetActiveScene().name;

            bool changeBgmFlg = false;

            // シーン読み込み時にBGMの切り替えが必要な場合
            switch (currentSceneName) {
                case "TopScene":
                case "GameScene":
                    changeBgmFlg = true;
                    break;
            }

            return changeBgmFlg;
        }

        // BGMの再生
        private async UniTask playBgm(string bgmResourceName)
        {
            // 現在再生中なのであればフェードアウト後に実行
            if (this._bgmAudioSource.isPlaying) {
                await this._bgmAudioSource.DOFade(0.0f, 0.3f);
                this._bgmAudioSource.Stop();
            }

            // BGMが無ければ再生しない
            if (bgmResourceName == "") {
                return;
            }

            this._bgmAudioSource.clip = Resources.Load<AudioClip>(bgmResourceName);
            this._bgmAudioSource.volume = 0.8f;
            this._bgmAudioSource.loop = true;
            this._bgmAudioSource.Play();
        }

        // BGMの停止
        public async UniTask StopBgm()
        {
            if (this._bgmAudioSource.isPlaying) {
                // フェードアウト
                await this._bgmAudioSource.DOFade(0.0f, 0.3f);
                this._bgmAudioSource.Stop();
            }
        }

        // SEの実行
        public async UniTask PlaySe(string seResourceName)
        {
            // オーディオソースをアタッチする
            AudioSource seAudioSource = this.gameObject.AddComponent<AudioSource>();

            // 読み込み前とかでちゃんと作成できなかったら処理しない
            if (seAudioSource == null) {
                return;
            }

            switch (seResourceName) {
                case "Game_Move":
                    seAudioSource.clip = this._seGameMove;
                    seAudioSource.volume = 0.7f;
                    break;
            }

            if (seAudioSource.clip != null) {
                seAudioSource.Play();
                await UniTask.WaitWhile(() => seAudioSource.isPlaying);
            }

            // 再生終了後に消去する
            Destroy(seAudioSource);
        }

        // Jingleの実行
        public void PlayJingle(string jingleResourceName)
        {
            if (this._jingleAudioSource.isPlaying) {
                this._jingleAudioSource.Stop();
            }

            string jingleResourceNamePath = "";
            switch (jingleResourceName) {
                case "Game_GameOver":
                    jingleResourceNamePath = "Sounds/JINGLE_Game_GameOver";
                    break;
            }

            if (jingleResourceNamePath != "") {
                this._jingleAudioSource.clip = Resources.Load<AudioClip>(jingleResourceNamePath);
                this._jingleAudioSource.volume = 1.0f;
                this._jingleAudioSource.Play();
            }
        }
    }
}
