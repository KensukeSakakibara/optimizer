using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SuisuiTetris
{
    public class HoldMinoAction : MonoBehaviour
    {
        private int _holdMinoType;

        public void InitHoldMino()
        {
            this._holdMinoType = 0;

            this.transform.Find("IminoBase").gameObject.SetActive(false);
            this.transform.Find("JminoBase").gameObject.SetActive(false);
            this.transform.Find("LminoBase").gameObject.SetActive(false);
            this.transform.Find("OminoBase").gameObject.SetActive(false);
            this.transform.Find("SminoBase").gameObject.SetActive(false);
            this.transform.Find("TminoBase").gameObject.SetActive(false);
            this.transform.Find("ZminoBase").gameObject.SetActive(false);
        }

        // ミノをホールドする
        public int SetHold(int minoType)
        {
            int prevMinoType = this._holdMinoType;
            this._holdMinoType = minoType;
            this.redrawHold();
            return prevMinoType;
        }

        private void redrawHold()
        {
            this.transform.Find("IminoBase").gameObject.SetActive(false);
            this.transform.Find("JminoBase").gameObject.SetActive(false);
            this.transform.Find("LminoBase").gameObject.SetActive(false);
            this.transform.Find("OminoBase").gameObject.SetActive(false);
            this.transform.Find("SminoBase").gameObject.SetActive(false);
            this.transform.Find("TminoBase").gameObject.SetActive(false);
            this.transform.Find("ZminoBase").gameObject.SetActive(false);

            switch (this._holdMinoType) {
                case Constants.MINO_TYPE_I:
                    this.transform.Find("IminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_J:
                    this.transform.Find("JminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_L:
                    this.transform.Find("LminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_O:
                    this.transform.Find("OminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_S:
                    this.transform.Find("SminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_T:
                    this.transform.Find("TminoBase").gameObject.SetActive(true);
                    break;
                case Constants.MINO_TYPE_Z:
                    this.transform.Find("ZminoBase").gameObject.SetActive(true);
                    break;
            }
        }
    }
}
