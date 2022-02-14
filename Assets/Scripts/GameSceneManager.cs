using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SuisuiTetris
{
    public class GameSceneManager : MonoBehaviour
    {
        private GameObject _stage;
        private GameObject _next;
        private GameObject _hold;
        private GameObject _text;
        private GameObject _deleteLine;
        private GameObject _renCount;
        private GameObject _maxRenCount;
        private GameObject _gameOver;

        private GameObject _imino;
        private GameObject _jmino;
        private GameObject _lmino;
        private GameObject _omino;
        private GameObject _smino;
        private GameObject _tmino;
        private GameObject _zmino;
        private GameObject _iminoPart;
        private GameObject _jminoPart;
        private GameObject _lminoPart;
        private GameObject _ominoPart;
        private GameObject _sminoPart;
        private GameObject _tminoPart;
        private GameObject _zminoPart;

        private SoundManager _soundManager;

        // ステージに関する属性
        private float _pastTime;     // ゲームの経過時間
        private float _speed;        // ミノの落下速度
        private int[,] _stagePos;    // ステージ内の情報
        private int _maxPosX = 9;    // ステージの横幅 0～9 constのがいいかも
        private int _maxPosY = 21;   // ステージの立幅 0～ 21 constのがいいかも
        private bool _holdFlg;       // ミノが落下する最中にホールドを１回のみに制限させるフラグ
        private int _deleteLineNum;  // 消したラインの数
        private int _renCountNum;    // 現在のレンの数
        private bool _renCountUpFlg; // レンが繋がっている間はtrue、レンが途切れたらfalseになる
        private int _maxRenCountNum; // 最大レン数

        // 落下中のミノの属性
        private bool _minoFreezeFlg;       // ミノが硬直してから次のミノが出現するまで操作を受け付けないようにするフラグ   
        private GameObject _activeMino;    // 現在操作しているミノのオブジェクト
        private int _activeMinoType;       // 現在操作しているミノのタイプ
        private int _activeMinoX;          // 現在操作しているミノの横位置 0～9
        private int _activeMinoY;          // 現在操作しているミノの縦位置 0～21
        private float _activeMinoFallTime; // 現在操作しているミノが次のマス目に落下するまでの経過時間、移動毎にゼロになる
        private bool _dropWaitFlg;         // 落下終了してミノの硬直カウントを開始した場合にtrue
        private float _dropWaitTime;       // 落下終了してからの経過時間
        private int _dropWaitCancelCount;  // 硬直を拒否した回数

        // 最適化関連の属性
        private int _moveCount; // ミノを動かした回数のカウント（最適化モードのときに利用する）
        private int _turnCount; // ミノを回転させた回数のカウント（最適化モードのときに利用する）

        void Start()
        {
            this._pastTime = 0.0f;
            this._stage       = GameObject.Find("Canvas").transform.Find("Stage").gameObject;
            this._next        = GameObject.Find("Canvas").transform.Find("Next").gameObject;
            this._hold        = GameObject.Find("Canvas").transform.Find("Hold").gameObject;
            this._text        = GameObject.Find("Canvas").transform.Find("Text").gameObject;
            this._deleteLine  = GameObject.Find("Canvas").transform.Find("DeleteLine").gameObject;
            this._renCount    = GameObject.Find("Canvas").transform.Find("RenCount").gameObject;
            this._maxRenCount = GameObject.Find("Canvas").transform.Find("MaxRenCount").gameObject;
            this._gameOver    = GameObject.Find("Canvas").transform.Find("GameOver").gameObject;

            this._imino = Resources.Load<GameObject>("Prefab/IminoBase");
            this._jmino = Resources.Load<GameObject>("Prefab/JminoBase");
            this._lmino = Resources.Load<GameObject>("Prefab/LminoBase");
            this._omino = Resources.Load<GameObject>("Prefab/OminoBase");
            this._smino = Resources.Load<GameObject>("Prefab/SminoBase");
            this._tmino = Resources.Load<GameObject>("Prefab/TminoBase");
            this._zmino = Resources.Load<GameObject>("Prefab/ZminoBase");
            this._iminoPart = Resources.Load<GameObject>("Prefab/IminoPart");
            this._jminoPart = Resources.Load<GameObject>("Prefab/JminoPart");
            this._lminoPart = Resources.Load<GameObject>("Prefab/LminoPart");
            this._ominoPart = Resources.Load<GameObject>("Prefab/OminoPart");
            this._sminoPart = Resources.Load<GameObject>("Prefab/SminoPart");
            this._tminoPart = Resources.Load<GameObject>("Prefab/TminoPart");
            this._zminoPart = Resources.Load<GameObject>("Prefab/ZminoPart");

            this._soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            this._soundManager.StartBgm();

            // ステージを初期化
            this._renCountNum = 0;
            this._renCountUpFlg = false;
            this._renCount.GetComponent<Text>().text = this._renCountNum.ToString("N0");
            this._maxRenCountNum = 0;
            this._maxRenCount.GetComponent<Text>().text = this._maxRenCountNum.ToString("N0");
            this._deleteLineNum = 0;
            this._deleteLine.GetComponent<Text>().text = this._deleteLineNum.ToString("N0");
            this._dropWaitFlg = false;
            this._dropWaitTime = 0.0f;
            this._dropWaitCancelCount = 0;
            this._holdFlg = false;
            this._speed = 3.0f;
            this._stagePos = new int[this._maxPosX + 1, this._maxPosY + 1];
            this.stageClear();
            this._gameOver.SetActive(false);
            

            // 最適化関連の属性を初期化
            this._moveCount = 0;
            this._turnCount = 0;           

            // Nextを初期化する
            this._next.GetComponent<NextMinoAction>().InitNextMino();

            // 最初のミノを作成する
            int minoNum = this._next.GetComponent<NextMinoAction>().GetNextMinoType();
            this.makeMino(minoNum);
        }

        // ステージをクリアする
        private void stageClear()
        {
            if (GlobalParams.GameMode == Constants.GAME_MODE_REN) {
                // 連モードのとき
                for (int i = 0; i <= this._maxPosX; i++) {
                    for (int j = 0; j <= this._maxPosY; j++) {
                        // タネ3を作る
                        if (j == 0) {
                            if (i == 3) {
                                this._stagePos[i, j] = 0;
                            } else {
                                this._stagePos[i, j] = 1;
                            }
                        } else {
                            if (i == 3 || i == 4 || i == 5 || i == 6) {
                                this._stagePos[i, j] = 0;
                            } else {
                                this._stagePos[i, j] = 1;
                            }
                        }
                    }
                }
            } else {
                // 通常
                for (int i = 0; i <= this._maxPosX; i++) {
                    for (int j = 0; j <= this._maxPosY; j++) {
                        this._stagePos[i, j] = 0;
                    }
                }
            }

            // テスト用のステージ
            //this._stagePos = this.GetComponent<TestStageManager>().GetStage25();

            this.redrawStage();
        }

        void Update()
        {
            this._pastTime += Time.deltaTime;
            this._activeMinoFallTime += Time.deltaTime;

            if (this._minoFreezeFlg) {
                return;
            }

            this.fallMino();
        }

        // 左移動を操作したとき
        public void MoveLeft()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合は2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                this._moveCount += 1;
                if (2 < this._moveCount) {
                    return;
                }
            }

            bool moveFlg = this.setMinoStagePos(this._activeMinoX - 1, this._activeMinoY);
            if (moveFlg) {
                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 1;
            }
        }

        // 長押しで左移動を操作したとき
        public void HoldMoveLeft()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合はカウントアップしないが2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                if (2 < this._moveCount) {
                    return;
                }
            }

            bool moveFlg = this.setMinoStagePos(this._activeMinoX - 1, this._activeMinoY);
            if (moveFlg) {
                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 1;
            }
        }

        // 右移動を操作したとき
        public void MoveRight()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合は2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                this._moveCount += 1;
                if (2 < this._moveCount) {
                    return;
                }
            }

            bool moveFlg = this.setMinoStagePos(this._activeMinoX + 1, this._activeMinoY);
            if (moveFlg) {
                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 1;
            }
        }

        // 長押しで右移動を操作したとき
        public void HoldMoveRight()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合はカウントアップしないが2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                if (2 < this._moveCount) {
                    return;
                }
            }

            bool moveFlg = this.setMinoStagePos(this._activeMinoX + 1, this._activeMinoY);
            if (moveFlg) {
                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 1;
            }
        }

        // 下移動を操作したとき
        public void MoveDown()
        {
            if (this._minoFreezeFlg) {
                return;
            }
            this.setMinoStagePos(this._activeMinoX, this._activeMinoY - 1);
        }

        // 上を押したとき
        public void MoveUp()
        {
            if (this._minoFreezeFlg) {
                return;
            }
            this.hardDrop();
        }

        // 左回転を操作したとき
        public void TurnLeft()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合は2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                this._turnCount += 1;
                if (2 < this._turnCount) {
                    return;
                }
            }

            this.turnMino(false);
        }

        // 右回転を操作したとき
        public void TurnRight()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            // 最適化モードの場合は2回以上操作させない
            if (GlobalParams.GameMode == Constants.GAME_MODE_OPTIMIZE) {
                this._turnCount += 1;
                if (2 < this._turnCount) {
                    return;
                }
            }

            this.turnMino(true);
        }

        // リスタートしたとき
        public void Restart()
        {
            // ステージをクリア
            this.stageClear();

            // 現在のミノを削除
            Destroy(this._activeMino);

            // Nextを初期化する
            this._next.GetComponent<NextMinoAction>().InitNextMino();

            // ホールドを初期化する
            this._hold.GetComponent<HoldMinoAction>().InitHoldMino();

            // 最初のミノを作成する
            int minoNum = this._next.GetComponent<NextMinoAction>().GetNextMinoType();
            this.makeMino(minoNum);
        }

        // ホールドしたとき
        public void Hold()
        {
            if (this._minoFreezeFlg) {
                return;
            }

            if (this._holdFlg) {
                return;
            }

            int newMinoType = this._hold.GetComponent<HoldMinoAction>().SetHold(this._activeMinoType);
            
            // 現在のミノを削除
            Destroy(this._activeMino);

            if (newMinoType == 0) {
                int minoNum = this._next.GetComponent<NextMinoAction>().GetNextMinoType();
                this.makeMino(minoNum);
            } else {
                this.makeMino(newMinoType);
            }

            this._holdFlg = true;
        }

        // ミノを落下させる
        private void fallMino()
        {
            // 経過時間から移動したかどうかをチェック
            int fallDistance = Mathf.FloorToInt(this._activeMinoFallTime * this._speed);

            // 位置をセット
            if (1 <= fallDistance) {
                int nextX = this._activeMinoX;
                int nextY = this._activeMinoY - 1;

                if (!this.checkMinoSet(nextX, nextY) || this.isOutOfStage(nextX, nextY)) {
                    // これ以上落下できない
                    if (this._dropWaitFlg) {
                        // 硬直までにかかる時間を加算する
                        this._dropWaitTime += Time.deltaTime;
                    } else {
                        this._dropWaitTime = 0.0f;
                        this._dropWaitCancelCount = 0;
                        this._dropWaitFlg = true;
                    }

                    // 硬直までにかかる時間が経過した場合か、硬直拒否が20以上であればミノをチェンジ
                    if (Constants.DROP_WAIT < this._dropWaitTime || 20 <= this._dropWaitCancelCount) {
                        this.changeNextMino();
                        this._activeMinoFallTime = 0.0f;
                    }
                    
                } else {
                    this.setMinoStagePos(this._activeMinoX, this._activeMinoY - 1);
                    this._activeMinoFallTime = 0.0f;
                }
            }
        }

        // ハードドロップさせる
        private void hardDrop()
        {
            int nextX = this._activeMinoX;
            int nextY = this._activeMinoY;

            while (true) {
                if (!this.checkMinoSet(nextX, nextY - 1) || this.isOutOfStage(nextX, nextY - 1)) {
                    break;
                }
                nextY--;
            }

            // 一番下まで落とす
            this.setMinoStagePos(nextX, nextY);
            this.changeNextMino();
            this._activeMinoFallTime = 0.0f;
        }

        // 次のミノにチェンジする
        private void changeNextMino()
        {
            this._minoFreezeFlg = true;

            // 現在のミノの状態をステージの情報に保持させる
            this.setStagePos(this._activeMinoType, this._activeMinoX, this._activeMinoY);

            // ラインが揃っていれば消去
            this.removeLine();

            // 連のカウントアップをする
            if (this._renCountUpFlg) {
                this._renCountUpFlg = false;
                this._renCountNum += 1;
                this._renCount.GetComponent<Text>().text = this._renCountNum.ToString("N0");

                // 連の最大数をセット
                if (this._maxRenCountNum < this._renCountNum) {
                    this._maxRenCountNum = this._renCountNum;
                    this._maxRenCount.GetComponent<Text>().text = this._maxRenCountNum.ToString("N0");
                }
            } else {
                this._renCountNum = 0;
                this._renCount.GetComponent<Text>().text = this._renCountNum.ToString("N0");
            }

            // 現在のミノを削除
            Destroy(this._activeMino);

            // ステージを再描画
            this.redrawStage();

            // デバッグ出力
            this.viewStagePos();

            // 落下硬直の待ちフラグをリセット
            this._dropWaitFlg = false;

            // ミノを作成
            int minoNum = this._next.GetComponent<NextMinoAction>().GetNextMinoType();
            this.makeMino(minoNum);
        }

        // ミノをステージのポジションにセットする
        private bool setMinoStagePos(int posX, int posY)
        {
            // セットが可能かどうかをチェック
            if (!this.checkMinoSet(posX, posY) || this.isOutOfStage(posX, posY)) {
                return false;
            }

            // 移動実行
            this._activeMinoX = posX;
            this._activeMinoY = posY;
            this._activeMino.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX * Constants.BOX_SIZE, posY * Constants.BOX_SIZE);

            return true;
        }

        // ミノを作成する
        private void makeMino(int minoNum)
        {
            switch (minoNum) {
                case Constants.MINO_TYPE_I:
                    this._activeMino = Instantiate(this._imino, this._stage.transform);
                    this._activeMino.GetComponent<IminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_I;
                    this._activeMinoX = 3;
                    this._activeMinoY = 17;
                    break;
                case Constants.MINO_TYPE_J:
                    this._activeMino = Instantiate(this._jmino, this._stage.transform);
                    this._activeMino.GetComponent<JminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_J;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
                case Constants.MINO_TYPE_L:
                    this._activeMino = Instantiate(this._lmino, this._stage.transform);
                    this._activeMino.GetComponent<LminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_L;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
                case Constants.MINO_TYPE_O:
                    this._activeMino = Instantiate(this._omino, this._stage.transform);
                    this._activeMino.GetComponent<OminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_O;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
                case Constants.MINO_TYPE_S:
                    this._activeMino = Instantiate(this._smino, this._stage.transform);
                    this._activeMino.GetComponent<SminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_S;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
                case Constants.MINO_TYPE_T:
                    this._activeMino = Instantiate(this._tmino, this._stage.transform);
                    this._activeMino.GetComponent<TminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_T;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
                case Constants.MINO_TYPE_Z:
                    this._activeMino = Instantiate(this._zmino, this._stage.transform);
                    this._activeMino.GetComponent<ZminoAction>().Init();
                    this._activeMinoType = Constants.MINO_TYPE_Z;
                    this._activeMinoX = 3;
                    this._activeMinoY = 18;
                    break;
            }

            // 位置をセット
            this.setMinoStagePos(this._activeMinoX, this._activeMinoY);

            // 落下時間をゼロにする
            this._activeMinoFallTime = 0.0f;

            // ホールドを解除する
            this._holdFlg = false;

            // 最適化関連の属性を初期化
            this._moveCount = 0;
            this._turnCount = 0;

            // ミノが重なっていた場合はゲームオーバー
            if (!this.checkAllOK(this._activeMinoX, this._activeMinoY)) {
                this._gameOver.SetActive(true);
            }

            this._minoFreezeFlg = false;
        }

        // ミノを回転させる
        private void turnMino(bool rightTurn)
        {
            switch (this._activeMinoType) {
                case Constants.MINO_TYPE_I:
                    this._activeMino.GetComponent<IminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_J:
                    this._activeMino.GetComponent<JminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_L:
                    this._activeMino.GetComponent<LminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_O:
                    this._activeMino.GetComponent<OminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_S:
                    this._activeMino.GetComponent<SminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_T:
                    this._activeMino.GetComponent<TminoAction>().Turn(rightTurn);
                    break;
                case Constants.MINO_TYPE_Z:
                    this._activeMino.GetComponent<ZminoAction>().Turn(rightTurn);
                    break;
            }

            // セットが可能かどうかをチェックしてセットできないのであればスピン
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY)) {
                // 回転したのであれば硬直にかかる時間をリセット
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 2;

                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");
            } else {
                this.spin(rightTurn);
            }
        }

        // 現在のミノのポジション情報を取得する
        private int[,] getActiveMinoPos()
        {
            int[,] minoPos = new int[4, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };

            switch (this._activeMinoType) {
                case Constants.MINO_TYPE_I:
                    minoPos = this._activeMino.GetComponent<IminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_J:
                    minoPos = this._activeMino.GetComponent<JminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_L:
                    minoPos = this._activeMino.GetComponent<LminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_O:
                    minoPos = this._activeMino.GetComponent<OminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_S:
                    minoPos = this._activeMino.GetComponent<SminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_T:
                    minoPos = this._activeMino.GetComponent<TminoAction>().GetMinoPos();
                    break;
                case Constants.MINO_TYPE_Z:
                    minoPos = this._activeMino.GetComponent<ZminoAction>().GetMinoPos();
                    break;
            }
            return minoPos;
        }

        // どっちも大丈夫チェック
        private bool checkAllOK(int x, int y)
        {
            if (this.checkMinoSet(x, y) && !this.isOutOfStage(x, y)) {
                return true;
            } else {
                return false;
            }
        }

        // ミノが存在しないかをチェック（ステージ外は無視する）
        private bool checkMinoSet(int x, int y)
        {
            int[,] minoPos = this.getActiveMinoPos();

            bool check = true;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (minoPos[i, j] != 0) {
                        if (x + i < 0 || this._maxPosX < x + i || y + j < 0 || this._maxPosY < y + j) {
                            continue;
                        }
                        if (this._stagePos[i + x, j + y] != 0) {
                            check = false;
                            break;
                        }
                    }
                }
            }

            return check;
        }

        // ステージ外かチェック
        private bool isOutOfStage(int x, int y)
        {
            int[,] minoPos = this.getActiveMinoPos();

            bool check = false;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (minoPos[i, j] != 0) {
                        if (x + i < 0 || this._maxPosX < x + i || y + j < 0 || this._maxPosY < y + j) {
                            check = true;
                            break;
                        }
                    }
                }
                if (check) {
                    break;
                }
            }

            return check;
        }

        // ミノの情報をセットする
        private void setStagePos(int minoType, int posX, int posY)
        {
            int[,] minoPos = this.getActiveMinoPos();

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    if (minoPos[i, j] != 0) {
                        if (posX + i < 0 || this._maxPosX < posX + i || posY + j < 0 || this._maxPosY < posY + j) {
                            continue;
                        }
                        this._stagePos[i + posX, j + posY] = minoType;
                    }
                }
            }
        }

        // 横が揃っていた場合にラインを消す
        private void removeLine()
        {
            bool removeLineFlg = false;
            int removeLineNum = 0;

            // 消去するラインを探す
            for (int y = 0; y <= this._maxPosY; y++) {
                bool lineFilled = true;
                for (int x = 0; x <= this._maxPosX; x++) {
                    if (this._stagePos[x, y] == 0) {
                        lineFilled = false;
                        break;
                    }
                }
                if (lineFilled) {
                    removeLineFlg = true;
                    removeLineNum = y;
                    break;
                }
            }

            // ライン消去を実行
            if (removeLineFlg) {
                for (int y = removeLineNum; y <= this._maxPosY; y++) {
                    for (int x = 0; x <= this._maxPosX; x++) {
                        if (y + 1 < this._maxPosY) {
                            this._stagePos[x, y] = this._stagePos[x, y + 1];
                        } else {
                            if (GlobalParams.GameMode == Constants.GAME_MODE_REN) {
                                // 連モードの場合
                                if (x == 3 || x == 4 || x == 5 || x == 6) {
                                    this._stagePos[x, y] = 0;
                                } else {
                                    this._stagePos[x, y] = 1;
                                }
                            } else {
                                // 通常モードの場合
                                this._stagePos[x, y] = 0;
                            }
                        }
                    }
                }

                // 消したラインをカウントアップ
                this._deleteLineNum += 1;
                this._deleteLine.GetComponent<Text>().text = this._deleteLineNum.ToString("N0");

                // 消したラインに合わせて速度を変える
                this._speed = Mathf.FloorToInt(this._deleteLineNum / 10) + 3;

                // 連のカウントアップをさせる
                this._renCountUpFlg = true;

                // 再起的にラインを消去
                this.removeLine();
            }
        }

        // 再描画
        private void redrawStage()
        {
            Transform minoPartsTransform = this._stage.transform.Find("MinoParts");

            // 一旦全部消す
            foreach (Transform child in minoPartsTransform) {
                Destroy(child.gameObject);
            }

            for (int i = 0; i <= this._maxPosX; i++) {
                for (int j = 0; j <= this._maxPosY; j++) {
                    int minoType = this._stagePos[i, j];
                    if (minoType != 0) {
                        GameObject minoPart = new GameObject();
                        switch (minoType) {
                            case Constants.MINO_TYPE_I:
                                minoPart = Instantiate(this._iminoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_J:
                                minoPart = Instantiate(this._jminoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_L:
                                minoPart = Instantiate(this._lminoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_O:
                                minoPart = Instantiate(this._ominoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_S:
                                minoPart = Instantiate(this._sminoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_T:
                                minoPart = Instantiate(this._tminoPart, minoPartsTransform);
                                break;
                            case Constants.MINO_TYPE_Z:
                                minoPart = Instantiate(this._zminoPart, minoPartsTransform);
                                break;
                        }
                        minoPart.GetComponent<RectTransform>().anchoredPosition = new Vector2(i * Constants.BOX_SIZE, j * Constants.BOX_SIZE);
                    }
                }
            }
        }

        // 強制的にテスト用のステージに差し替える
        public void ResetStage(int[,] stageDat)
        {
            this._stagePos = stageDat;
            this.redrawStage();
        }

        // 内部的に保持している情報をデバッグ出力する
        private void viewStagePos()
        {
            string stagePosStr = "";
            for (int y = this._maxPosY; 0 <= y; y--) {
                for (int x = 0; x <= this._maxPosX; x++) {
                    stagePosStr += this._stagePos[x, y] + ",";
                }
                stagePosStr += "\n";
            }

            this._text.GetComponent<Text>().text = stagePosStr;
        }

        // ゲームオーバーで戻るボタンを押したとき
        public void OnBackBtn()
        {
            SceneManager.LoadScene("TopScene");
        }

        //------------------------------------------------------------------------------------------------------------ スピンの処理
        private void spin(bool rightTurn)
        {
            bool spinFlg = false;
            switch (this._activeMinoType) {
                case Constants.MINO_TYPE_I:
                    spinFlg = this.iSpin();
                    break;
                case Constants.MINO_TYPE_L:
                case Constants.MINO_TYPE_J:
                    spinFlg = this.ljSpin();
                    break;
                case Constants.MINO_TYPE_S:
                case Constants.MINO_TYPE_Z:
                    spinFlg = this.szSpin();
                    break;
                case Constants.MINO_TYPE_T:
                    spinFlg = this.tSpin();
                    break;
            }

            // スピンできなければ元に戻す
            if (spinFlg) {
                // 回転したのであれば硬直にかかる時間をリセット
                this._dropWaitTime = 0.0f;
                this._dropWaitCancelCount += 2;

                // SEを鳴らす
                this._soundManager.PlaySe("Game_Move");

            } else {
                switch (this._activeMinoType) {
                    case Constants.MINO_TYPE_I:
                        this._activeMino.GetComponent<IminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_J:
                        this._activeMino.GetComponent<JminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_L:
                        this._activeMino.GetComponent<LminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_O:
                        this._activeMino.GetComponent<OminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_S:
                        this._activeMino.GetComponent<SminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_T:
                        this._activeMino.GetComponent<TminoAction>().Turn(!rightTurn);
                        break;
                    case Constants.MINO_TYPE_Z:
                        this._activeMino.GetComponent<ZminoAction>().Turn(!rightTurn);
                        break;
                }
            }
        }

        private bool iSpin()
        {
            // 下に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 1)) {
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左下に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右下に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 1)) {
                this._activeMinoX += 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 1)) {
                if (iSpinSpecial()) {
                    return true;
                }
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に2つ動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 2)) {
                if (iSpinSpecial()) {
                    return true;
                }
                this._activeMinoY += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY)) {
                this._activeMinoX -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY)) {
                this._activeMinoX += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に1マス下に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 2)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左上に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY + 1)) {
                if (iSpinSpecial()) {
                    return true;
                }
                this._activeMinoX -= 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右上に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY + 1)) {
                if (iSpinSpecial()) {
                    return true;
                }
                this._activeMinoX += 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX - 2, this._activeMinoY)) {
                this._activeMinoX -= 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX + 2, this._activeMinoY)) {
                this._activeMinoX += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }
            
            // 右に2マス下に1マス動かした場合
            if (this.checkAllOK(this._activeMinoX + 2, this._activeMinoY - 1)) {
                this._activeMinoX += 2;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に2マス下に1マス動かした場合
            if (this.checkAllOK(this._activeMinoX - 2, this._activeMinoY - 1)) {
                this._activeMinoX -= 2;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            return false;
        }

        // 特殊スピン
        private bool iSpinSpecial()
        {
            // ステージ外であればfalseを返す
            if (isOutOfStage(this._activeMinoX, this._activeMinoY)) {
                return false;
            }

            // 左側に屋根がある場合
            if (this._stagePos[this._activeMinoX, this._activeMinoY + 3] != 0) {
                if (this.checkAllOK(this._activeMinoX - 2, this._activeMinoY - 1)) {
                    this._activeMinoX -= 2;
                    this._activeMinoY -= 1;
                    this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                    return true;
                }
            }

            // 右側に屋根がある場合
            if (this._stagePos[this._activeMinoX + 3, this._activeMinoY + 2] != 0) {
                if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 2)) {
                    this._activeMinoX += 1;
                    this._activeMinoY -= 2;
                    this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                    return true;
                }
            }

            return false;
        }

        private bool ljSpin()
        {
            // 上に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 1)) {
                if (this.ljSpinSpecial()) {
                    return true;
                }
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY)) {
                this._activeMinoX -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY)) {
                this._activeMinoX += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 下に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 1)) {
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左下に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右下に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 1)) {
                this._activeMinoX += 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左上に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY + 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右上に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY + 1)) {
                this._activeMinoX += 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 2)) {
                this._activeMinoY += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に2マス左に1マス動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY + 2)) {
                this._activeMinoX -= 1;
                this._activeMinoY += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に2マス右に1マス動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY + 2)) {
                this._activeMinoX += 1;
                this._activeMinoY += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            return false;
        }

        // 特殊スピン
        private bool ljSpinSpecial()
        {
            // 左下に動かして入れば入れる
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右下に動かして入れば入れる
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 1)) {
                this._activeMinoX += 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 屋根がある場合
            if (this._activeMinoType == Constants.MINO_TYPE_J) {
                if (this._stagePos[this._activeMinoX + 2, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX + 2, this._activeMinoY + 3] != 0) {
                    if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                    if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 2)) {
                        this._activeMinoX += 1;
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                }
            }
            if (this._activeMinoType == Constants.MINO_TYPE_L) {
                if (this._stagePos[this._activeMinoX, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX, this._activeMinoY + 3] != 0) {
                    if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                    if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 2)) {
                        this._activeMinoX -= 1;
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool szSpin()
        {
            // 下に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 1)) {
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左下に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右下に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 1)) {
                this._activeMinoX += 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に動かした場合特殊スピンができるのであれば実行する
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 1)) {
                if (this.szSpinSpecial()) {
                    return true;
                }
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左上に動かした場合特殊スピンができるのであれば実行する
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY + 1)) {
                if (this.szSpinSpecial()) {
                    return true;
                }
                this._activeMinoX -= 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右上に動かした場合特殊スピンができるのであれば実行する
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY + 1)) {
                if (this.szSpinSpecial()) {
                    return true;
                }
                this._activeMinoX += 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY)) {
                this._activeMinoX -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY)) {
                this._activeMinoX += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            return false;
        }

        // 特殊スピン
        private bool szSpinSpecial()
        {
            // ステージ外であればfalseを返す
            if (isOutOfStage(this._activeMinoX, this._activeMinoY)) {
                return false;
            }

            // Sミノの場合
            if (this._activeMinoType == Constants.MINO_TYPE_S) {
                // 屋根がある場合
                if (this._stagePos[this._activeMinoX, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX, this._activeMinoY + 3] != 0) {
                    if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                    if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 2)) {
                        this._activeMinoX -= 1;
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                } else {
                    // 変態スピン
                    if (this._stagePos[this._activeMinoX + 2, this._activeMinoY + 1] != 0) {
                        if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                            this._activeMinoY -= 2;
                            this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                            return true;
                        }
                    }
                }
            }

            // Zミノの場合
            if (this._activeMinoType == Constants.MINO_TYPE_Z) {
                // 屋根がある場合
                if (this._stagePos[this._activeMinoX + 2, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX + 2, this._activeMinoY + 3] != 0) {
                    if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                    if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 2)) {
                        this._activeMinoX += 1;
                        this._activeMinoY -= 2;
                        this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                        return true;
                    }
                } else {
                    // 変態スピン
                    if (this._stagePos[this._activeMinoX, this._activeMinoY + 1] != 0) {
                        if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                            this._activeMinoY -= 2;
                            this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool tSpin()
        {
            // 下に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 1)) {
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左下に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 1)) {
                this._activeMinoX -= 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右下に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 1)) {
                this._activeMinoX += 1;
                this._activeMinoY -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 1)) {
                if (this.tSpinSpecial()) {
                    return true;
                }
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 下に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY - 2)) {
                this._activeMinoY -= 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 上に2マス動かした場合
            if (this.checkAllOK(this._activeMinoX, this._activeMinoY + 2)) {
                this._activeMinoY += 2;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左上に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY + 1)) {
                if (this.tSpinSpecial()) {
                    return true;
                }
                this._activeMinoX -= 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右上に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY + 1)) {
                if (this.tSpinSpecial()) {
                    return true;
                }
                this._activeMinoX += 1;
                this._activeMinoY += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 左に動かした場合
            if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY)) {
                this._activeMinoX -= 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            // 右に動かした場合
            if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY)) {
                this._activeMinoX += 1;
                this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                return true;
            }

            return false;
        }

        // 特殊スピン
        private bool tSpinSpecial()
        {
            // ステージ外であればfalseを返す
            if (isOutOfStage(this._activeMinoX, this._activeMinoY)) {
                return false;
            }

            // 左側に屋根がある場合
            if (this._stagePos[this._activeMinoX, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX, this._activeMinoY + 3] != 0) {
                if (this.checkAllOK(this._activeMinoX - 1, this._activeMinoY - 2)) {
                    this._activeMinoX -= 1;
                    this._activeMinoY -= 2;
                    this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                    return true;
                }
            }

            // 右側に屋根がある場合
            if (this._stagePos[this._activeMinoX + 2, this._activeMinoY + 2] != 0 || this._stagePos[this._activeMinoX + 2, this._activeMinoY + 3] != 0) {
                if (this.checkAllOK(this._activeMinoX + 1, this._activeMinoY - 2)) {
                    this._activeMinoX += 1;
                    this._activeMinoY -= 2;
                    this.setMinoStagePos(this._activeMinoX, this._activeMinoY);
                    return true;
                }
            }

            return false;
        }
    }
}
