using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuisuiTetris
{
    public class TopSceneManager : MonoBehaviour
    {
        private SoundManager _soundManager;

        void Start()
        {
            this._soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            this._soundManager.StartBgm();
        }

        // 通常モード
        public void GameStart()
        {
            GlobalParams.GameMode = Constants.GAME_MODE_NOMAL;
            SceneManager.LoadScene("GameScene");
        }

        // 最適化モード
        public void OptimizeGameStart()
        {
            GlobalParams.GameMode = Constants.GAME_MODE_OPTIMIZE;
            SceneManager.LoadScene("GameScene");
        }

        // 連モード
        public void RenGameStart()
        {
            GlobalParams.GameMode = Constants.GAME_MODE_REN;
            SceneManager.LoadScene("GameScene");
        }
    }
}
