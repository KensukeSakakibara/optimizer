using UnityEngine;
using UnityEngine.InputSystem;

namespace SuisuiTetris
{
    public class GameInputManager : MonoBehaviour
    {
        private float _holdTime;
        private float _pastHoldTime;
        private float _stepTime;
        private float _pastStepTime;

        private InputAction.CallbackContext _context;

        void Start()
        {
            this._holdTime = 0.2f;
            this._pastHoldTime = 0.0f;
            this._stepTime = 0.02f;
            this._pastStepTime = 0.0f;
        }

        void Update()
        {
            this._pastHoldTime += Time.deltaTime;
            this._pastStepTime += Time.deltaTime;

            // 左右キー長押しの処理
            if (Gamepad.current.dpad.left.isPressed || Gamepad.current.dpad.right.isPressed) {
                if (this._holdTime <= this._pastHoldTime) {
                    if (this._stepTime < this._pastStepTime) {
                        var value = this._context.ReadValue<Vector2>();
                        if (value.x == 1.0f) {
                            this.GetComponent<GameSceneManager>().HoldMoveRight();
                        }
                        if (value.x == -1.0f) {
                            this.GetComponent<GameSceneManager>().HoldMoveLeft();
                        }
                        this._pastStepTime = 0.0f;
                    }
                }
            }

            // 下キー長押しの処理
            if (Gamepad.current.dpad.down.isPressed) {
                if (this._stepTime < this._pastStepTime) {
                    var value = this._context.ReadValue<Vector2>();
                    if (value.y == -1.0f) {
                        this.GetComponent<GameSceneManager>().MoveDown();
                    }
                    this._pastStepTime = 0.0f;
                }
            }

            // キーボードのAとDの場合
            var keyboard = Keyboard.current;
            if (keyboard != null) {
                if (this._stepTime < this._pastStepTime) {
                    if (this._holdTime <= this._pastHoldTime) {
                        if (keyboard.dKey.isPressed) {
                            this.GetComponent<GameSceneManager>().HoldMoveRight();
                        }
                        if (keyboard.aKey.isPressed) {
                            this.GetComponent<GameSceneManager>().HoldMoveLeft();
                        }
                        this._pastStepTime = 0.0f;
                    }
                    if (keyboard.sKey.isPressed) {
                        this.GetComponent<GameSceneManager>().MoveDown();
                        this._pastStepTime = 0.0f;
                    }
                }
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                this._context = context;
                this._pastHoldTime = 0.0f;

                var value = context.ReadValue<Vector2>();

                if (value.x == 1.0f) {
                    this.GetComponent<GameSceneManager>().MoveRight();
                }
                if (value.x == -1.0f) {
                    this.GetComponent<GameSceneManager>().MoveLeft();
                }
                if (value.y == 1.0f) {
                    this.GetComponent<GameSceneManager>().MoveUp();
                }
                if (value.y == -1.0f) {
                    this.GetComponent<GameSceneManager>().MoveDown();
                }
            }
        }

        public void OnRightTurn(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                this.GetComponent<GameSceneManager>().TurnRight();
            }
        }

        public void OnLeftTurn(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                this.GetComponent<GameSceneManager>().TurnLeft();
            }
        }

        public void OnHold(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                this.GetComponent<GameSceneManager>().Hold();
            }
        }

        public void OnRestart(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                this.GetComponent<GameSceneManager>().Restart();
            }
        }
    }
}