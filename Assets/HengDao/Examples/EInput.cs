using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !FIRE_VR
namespace HengDao
{
    public class EInput
    {
        public static float GetAxis(string axis)
        {
            return Input.GetAxis(axis);
        }

        public static KeyStatus GetFireStatus()
        {
            return KeyStatus.Normal;
        }

        public static bool GetJumpStatus()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        public static bool GetSprayStatus()
        {
            return Input.GetKeyDown(KeyCode.U);
        }
    }
}

#else

using Valve.VR.InteractionSystem;
namespace HengDao
{
    class EInput
    {
        private static Hand mLeftHand;
        public static Hand leftHand
        {
            get
            {
                if (mLeftHand == null)
                {
                    mLeftHand = Player.instance.GetHand(0);
                }

                return mLeftHand;
            }
        }

        private static Hand mRightHand;
        public static Hand rightHand
        {

            get
            {
                if (mRightHand == null)
                {
                    mRightHand = Player.instance.GetHand(1);
                }

                return mRightHand;
            }
        }
        public enum TouchPoint
        {
            Left = 0,
            Right,
            Up,
            Down
        }

        private static TouchPoint GetTouchPadPoint(Hand hand)
        {
            //方法返回一个坐标 接触圆盘位置  
            Vector2 cc = hand.controller.GetAxis();
            // 例子：圆盘分成上下左右  
            float angle = VectorAngle(new Vector2(1, 0), cc);
            //下  
            if (angle > 45 && angle < 135)
            {
                return TouchPoint.Down;
            }
            //上  
            else if (angle < -45 && angle > -135)
            {
                return TouchPoint.Up;
            }
            //左  
            else if ((angle < 180 && angle > 135) || (angle < -135 && angle > -180))
            {
                return TouchPoint.Left;
            }
            //右  
            else if ((angle > 0 && angle < 45) || (angle > -45 && angle < 0))
            {
                return TouchPoint.Right;
            }
            else
            {
                return TouchPoint.Left;
            }
        }

        //这个函数输入两个二维向量会返回一个夹角 180 到 -180  
        static float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;
            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            return cross.z > 0 ? -angle : angle;
        }

        public static float GetAxis(string name)
        {
            Hand hand = leftHand;
            if (hand == null || hand.controller == null)
            {
                hand = rightHand;
                if (hand == null || hand.controller == null)
                {
                    return 0;
                }
            }

            if (!hand.controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                return 0;
            }

            TouchPoint point = GetTouchPadPoint(hand);
            if (name == InputAxisName.Horizontal)
            {
                if (point == TouchPoint.Left)
                {
                    return -1;
                }
                else if (point == TouchPoint.Right)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else if (name == InputAxisName.Vertical)
            {
                if (point == TouchPoint.Down)
                {
                    return -1;
                }
                else if (point == TouchPoint.Up)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public static KeyStatus GetFireStatus()
        {
            Hand hand = rightHand;
            if (hand == null || hand.controller == null)
            {
                return KeyStatus.Normal;
            }

            if (hand.controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
            {
                return KeyStatus.Down;
            }

            if (hand.controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
            {
                return KeyStatus.Up;
            }

            return KeyStatus.Normal;
        }

        public static bool GetQuitButtonDown()
        {
            bool res = false;
            if (leftHand == null || leftHand.controller == null)
            {
                res |= false;
            }
            else
            {
                res |= leftHand.controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
            }


            if (rightHand == null || rightHand.controller == null)
            {
                res |= false;
            }
            else
            {
                res |= rightHand.controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu);
            }

            return res;
        }
    }
}

#endif

namespace HengDao
{
    public enum KeyStatus
    {
        Normal = 0,
        Down,
        Up,
    }

    public class InputAxisName
    {
        public const string Horizontal = "Horizontal";
        public const string Vertical = "Vertical";
    }
}