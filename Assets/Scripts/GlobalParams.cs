using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SuisuiTetris
{
    static class Constants
    {
        public const float BOX_SIZE = 30.0f;
        public const float DROP_WAIT = 0.4f;

        // ミノの番号を定義
        public const int MINO_TYPE_I = 1;
        public const int MINO_TYPE_J = 2;
        public const int MINO_TYPE_L = 3;
        public const int MINO_TYPE_O = 4;
        public const int MINO_TYPE_S = 5;
        public const int MINO_TYPE_T = 6;
        public const int MINO_TYPE_Z = 7;

        // ゲームの番号を定義
        public const int GAME_MODE_NOMAL = 1;
        public const int GAME_MODE_OPTIMIZE = 2;
        public const int GAME_MODE_REN = 3;
    }

    static class GlobalParams
    {
        public static int GameMode = Constants.GAME_MODE_NOMAL;
    }
}
