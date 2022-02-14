using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuisuiTetris
{
    public class NextMinoAction : MonoBehaviour
    {
        private GameObject _next0;
        private GameObject _next1;
        private GameObject _next2;
        private GameObject _next3;
        private GameObject _next4;
        private GameObject _next5;

        // ミノグループ
        private int[] _minoGroup;
        private int[] _nextMinoGroup;
        private int _minoNum;

        public void InitNextMino()
        {
            // 各nextを保持しておく
            this._next0 = this.transform.Find("Next0").gameObject;
            this._next1 = this.transform.Find("Next1").gameObject;
            this._next2 = this.transform.Find("Next2").gameObject;
            this._next3 = this.transform.Find("Next3").gameObject;
            this._next4 = this.transform.Find("Next4").gameObject;
            this._next5 = this.transform.Find("Next5").gameObject;

            // ミノグループを作成してシャッフルする
            this._minoGroup = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
            this._nextMinoGroup = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
            this._minoGroup = this._minoGroup.OrderBy(a => Guid.NewGuid()).ToArray();
            this._nextMinoGroup = this._minoGroup.OrderBy(a => Guid.NewGuid()).ToArray();
            this._minoNum = 0;
        }

        // 次のミノを取り出す
        public int GetNextMinoType()
        {
            int retMinoTypeNum = this._minoGroup[this._minoNum];

            this._minoNum++;
            if (7 <= this._minoNum) {
                this._minoGroup = this._nextMinoGroup;
                this._nextMinoGroup = this._minoGroup.OrderBy(a => Guid.NewGuid()).ToArray();
                this._minoNum = 0;
            }

            this.redrawNext();

            return retMinoTypeNum;
        }

        // Nextを再描画する
        private void redrawNext()
        {
            if (this._minoNum < 7) {
                this.setMinoType(this._next0, this._minoGroup[this._minoNum + 0]);
            } else {
                this.setMinoType(this._next0, this._nextMinoGroup[this._minoNum - 7]);
            }
            if (this._minoNum < 6) {
                this.setMinoType(this._next1, this._minoGroup[this._minoNum + 1]);
            } else {
                this.setMinoType(this._next1, this._nextMinoGroup[this._minoNum - 6]);
            }
            if (this._minoNum < 5) {
                this.setMinoType(this._next2, this._minoGroup[this._minoNum + 2]);
            } else {
                this.setMinoType(this._next2, this._nextMinoGroup[this._minoNum - 5]);
            }
            if (this._minoNum < 4) {
                this.setMinoType(this._next3, this._minoGroup[this._minoNum + 3]);
            } else {
                this.setMinoType(this._next3, this._nextMinoGroup[this._minoNum - 4]);
            }
            if (this._minoNum < 3) {
                this.setMinoType(this._next4, this._minoGroup[this._minoNum + 4]);
            } else {
                this.setMinoType(this._next4, this._nextMinoGroup[this._minoNum - 3]);
            }
            if (this._minoNum < 2) {
                this.setMinoType(this._next5, this._minoGroup[this._minoNum + 5]);
            } else {
                this.setMinoType(this._next5, this._nextMinoGroup[this._minoNum - 2]);
            }
        }

        // Nextで表示させるミノをセットする
        private void setMinoType(GameObject next, int minoType)
        {
            next.transform.Find("IminoBase").gameObject.SetActive(false);
            next.transform.Find("JminoBase").gameObject.SetActive(false);
            next.transform.Find("LminoBase").gameObject.SetActive(false);
            next.transform.Find("OminoBase").gameObject.SetActive(false);
            next.transform.Find("SminoBase").gameObject.SetActive(false);
            next.transform.Find("TminoBase").gameObject.SetActive(false);
            next.transform.Find("ZminoBase").gameObject.SetActive(false);

            switch (minoType) {
                case Constants.MINO_TYPE_I:
                    next.transform.Find("IminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_J:
                    next.transform.Find("JminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_L:
                    next.transform.Find("LminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_O:
                    next.transform.Find("OminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_S:
                    next.transform.Find("SminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_T:
                    next.transform.Find("TminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_Z:
                    next.transform.Find("ZminoBase").gameObject.SetActive(true);
                    break;
            }
        }
    }
}
