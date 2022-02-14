using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuisuiTetris
{
    public class IminoAction : MonoBehaviour
    {
        private GameObject _mino0;
        private GameObject _mino1;
        private GameObject _mino2;
        private GameObject _mino3;
        private int _directionType;
        private int[,] _minoPos;

        public void Init()
        {
            this._mino0 = this.transform.Find("Mino0").gameObject;
            this._mino1 = this.transform.Find("Mino1").gameObject;
            this._mino2 = this.transform.Find("Mino2").gameObject;
            this._mino3 = this.transform.Find("Mino3").gameObject;
            this._mino0.SetActive(true);
            this._mino1.SetActive(false);
            this._mino2.SetActive(false);
            this._mino3.SetActive(false);

            this._directionType = 0;
            this._minoPos = new int[4, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
            this._minoPos[0, 2] = Constants.MINO_TYPE_I;
            this._minoPos[1, 2] = Constants.MINO_TYPE_I;
            this._minoPos[2, 2] = Constants.MINO_TYPE_I;
            this._minoPos[3, 2] = Constants.MINO_TYPE_I;
        }

        // 回転
        public void Turn(bool rightTurn)
        {
            if (rightTurn) {
                this._directionType += 1;
                if (3 < this._directionType) {
                    this._directionType = 0;
                }
            } else {
                this._directionType -= 1;
                if (this._directionType < 0) {
                    this._directionType = 3;
                }
            }

            // 一旦初期化する
            this._minoPos = new int[4, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };

            switch (this._directionType) {
                case 0:
                    this._mino0.SetActive(true);
                    this._mino1.SetActive(false);
                    this._mino2.SetActive(false);
                    this._mino3.SetActive(false);
                    this._minoPos[0, 2] = Constants.MINO_TYPE_I;
                    this._minoPos[1, 2] = Constants.MINO_TYPE_I;
                    this._minoPos[2, 2] = Constants.MINO_TYPE_I;
                    this._minoPos[3, 2] = Constants.MINO_TYPE_I;
                    break;
                case 1:
                    this._mino0.SetActive(false);
                    this._mino1.SetActive(true);
                    this._mino2.SetActive(false);
                    this._mino3.SetActive(false);
                    this._minoPos[2, 0] = Constants.MINO_TYPE_I;
                    this._minoPos[2, 1] = Constants.MINO_TYPE_I;
                    this._minoPos[2, 2] = Constants.MINO_TYPE_I;
                    this._minoPos[2, 3] = Constants.MINO_TYPE_I;
                    break;
                case 2:
                    this._mino0.SetActive(false);
                    this._mino1.SetActive(false);
                    this._mino2.SetActive(true);
                    this._mino3.SetActive(false);
                    this._minoPos[0, 1] = Constants.MINO_TYPE_I;
                    this._minoPos[1, 1] = Constants.MINO_TYPE_I;
                    this._minoPos[2, 1] = Constants.MINO_TYPE_I;
                    this._minoPos[3, 1] = Constants.MINO_TYPE_I;
                    break;
                case 3:
                    this._mino0.SetActive(false);
                    this._mino1.SetActive(false);
                    this._mino2.SetActive(false);
                    this._mino3.SetActive(true);
                    this._minoPos[1, 0] = Constants.MINO_TYPE_I;
                    this._minoPos[1, 1] = Constants.MINO_TYPE_I;
                    this._minoPos[1, 2] = Constants.MINO_TYPE_I;
                    this._minoPos[1, 3] = Constants.MINO_TYPE_I;
                    break;
            }
        }

        // ミノのポジションを取得する
        public int[,] GetMinoPos()
        {
            return this._minoPos;
        }
    }
}
